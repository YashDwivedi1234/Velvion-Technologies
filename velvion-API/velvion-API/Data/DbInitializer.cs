using Microsoft.EntityFrameworkCore;
using velvion_API.Data;
using velvion_API.Entities;

namespace velvion_API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("🌱 Checking and seeding initial master data...");

            // 1. Seed Roles if empty
            if (!await context.Roles.IgnoreQueryFilters().AnyAsync())
            {
                await context.Roles.AddRangeAsync(
                    new Role { RoleName = "SuperAdmin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Role { RoleName = "Admin", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Role { RoleName = "HR Manager", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Role { RoleName = "Employee", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // 2. Seed Default Departments if empty
            if (!await context.Departments.IgnoreQueryFilters().AnyAsync())
            {
                var departments = new List<DepartmentMaster>
                {
                    new DepartmentMaster
                    {
                        DepartmentName = "Engineering & Architecture",
                        Description = "Software engineering, DevOps, cloud infrastructure & system architecture",
                        HeadOfDepartment = "Alex Vance",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new DepartmentMaster
                    {
                        DepartmentName = "UI/UX & Product Design",
                        Description = "User research, UI design, prototyping, UX architecture & brand systems",
                        HeadOfDepartment = "Elena Rostova",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new DepartmentMaster
                    {
                        DepartmentName = "AI & Data Science",
                        Description = "Machine learning models, GenAI solutions, data engineering & NLP analytics",
                        HeadOfDepartment = "Dr. Marcus Reed",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new DepartmentMaster
                    {
                        DepartmentName = "Project Management & Delivery",
                        Description = "Agile sprint management, technical delivery, client communication & QA",
                        HeadOfDepartment = "Sarah Jenkins",
                        DisplayOrder = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new DepartmentMaster
                    {
                        DepartmentName = "Sales & Strategic Growth",
                        Description = "Enterprise sales, business development, client partnerships & account expansion",
                        HeadOfDepartment = "David Sterling",
                        DisplayOrder = 5,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new DepartmentMaster
                    {
                        DepartmentName = "Human Resources & Talent",
                        Description = "Talent acquisition, employee engagement, HR policy & organizational development",
                        HeadOfDepartment = "Priya Sharma",
                        DisplayOrder = 6,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // 3. Seed Default Designations if empty
            if (!await context.Designations.IgnoreQueryFilters().AnyAsync())
            {
                var engDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Engineering & Architecture");
                var designDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "UI/UX & Product Design");
                var aiDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "AI & Data Science");
                var pmDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Project Management & Delivery");
                var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Sales & Strategic Growth");
                var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Human Resources & Talent");

                var designations = new List<DesignationMaster>
                {
                    // Engineering
                    new DesignationMaster { DesignationName = "Chief Technical Architect", DepartmentId = engDept?.Id, Description = "High-level software architecture and tech vision", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Lead Software Engineer", DepartmentId = engDept?.Id, Description = "Technical team leadership and code quality", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Senior Full-Stack Developer", DepartmentId = engDept?.Id, Description = "End-to-end full stack web application development", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Frontend Specialist (Angular)", DepartmentId = engDept?.Id, Description = "Modern Angular UI architecture and reactive patterns", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Backend .NET Core Engineer", DepartmentId = engDept?.Id, Description = "High-performance microservices and REST APIs", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "DevOps & Cloud Architect", DepartmentId = engDept?.Id, Description = "CI/CD pipelines, Docker, Kubernetes and Cloud ops", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "QA Automation Lead", DepartmentId = engDept?.Id, Description = "Automated testing, quality assurance and security audits", DisplayOrder = 7, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // Design
                    new DesignationMaster { DesignationName = "Head of Product Design", DepartmentId = designDept?.Id, Description = "Design vision, user experience strategy & design ops", DisplayOrder = 8, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Senior UI/UX Designer", DepartmentId = designDept?.Id, Description = "User journey mapping, wireframing & high-fidelity UI", DisplayOrder = 9, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Design System Architect", DepartmentId = designDept?.Id, Description = "Reusable design tokens, component libraries & style guides", DisplayOrder = 10, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // AI & Data
                    new DesignationMaster { DesignationName = "Principal AI Scientist", DepartmentId = aiDept?.Id, Description = "Advanced machine learning, LLM fine-tuning & research", DisplayOrder = 11, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "MLOps & Data Engineer", DepartmentId = aiDept?.Id, Description = "Data pipelines, model deployment and vector databases", DisplayOrder = 12, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // PM & Delivery
                    new DesignationMaster { DesignationName = "VP of Delivery & Operations", DepartmentId = pmDept?.Id, Description = "Client delivery management and organizational execution", DisplayOrder = 13, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Senior Scrum Master", DepartmentId = pmDept?.Id, Description = "Agile sprint facilitation and delivery management", DisplayOrder = 14, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // Sales
                    new DesignationMaster { DesignationName = "Chief Revenue Officer", DepartmentId = salesDept?.Id, Description = "Strategic revenue growth and corporate expansion", DisplayOrder = 15, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Enterprise Solutions Consultant", DepartmentId = salesDept?.Id, Description = "Client discovery, solution design & proposal closure", DisplayOrder = 16, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },

                    // HR
                    new DesignationMaster { DesignationName = "Director of People Operations", DepartmentId = hrDept?.Id, Description = "HR strategy, organizational culture and talent growth", DisplayOrder = 17, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new DesignationMaster { DesignationName = "Technical Talent Scout", DepartmentId = hrDept?.Id, Description = "Top engineering talent recruitment and onboarding", DisplayOrder = 18, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };

                await context.Designations.AddRangeAsync(designations);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("✅ Master data seeding check completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Initial data seeding skipped or database not yet created.");
        }
    }
}
