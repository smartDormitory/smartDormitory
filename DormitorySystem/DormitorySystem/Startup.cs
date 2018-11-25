﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DormitorySystem.Services;
using DormitorySystem.Data.Context;
using DormitorySystem.Data.Models;
using Newtonsoft.Json.Serialization;
using DormitorySystem.Services.Abstractions;
using Utilities.WebProvider;
using Utilities.Abstractions;

namespace DormitorySystem
{
    public class Startup
    {

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            this.Environment = env;
        }

        private IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            this.RegisterData(services);
            this.RegisterAuthentication(services);
            this.RegisterServices(services);
            this.RegisterInfrastructure(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "admin",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
                routes.MapRoute(
                name: "private",
                template: "{area:exists}/{controller=Sensors}/{action=ViewSensors}/{id?}"
                );
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

        private void RegisterInfrastructure(IServiceCollection services)
        {
            services.AddMemoryCache();

            services
               .AddMvc()
               .AddJsonOptions(options =>
                   options
                   .SerializerSettings
                   .ContractResolver = new DefaultContractResolver());

            services.AddMvc();
        }

        // Add application services.
        private void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<IUserSensorService, UserSensorService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHostedService<TimedHostedService>();
            services.AddScoped<IApiProvider, ApiProvider>();
            services.AddScoped<IICBApiService, ICBApiService>();
        }

        private void RegisterAuthentication(IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>()
                           .AddEntityFrameworkStores<DormitorySystemContext>()
                           .AddDefaultTokenProviders();

            if (this.Environment.IsDevelopment())
            {
                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 3;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;

                });
            }
        }

        private void RegisterData(IServiceCollection services)
        {
            services.AddDbContext<DormitorySystemContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
