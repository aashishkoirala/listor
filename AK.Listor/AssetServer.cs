/*******************************************************************************************************************************
 * AK.Listor.AssetServer
 * Copyright © 2017 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of Aashish Koirala's Listor.
 *  
 * Listor is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Listor is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Listor.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace AK.Listor
{
    public class AssetServer
    {
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, Asset> _assets = new Dictionary<string, Asset>();
        private DateTime _lastModified;
        private string _lastModifiedText;

        private static readonly IDictionary<string, string> ContentTypesByExtension = new Dictionary<string, string>
        {
            {".html", "text/html"},
            {".js", "application/javascript"},
            {".css", "text/css"},
            {".ico", "image/x-icon"},
            {".png", "image/png"},
            {".svg", "image/svg+xml"},
            {".ttf", "application/x-font-truetype"},
            {".otf", "application/x-font-opentype"},
            {".woff", "application/font-woff"},
            {".woff2", "application/font-woff2"},
            {".eot", "application/vnd.ms-fontobject"},
            {".json", "application/json"}
        };

        public AssetServer(RequestDelegate next)
        {
            _next = next;
            BuildAssetDictionary();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method != "GET" || !context.Request.Path.HasValue ||
                !context.Request.Path.Value.StartsWith("/"))
            {
                await _next.Invoke(context);
                return;
            }

            var path = context.Request.Path.Value.ToLower();
            if (path == "/index.html")
            {
                await _next.Invoke(context);
                return;
            }

            var isRoot = false;
            if (path == "/")
            {
                path = "/index.html";
                isRoot = true;
            }
            path = path.TrimStart('/').ToLower().Replace('/', '.');

            if (!_assets.TryGetValue(path, out Asset content))
            {
                await _next.Invoke(context);
                return;
            }

            var extension = Path.GetExtension(path);
            if (!ContentTypesByExtension.TryGetValue(extension, out string contentType))
                contentType = "application/octet-stream";

            var isAuthenticated = (context.User?.Identity as ClaimsIdentity)?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                await context.ChallengeAsync(LoginConstants.CookieName);
                return;
            }

            if (!isRoot && IsNotModified(context)) return;

            if (isRoot)
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
                context.Response.Headers["Expires"] = "Tue, 01 Jan 1980 1:00:00 GMT";
                context.Response.Headers["Pragma"] = "no-cache";
            }

            var (gzip, deflate) = IsCompressionRequested(context.Request);
            var data = gzip ? content.GZipCompressed : (deflate ? content.DeflateCompressed : content.Uncompressed);
            if (gzip) context.Response.Headers.Add("Content-Encoding", new[] {"gzip"});
            else if (deflate) context.Response.Headers.Add("Content-Encoding", new[] {"deflate"});

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;
            context.Response.ContentLength = data.Length;
            await context.Response.Body.WriteAsync(data, 0, data.Length);
        }

        private void BuildAssetDictionary()
        {
            const string prefix = "AK.Listor.Client.";

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resource in assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(prefix)))
            {
                var key = resource.Substring(prefix.Length).ToLower();
                using (var targetStream = new MemoryStream())
                {
                    using (var sourceStream = assembly.GetManifestResourceStream(resource))
                        sourceStream.CopyTo(targetStream);
                    _assets[key] = new Asset {Uncompressed = targetStream.ToArray()};
                }
            }

            foreach (var asset in _assets.Values)
            {
                asset.GZipCompressed = GZipCompress(asset.Uncompressed);
                asset.DeflateCompressed = DeflateCompress(asset.Uncompressed);
            }

            var lastModified = new FileInfo(assembly.Location).LastWriteTime;
            _lastModified = new DateTime(lastModified.Year, lastModified.Month, lastModified.Day,
                lastModified.Hour, lastModified.Minute, lastModified.Second, lastModified.Kind);
            _lastModifiedText = _lastModified.ToString("r", CultureInfo.InvariantCulture);
        }

        private static byte[] GZipCompress(byte[] content)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    zipStream.Write(content, 0, content.Length);
                }
                return memoryStream.ToArray();
            }
        }

        private static byte[] DeflateCompress(byte[] content)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(content, 0, content.Length);
                }
                return memoryStream.ToArray();
            }
        }

        private static (bool, bool) IsCompressionRequested(HttpRequest request) =>
            !request.Headers.TryGetValue("Accept-Encoding", out var headers)
                ? (false, false)
                : (headers.Any(x => x.Contains("gzip")), headers.Any(x => x.Contains("deflate")));

        private bool IsNotModified(HttpContext context)
        {
            var ifModifiedSince = context.Request.Headers["If-Modified-Since"];
            if (string.IsNullOrWhiteSpace(ifModifiedSince)) return false;

            if (!DateTime.TryParseExact(ifModifiedSince, "r", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var ifModifiedSinceDate)) return false;
            if (_lastModified > ifModifiedSinceDate) return false;

            context.Response.StatusCode = 304;
            context.Response.Headers["Last-Modified"] = _lastModifiedText;
            return true;
        }

        private class Asset
        {
            public byte[] Uncompressed { get; set; }
            public byte[] GZipCompressed { get; set; }
            public byte[] DeflateCompressed { get; set; }
        }
    }
}