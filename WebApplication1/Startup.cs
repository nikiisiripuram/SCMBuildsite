//using Microsoft.AspNetCore.Authentication.Negotiate;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using WebApplication1.Data;
//using Microsoft.Extensions.Hosting;
//using System;


//namespace WebApplication1
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddControllersWithViews();

//            // Configure database context
//            services.AddDbContext<Data.ScmDBContext>(options =>
//                options.UseSqlServer(Configuration.GetConnectionString("ScmDBContext") ?? throw new InvalidOperationException("Connection string 'ScmDBContext' not found.")));

//            // Configure Windows authentication
//            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//                .AddNegotiate();

//            // Configure authorization
//            services.AddAuthorization(options =>
//            {
//                options.DefaultPolicy = new AuthorizationPolicyBuilder()
//                    .RequireAuthenticatedUser()
//                    .Build();

//                options.AddPolicy("WindowsPolicy", policy =>
//                {
//                    policy.AuthenticationSchemes.Add(NegotiateDefaults.AuthenticationScheme);
//                    policy.RequireAuthenticatedUser();
//                });
//            });

//            // Configure session
//            services.AddSession(options =>
//            {
//                options.IdleTimeout = TimeSpan.FromMinutes(2); // Set session timeout to 2 minutes
//                options.Cookie.HttpOnly = true; // Ensure cookies are HTTP only
//                options.Cookie.IsEssential = true; // Mark the session cookie as essential
//            });

//            // Add other services and configurations here
//        }

//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Home/Error");
//                app.UseHsts();
//            }

//            app.UseHttpsRedirection();
//            app.UseStaticFiles();

//            app.UseRouting();

//            // Use session middleware
//            app.UseSession();

//            // Use authentication
//            app.UseAuthentication();

//            // Use authorization
//            app.UseAuthorization();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllerRoute(
//                    name: "default",
//                    pattern: "{controller=Home}/{action=Index}/{id?}");
//            });
//        }
//    }
//}
