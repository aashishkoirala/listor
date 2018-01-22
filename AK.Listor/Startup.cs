/*******************************************************************************************************************************
 * AK.Listor.Startup
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

using AK.Listor.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AK.Listor
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<ListorContext>();
            services.AddScoped<UserRepository>();
            services.AddScoped<ListRepository>();
            services.AddScoped<ItemRepository>();
            services
                .AddAuthentication(o =>
                {
                    o.AddScheme(LoginConstants.CookieName, x => x.HandlerType = typeof(CookieAuthenticationHandler));
                    o.DefaultScheme = LoginConstants.CookieName;
                })
                .AddCookie(o => o.Cookie.Name = LoginConstants.CookieName);

            if (!_hostingEnvironment.IsDevelopment()) return;

            services.AddCors(o => o.AddPolicy("DefaultCorsPolicy", c => c
                .WithOrigins("http://dev.loc:3000")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowAnyHeader()
                .AllowCredentials()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware(typeof(HttpsVerifier));
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            else app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
            app.UseAuthentication();
            app.UseCors("DefaultCorsPolicy");
            app.UseMvc();
            app.UseMiddleware(typeof(AssetServer));
        }
    }
}