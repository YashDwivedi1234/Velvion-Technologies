using Microsoft.EntityFrameworkCore;
using velvion_API.Entities;

namespace velvion_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Master Tables
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<ServiceMaster> Services => Set<ServiceMaster>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<CategoryMaster> Categories => Set<CategoryMaster>();
    public DbSet<DepartmentMaster> Departments => Set<DepartmentMaster>();
    public DbSet<DesignationMaster> Designations => Set<DesignationMaster>();
    public DbSet<ClientMaster> Clients => Set<ClientMaster>();
    public DbSet<FaqMaster> Faqs => Set<FaqMaster>();

    // Application Tables
    public DbSet<User> Users => Set<User>();
    public DbSet<RoleMenuPermission> RoleMenuPermissions => Set<RoleMenuPermission>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Careers / ATS
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filter for soft delete on tables with IsDeleted
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Menu>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ServiceMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Setting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CategoryMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DepartmentMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DesignationMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ClientMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FaqMaster>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RoleMenuPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Blog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Portfolio>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<TeamMember>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Testimonial>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ContactInquiry>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<JobPosting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<JobApplication>().HasQueryFilter(e => !e.IsDeleted);

        // Setting unique index
        modelBuilder.Entity<Setting>()
            .HasIndex(s => s.SettingKey)
            .IsUnique();

        // User unique email index
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Blog slug unique index
        modelBuilder.Entity<Blog>()
            .HasIndex(b => b.Slug)
            .IsUnique()
            .HasFilter("Slug IS NOT NULL AND Slug != ''");

        // JobApplication → JobPosting relationship
        modelBuilder.Entity<JobApplication>()
            .HasOne(a => a.JobPosting)
            .WithMany(j => j.Applications)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
