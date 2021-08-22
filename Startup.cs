using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace DotNetCoreWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNetCoreWebApi", Version = "v1" });
            });
            services.AddCors(options => {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy => {
                    policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddIdentityServer()//配置id4
                .AddInMemoryApiResources(OAuthConfig.GetApiResources())//配置允许请求的api
                .AddInMemoryClients(OAuthConfig.GetClients())//配置允许哪些client请求
                .AddDeveloperSigningCredential()
                .AddResourceOwnerValidator<MyUserValidator>(); //User验证接口
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("default");
            //OAuth
            app.UseIdentityServer();

            //开发模式
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNetCoreWebApi v1"));
            }
            //使用https
            app.UseHttpsRedirection();
            //路由
            app.UseRouting();
            //身份认证
            app.UseAuthorization();
            //端点
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
