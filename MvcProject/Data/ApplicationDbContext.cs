using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models.Domain;

namespace MvcProject.Data
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { 
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Configure Enum to String conversions (To match NVARCHAR constraints)
            builder.Entity<ProjectTask>()
                .Property(t => t.Status)
                .HasConversion<string>();

            builder.Entity<ProjectTask>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            builder.Entity<ProjectUser>()
                .Property(pu => pu.Role)
                .HasConversion<string>();

            builder.Entity<AuditLog>()
                .Property(a => a.ActionType)
                .HasConversion<string>();

            builder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>();

            // 2. Composite Key for ProjectUser
            builder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            // 3. Configure ALL relationships and ON DELETE behaviors to prevent cascade cycles

            // Project -> CreatedBy (Owner)
            builder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany()
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectTask -> Assignee (sets to NULL if user deleted)
            builder.Entity<ProjectTask>()
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            // ProjectUser -> User
            builder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProjectUser -> Project
            builder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pu => pu.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskComment -> User
            builder.Entity<TaskComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // AuditLog -> User (sets to NULL if user deleted)
            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification -> Recipient User
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> Sender User (NoAction to avoid cascade cycles)
            builder.Entity<Notification>()
                .HasOne(n => n.SenderUser)
                .WithMany()
                .HasForeignKey(n => n.SenderUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Attachment configuration
            builder.Entity<Attachment>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.OriginalFileName).HasMaxLength(260).IsRequired();
                b.Property(a => a.StoredFileName).HasMaxLength(260).IsRequired();
                b.Property(a => a.ContentType).HasMaxLength(100).IsRequired();
                b.Property(a => a.Size).IsRequired();
                b.Property(a => a.UploadedAt).IsRequired();

                // FK to ProjectTask
                b.HasOne(a => a.ProjectTask)
                    .WithMany(t => t.Attachments)
                    .HasForeignKey(a => a.ProjectTaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
