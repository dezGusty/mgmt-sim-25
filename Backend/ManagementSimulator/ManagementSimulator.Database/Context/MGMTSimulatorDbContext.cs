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
            optionsBuilder.UseSqlServer(AppConfig.ConnectionStrings?.MGMTSimulatorDb);

            if (AppConfig.ConsoleLogQueries)
            {
                optionsBuilder.LogTo(Console.WriteLine);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .Property(lrt => lrt.Description)
                .HasMaxLength(50);

            modelBuilder.Entity<LeaveRequestType>()
                .HasIndex(lrt => lrt.Description)
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

            base.OnModelCreating(modelBuilder);

            ApplySoftDeleteFilter<Department>(modelBuilder);
            ApplySoftDeleteFilter<JobTitle>(modelBuilder);
            ApplySoftDeleteFilter<LeaveRequest>(modelBuilder);
            ApplySoftDeleteFilter<LeaveRequestType>(modelBuilder);
            ApplySoftDeleteFilter<User>(modelBuilder);
        }

        private void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => EF.Property<DateTime?>(e, "DeletedAt") == null);
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveRequestType> LeaveRequestTypes { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
