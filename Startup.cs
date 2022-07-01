using Authentication.IdentityServer.WebAPI.Models;
using Authentication.IdentityServer.WebAPI.Services;
using Authentication.IdentityServer.WebAPI.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using SigningServer_TedaSign.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Authentication.IdentityServer.WebAPI
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            var apiName = Configuration.GetSection("IDPSettings")["ApiName"];
            var authority = Configuration.GetSection("IDPSettings")["Authority"];

            services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication("Bearer", options =>
            {
                options.ApiName = apiName;
                options.Authority = authority;

            });

            //DB Setting
            services.Configure<ConnectionStrings>(
                Configuration.GetSection(nameof(ConnectionStrings)));

            services.AddSingleton<IDatabaseSettings>(provider =>
                provider.GetRequiredService<IOptions<ConnectionStrings>>().Value);

            //LDAP Setting
            services.Configure<LDAPSettings>(
                Configuration.GetSection(nameof(LDAPSettings)));

            services.AddSingleton<ILDAPSettings>(provider =>
                provider.GetRequiredService<IOptions<LDAPSettings>>().Value);

            //SMTP Setting
            services.Configure<SMTPSettings>(
                Configuration.GetSection(nameof(SMTPSettings)));

            services.AddSingleton<ISMTPSettings>(provider =>
                provider.GetRequiredService<IOptions<SMTPSettings>>().Value);

            //Fido Setting
            services.Configure<FidoSettings>(
                Configuration.GetSection(nameof(FidoSettings)));

            services.AddSingleton<IFidoSettings>(provider =>
                provider.GetRequiredService<IOptions<FidoSettings>>().Value);

            services.AddScoped<UserService>();

            services.AddScoped<AdminService>();

            services.AddControllers();

            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication.IdentityServer.WebAPI", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication.IdentityServer.WebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
