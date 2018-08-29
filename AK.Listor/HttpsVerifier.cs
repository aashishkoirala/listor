/*******************************************************************************************************************************
 * AK.Listor.HttpsVerifier
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

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AK.Listor
{
    public class HttpsVerifier
    {
        private readonly RequestDelegate _next;
        private static readonly IDictionary<string, string> Map = new Dictionary<string, string>();

        public HttpsVerifier(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains(".well-known/acme-challenge/"))
            {
                var parts = context.Request.Path.Value.Split('/');
                var key = parts[parts.Length - 1];
                if (!Map.TryGetValue(key, out string value)) value = "Not found";
                await context.Response.WriteAsync(value);
                return;
            }

            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains(".well-known/set-acme-challenge/"))
            {
                var parts = context.Request.Path.Value.Split('/');
                var key = parts[parts.Length - 2];
                var value = parts[parts.Length - 1];
                Map[key] = value;
                await context.Response.WriteAsync("Value put!");
                return;
            }

            await _next.Invoke(context);
        }
    }
}