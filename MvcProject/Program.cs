using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcProject.Authorization.Handlers;
using MvcProject.Authorization.Requirements;
using MvcProject.Data;
using MvcProject.Hubs;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Implementations;
using MvcProject.Repositories.Interfaces;
using MvcProject.Services;
using MvcProject.Services.Interfaces;

namespace MvcProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
             
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services
            //    .AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

       //     builder.Services.AddDataProtection()
       ////  .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Keys")) // any folder on your machine
       //   .SetApplicationName("Velocity");
          
            builder.Services
               .AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(20);
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireProjectAdmin", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Admin)));

                options.AddPolicy("RequireProjectManager", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Manager)));

                options.AddPolicy("RequireProjectMember", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Member)));
            });

            builder.Services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();

            builder.Services.AddScoped<IAttachmentService, AttachmentService>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            builder.Services.AddAuthentication()
       .AddGoogle(options =>
       {
           options.CallbackPath = "/signin-google";
           options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
           options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
       });


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            //
            builder.Services.Configure<EmailSettings>(
               builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddTransient<EmailService>();




            var app = builder.Build();

            
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapHub<TaskHub>("/hubs/task");
            app.MapHub<CommentHub>("/hubs/comment");
            app.MapHub<NotificationHub>("/hubs/notification");

            app.Run();
        }
    }
}
