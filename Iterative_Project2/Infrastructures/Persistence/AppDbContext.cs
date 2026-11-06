using Iterative_Project2.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iterative_Project2.Infrastructures.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Company> Companies { get; set; }

        // Candidate Profile
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Experience> Experiences { get; set; }

        // Job Management
        public DbSet<Job> Jobs { get; set; }

        // Application Management
        public DbSet<JobApplication> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Table / schema configuration ----------
            modelBuilder.Entity<User>().ToTable("Users", "Project2");
            modelBuilder.Entity<Candidate>().ToTable("Candidates", "Project2");
            modelBuilder.Entity<Company>().ToTable("Companies", "Project2");
            modelBuilder.Entity<Skill>().ToTable("Skills", "Project2");
            modelBuilder.Entity<Education>().ToTable("Educations", "Project2");
            modelBuilder.Entity<Experience>().ToTable("Experiences", "Project2");
            modelBuilder.Entity<Job>().ToTable("Jobs", "Project2");
            modelBuilder.Entity<JobApplication>().ToTable("Applications", "Project2");

            // ---------- Property configuration ----------

            // User: unique login identifiers, role stored as text
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.Username).IsUnique();
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
            });

            // Skill: no duplicate skill name per candidate, level stored as text
            modelBuilder.Entity<Skill>(e =>
            {
                e.HasIndex(s => new { s.CandidateId, s.Name }).IsUnique();
                e.Property(s => s.ProficiencyLevel).HasConversion<string>().HasMaxLength(30);
            });

            // Experience: employment type stored as text
            modelBuilder.Entity<Experience>()
                .Property(x => x.EmploymentType)
                .HasConversion<string>()
                .HasMaxLength(30);

            // Job: enums stored as text, index for the "open jobs" listing query
            modelBuilder.Entity<Job>(e =>
            {
                e.Property(j => j.JobPosition).HasConversion<string>().HasMaxLength(30);
                e.Property(j => j.JobType).HasConversion<string>().HasMaxLength(30);
                e.HasIndex(j => new { j.IsActive, j.ApplicationDeadline });
            });

            // JobApplication: status stored as text
            modelBuilder.Entity<JobApplication>()
                .Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            // ---------- Relationships ----------

            // 1. User - Candidate (One-to-One, shared primary key)
            // Candidate.Id is both Primary Key and Foreign Key
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.User)
                .WithOne(u => u.Candidate)
                .HasForeignKey<Candidate>(c => c.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. User - Company (One-to-Many)
            modelBuilder.Entity<Company>()
                .HasOne(c => c.OwnerUser)
                .WithMany(u => u.OwnedCompanies)
                .HasForeignKey(c => c.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Company - Job (One-to-Many)
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Jobs)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Candidate - Skill (One-to-Many)
            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Skills)
                .WithOne(s => s.Candidate)
                .HasForeignKey(s => s.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Candidate - Education (One-to-Many)
            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Educations)
                .WithOne(e => e.Candidate)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Candidate - Experience (One-to-Many)
            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Experiences)
                .WithOne(e => e.Candidate)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // 7. Candidate - Application (One-to-Many)
            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Applications)
                .WithOne(a => a.Candidate)
                .HasForeignKey(a => a.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Job - Application (One-to-Many)
            modelBuilder.Entity<Job>()
                .HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. A candidate can apply to a job only once
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => new { a.CandidateId, a.JobId })
                .IsUnique();
        }
    }
}