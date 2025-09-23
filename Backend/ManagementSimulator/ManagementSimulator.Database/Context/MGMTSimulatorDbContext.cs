using ManagementSimulator.Database.Entities;
using ManagementSimulator.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Context
{

    public class MGMTSimulatorDbContext : DbContext
    {
        public MGMTSimulatorDbContext(DbContextOptions<MGMTSimulatorDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(AppConfig.ConnectionStrings?.MGMTSimulatorDb);

                if (AppConfig.ConsoleLogQueries)
                {
                    optionsBuilder.LogTo(Console.WriteLine);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeRole>()
                .Property(er => er.Rolename)
                .HasMaxLength(50);
            modelBuilder.Entity<EmployeeRole>()
                .HasIndex(er => er.Rolename)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(50);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .Property(d => d.Name)
                .HasMaxLength(30);
            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<JobTitle>()
                .Property(jt => jt.Name)
                .HasMaxLength(50);
            modelBuilder.Entity<JobTitle>()
                .HasIndex(jt => jt.Name)
                .IsUnique();
            modelBuilder.Entity<LeaveRequestType>()
                .Property(lrt => lrt.Title)
                .HasMaxLength(50);

            modelBuilder.Entity<LeaveRequestType>()
                .HasIndex(e => new { e.IsPaid, e.Title })
                .IsUnique();

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.User)
                .WithMany(u => u.LeaveRequests)
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Reviewer)
                .WithMany(u => u.ReviewedRequests)
                .HasForeignKey(lr => lr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.LeaveRequestType)
                .WithMany(lrt => lrt.LeaveRequests)
                .HasForeignKey(lr => lr.LeaveRequestTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeManager>(entity =>
            {
                entity.HasKey(em => new { em.EmployeeId, em.ManagerId });

                entity.HasOne(em => em.Employee)
                      .WithMany(u => u.Managers)
                      .HasForeignKey(em => em.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(em => em.Manager)
                      .WithMany(u => u.Subordinates)
                      .HasForeignKey(em => em.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeRoleUser>()
                .HasKey(eru => new { eru.RolesId, eru.UsersId });
            modelBuilder.Entity<EmployeeRoleUser>()
                .HasOne(eru => eru.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(eru => eru.RolesId);
            modelBuilder.Entity<EmployeeRoleUser>()
                .HasOne(eru => eru.User)
                .WithMany(u => u.Roles)
                .HasForeignKey(eru => eru.UsersId);

            modelBuilder.Entity<SecondManager>(entity =>
            {
                entity.HasKey(sm => new { sm.SecondManagerEmployeeId, sm.ReplacedManagerId, sm.StartDate });

                entity.HasOne(sm => sm.SecondManagerEmployee)
                      .WithMany()
                      .HasForeignKey(sm => sm.SecondManagerEmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sm => sm.ReplacedManager)
                      .WithMany()
                      .HasForeignKey(sm => sm.ReplacedManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(sm => sm.StartDate)
                      .IsRequired();

                entity.Property(sm => sm.EndDate)
                      .IsRequired();
            });

            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasKey(up => new { up.UserId, up.ProjectId });

                entity.HasOne(up => up.User)
                      .WithMany(u => u.UserProjects)
                      .HasForeignKey(up => up.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(up => up.Project)
                      .WithMany(p => p.UserProjects)
                      .HasForeignKey(up => up.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Project>()
                .Property(p => p.Name)
                .HasMaxLength(100);
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<PublicHoliday>()
                .Property(ph => ph.Name)
                .HasMaxLength(100);
            modelBuilder.Entity<PublicHoliday>()
                .HasIndex(ph => new { ph.Name, ph.Date })
                .IsUnique();

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                // Indexes for efficient querying
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.EntityType);
                entity.HasIndex(a => a.EntityId);
                entity.HasIndex(a => a.Action);
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => new { a.EntityType, a.EntityId });
                entity.HasIndex(a => new { a.UserId, a.Timestamp });

                // Foreign key relationships
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.OriginalUser)
                      .WithMany()
                      .HasForeignKey(a => a.OriginalUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveRequestType> LeaveRequestTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EmployeeManager> EmployeeManagers { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<EmployeeRoleUser> EmployeeRolesUsers { get; set; }
        public DbSet<SecondManager> SecondManagers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
