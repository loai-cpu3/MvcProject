using Microsoft.EntityFrameworkCore;
using MvcProject.Data;
using MvcProject.Repositories.Interfaces;
using MvcProject.Repositories.Implementations;
using MvcProject.Authorization.Requirements;

namespace MvcProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireProjectAdmin", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Admin)));

                options.AddPolicy("RequireProjectManager", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Manager)));

                options.AddPolicy("RequireProjectMember", policy =>
                    policy.Requirements.Add(new ProjectRequirement(ProjectRole.Member)));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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

            app.Run();
        }
    }
}
