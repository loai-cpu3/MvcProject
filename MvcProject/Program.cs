using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MvcProject.Data;
using MvcProject.Models.Domain;
using MvcProject.Repositories.Interfaces;
using MvcProject.Repositories.Implementations;
using MvcProject.Authorization.Handlers;
using MvcProject.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
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
             
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
                

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

            builder.Services.AddScoped<IAttachmentService, AttachmentService>();


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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    
                    context.Database.EnsureCreated();

                    if (!context.Users.Any(u => u.Email == "admin@mock.com"))
                    {
                        var admin = new ApplicationUser { UserName = "admin@mock.com", Email = "admin@mock.com", FirstName = "Admin", LastName = "User" };
                        var regular = new ApplicationUser { UserName = "user@mock.com", Email = "user@mock.com", FirstName = "Regular", LastName = "User" };

                        userManager.CreateAsync(admin, "Password123!").GetAwaiter().GetResult();
                        userManager.CreateAsync(regular, "Password123!").GetAwaiter().GetResult();
                    }

                    var mockAdmin = context.Users.FirstOrDefault(u => u.Email == "admin@mock.com");
                    if (mockAdmin != null)
                    {
                        var project = context.Projects.FirstOrDefault(p => p.Title == "Mock Project");
                        if (project == null)
                        {
                            project = new Project
                            {
                                Title = "Mock Project",
                                Description = "This is a seeded mock project.",
                                CreatedById = mockAdmin.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            context.Projects.Add(project);
                            context.SaveChanges();
                        }

                        // Assign all existing users to the Mock Project so anyone logging in can see it
                        var allUsers = context.Users.ToList();
                        foreach (var user in allUsers)
                        {
                            if (!context.ProjectUsers.Any(pu => pu.ProjectId == project.Id && pu.UserId == user.Id))
                            {
                                context.ProjectUsers.Add(new ProjectUser 
                                { 
                                    ProjectId = project.Id, 
                                    UserId = user.Id, 
                                    Role = user.Id == mockAdmin.Id ? ProjectRole.Admin : ProjectRole.Member 
                                });
                            }
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            app.Run();
        }
    }
}
