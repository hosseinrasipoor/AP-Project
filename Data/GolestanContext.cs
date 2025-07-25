using Golestan.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace UniversityManager.Data
{
    public class GolestanContext : DbContext
    {
        public GolestanContext(DbContextOptions<GolestanContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Take> Takes { get; set; }
        public DbSet<Teach> Teaches { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<Take>().HasKey(t => new { t.StudentId, t.SectionId });
            modelBuilder.Entity<Teach>().HasKey(t => new { t.InstructorId, t.SectionId });

            // User - Students (one to many)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany(u => u.StudentProfiles)
                .HasForeignKey(s => s.UserId);

            // User - Instructors (one to many)
            modelBuilder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithMany(u => u.InstructorProfiles)
                .HasForeignKey(i => i.UserId);

            // UserRoles
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Takes
            modelBuilder.Entity<Take>()
                .HasOne(t => t.Student)
                .WithMany(s => s.Takes)
                .HasForeignKey(t => t.StudentId);

            modelBuilder.Entity<Take>()
                .HasOne(t => t.Section)
                .WithMany(s => s.Takes)
                .HasForeignKey(t => t.SectionId);

            // Teaches
            modelBuilder.Entity<Teach>()
                .HasOne(t => t.Instructor)
                .WithMany(i => i.Teaches)
                .HasForeignKey(t => t.InstructorId);

            modelBuilder.Entity<Teach>()
                 .HasOne(t => t.Section)
                 .WithOne(s => s.Teach)
                 .HasForeignKey<Teach>(t => t.SectionId);


            // Section relations
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId);

            modelBuilder.Entity<Section>()
                .HasOne(s => s.Classroom)
                .WithMany()
                .HasForeignKey(s => s.ClassroomId);

            modelBuilder.Entity<Section>()
                .HasOne(s => s.TimeSlot)
                .WithMany()
                .HasForeignKey(s => s.TimeSlotId);


        }
    }
}
