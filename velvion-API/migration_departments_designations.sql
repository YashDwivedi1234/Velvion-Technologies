-- ============================================================
-- VELVION TECHNOLOGIES — Master Tables & Schema Sync Script
-- Database: VelvionDB
-- Compatible with MySQL 5.7, 8.0+, MariaDB, MySQL Workbench
-- ============================================================

USE VelvionDB;

DELIMITER $$

DROP PROCEDURE IF EXISTS Sp_MigrateMasterTables $$

CREATE PROCEDURE Sp_MigrateMasterTables()
BEGIN
    -- Handlers for duplicate column (1060), duplicate key (1061), unknown column (1054), etc.
    DECLARE CONTINUE HANDLER FOR 1060 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1061 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1054 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1091 BEGIN END;
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;

    -- ============================================================
    -- 1. md_roles columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_roles (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        RoleName VARCHAR(50) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_roles ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_roles ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_roles ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_roles ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_roles ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 2. md_menu columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_menu (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        MenuName VARCHAR(100) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_menu ADD COLUMN RouteUrl VARCHAR(255) NULL AFTER MenuName;
    ALTER TABLE md_menu ADD COLUMN Icon VARCHAR(50) NULL AFTER RouteUrl;
    ALTER TABLE md_menu ADD COLUMN ParentMenuId INT NULL DEFAULT 0 AFTER Icon;
    ALTER TABLE md_menu ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_menu ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_menu ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_menu ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_menu ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 3. md_services columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_services (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        ServiceName VARCHAR(100) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_services ADD COLUMN Description TEXT NULL AFTER ServiceName;
    ALTER TABLE md_services ADD COLUMN IconUrl VARCHAR(255) NULL AFTER Description;
    ALTER TABLE md_services ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_services ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_services ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_services ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_services ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 4. md_settings columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_settings (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        SettingKey VARCHAR(100) NOT NULL,
        SettingValue TEXT NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_settings ADD COLUMN Description VARCHAR(255) NULL AFTER SettingValue;
    ALTER TABLE md_settings ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_settings ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_settings ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_settings ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_settings ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 5. md_categories Table & columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_categories (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        CategoryName VARCHAR(100) NOT NULL,
        Slug VARCHAR(100) NULL,
        Description VARCHAR(500) NULL,
        Icon VARCHAR(50) NULL DEFAULT 'folder',
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive TINYINT(1) NOT NULL DEFAULT 1,
        IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
        UpdatedBy INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_categories ADD COLUMN Slug VARCHAR(100) NULL AFTER CategoryName;
    ALTER TABLE md_categories ADD COLUMN Description VARCHAR(500) NULL AFTER Slug;
    ALTER TABLE md_categories ADD COLUMN Icon VARCHAR(50) NULL DEFAULT 'folder' AFTER Description;
    ALTER TABLE md_categories ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;
    ALTER TABLE md_categories ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_categories ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_categories ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_categories ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_categories ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 6. md_departments Table & columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_departments (
        Id                  INT AUTO_INCREMENT PRIMARY KEY,
        DepartmentName      VARCHAR(100) NOT NULL,
        Description         VARCHAR(500) NULL,
        Designations        VARCHAR(500) NULL,
        HeadOfDepartment    VARCHAR(100) NULL,
        DisplayOrder        INT NOT NULL DEFAULT 0,
        IsActive            TINYINT(1) NOT NULL DEFAULT 1,
        IsDeleted           TINYINT(1) NOT NULL DEFAULT 0,
        UpdatedBy           INT NULL,
        CreatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_departments ADD COLUMN Description VARCHAR(500) NULL AFTER DepartmentName;
    ALTER TABLE md_departments ADD COLUMN Designations VARCHAR(500) NULL AFTER Description;
    ALTER TABLE md_departments ADD COLUMN HeadOfDepartment VARCHAR(100) NULL AFTER Designations;
    ALTER TABLE md_departments ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;
    ALTER TABLE md_departments ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_departments ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_departments ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_departments ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_departments ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 7. md_designations Table & columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_designations (
        Id                  INT AUTO_INCREMENT PRIMARY KEY,
        DesignationName     VARCHAR(100) NOT NULL,
        DepartmentId        INT NULL,
        Description         VARCHAR(500) NULL,
        DisplayOrder        INT NOT NULL DEFAULT 0,
        IsActive            TINYINT(1) NOT NULL DEFAULT 1,
        IsDeleted           TINYINT(1) NOT NULL DEFAULT 0,
        UpdatedBy           INT NULL,
        CreatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_designations ADD COLUMN DepartmentId INT NULL AFTER DesignationName;
    ALTER TABLE md_designations ADD COLUMN Description VARCHAR(500) NULL AFTER DepartmentId;
    ALTER TABLE md_designations ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;
    ALTER TABLE md_designations ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_designations ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_designations ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_designations ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_designations ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 8. md_clients Table & columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_clients (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        ClientName VARCHAR(150) NOT NULL,
        LogoUrl VARCHAR(500) NULL,
        WebsiteUrl VARCHAR(255) NULL,
        Industry VARCHAR(100) NULL,
        PartnerTier VARCHAR(50) NULL DEFAULT 'Enterprise Client',
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive TINYINT(1) NOT NULL DEFAULT 1,
        IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
        UpdatedBy INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_clients ADD COLUMN LogoUrl VARCHAR(500) NULL AFTER ClientName;
    ALTER TABLE md_clients ADD COLUMN WebsiteUrl VARCHAR(255) NULL AFTER LogoUrl;
    ALTER TABLE md_clients ADD COLUMN Industry VARCHAR(100) NULL AFTER WebsiteUrl;
    ALTER TABLE md_clients ADD COLUMN PartnerTier VARCHAR(50) NULL DEFAULT 'Enterprise Client' AFTER Industry;
    ALTER TABLE md_clients ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;
    ALTER TABLE md_clients ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_clients ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_clients ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_clients ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_clients ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 9. md_faqs Table & columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS md_faqs (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        Question VARCHAR(300) NOT NULL,
        Answer TEXT NOT NULL,
        Category VARCHAR(100) NULL DEFAULT 'General',
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive TINYINT(1) NOT NULL DEFAULT 1,
        IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
        UpdatedBy INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE md_faqs ADD COLUMN Category VARCHAR(100) NULL DEFAULT 'General' AFTER Answer;
    ALTER TABLE md_faqs ADD COLUMN DisplayOrder INT NOT NULL DEFAULT 0;
    ALTER TABLE md_faqs ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE md_faqs ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE md_faqs ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE md_faqs ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE md_faqs ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 10. tbl_role_menu_permission columns
    -- ============================================================
    CREATE TABLE IF NOT EXISTS tbl_role_menu_permission (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        RoleId INT NOT NULL,
        MenuId INT NOT NULL,
        CanView TINYINT(1) NOT NULL DEFAULT 0,
        CanAdd TINYINT(1) NOT NULL DEFAULT 0,
        CanEdit TINYINT(1) NOT NULL DEFAULT 0,
        CanDelete TINYINT(1) NOT NULL DEFAULT 0
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE tbl_role_menu_permission ADD COLUMN CanView TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN CanAdd TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN CanEdit TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN CanDelete TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE tbl_role_menu_permission ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 11. tbl_users columns (DepartmentId, DesignationId, Mobile, Audit)
    -- ============================================================
    CREATE TABLE IF NOT EXISTS tbl_users (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        RoleId INT NOT NULL,
        FullName VARCHAR(100) NOT NULL,
        Email VARCHAR(100) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    ALTER TABLE tbl_users ADD COLUMN DepartmentId INT NULL AFTER RoleId;
    ALTER TABLE tbl_users ADD COLUMN DesignationId INT NULL AFTER DepartmentId;
    ALTER TABLE tbl_users ADD COLUMN Mobile VARCHAR(20) NULL AFTER Email;
    ALTER TABLE tbl_users ADD COLUMN IsActive TINYINT(1) NOT NULL DEFAULT 1;
    ALTER TABLE tbl_users ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;
    ALTER TABLE tbl_users ADD COLUMN UpdatedBy INT NULL;
    ALTER TABLE tbl_users ADD COLUMN CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ALTER TABLE tbl_users ADD COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

    -- ============================================================
    -- 12. Seed Default Departments
    -- ============================================================
    INSERT IGNORE INTO md_departments (Id, DepartmentName, Description, HeadOfDepartment, DisplayOrder, IsActive, IsDeleted)
    VALUES 
    (1, 'Engineering & Architecture', 'Software engineering, DevOps, cloud infrastructure & system architecture', 'Alex Vance', 1, 1, 0),
    (2, 'UI/UX & Product Design', 'User research, UI design, prototyping, UX architecture & brand systems', 'Elena Rostova', 2, 1, 0),
    (3, 'AI & Data Science', 'Machine learning models, GenAI solutions, data engineering & NLP analytics', 'Dr. Marcus Reed', 3, 1, 0),
    (4, 'Project Management & Delivery', 'Agile sprint management, technical delivery, client communication & QA', 'Sarah Jenkins', 4, 1, 0),
    (5, 'Sales & Strategic Growth', 'Enterprise sales, business development, client partnerships & account expansion', 'David Sterling', 5, 1, 0),
    (6, 'Human Resources & Talent', 'Talent acquisition, employee engagement, HR policy & organizational development', 'Priya Sharma', 6, 1, 0);

    -- ============================================================
    -- 13. Seed Default Designations
    -- ============================================================
    INSERT IGNORE INTO md_designations (Id, DesignationName, DepartmentId, Description, DisplayOrder, IsActive, IsDeleted)
    VALUES
    (1, 'Chief Technical Architect', 1, 'High-level software architecture and tech vision', 1, 1, 0),
    (2, 'Lead Software Engineer', 1, 'Technical team leadership and code quality', 2, 1, 0),
    (3, 'Senior Full-Stack Developer', 1, 'End-to-end full stack web application development', 3, 1, 0),
    (4, 'Frontend Specialist (Angular)', 1, 'Modern Angular UI architecture and reactive patterns', 4, 1, 0),
    (5, 'Backend .NET Core Engineer', 1, 'High-performance microservices and REST APIs', 5, 1, 0),
    (6, 'DevOps & Cloud Architect', 1, 'CI/CD pipelines, Docker, Kubernetes and Cloud ops', 6, 1, 0),
    (7, 'QA Automation Lead', 1, 'Automated testing, quality assurance and security audits', 7, 1, 0),
    (8, 'Head of Product Design', 2, 'Design vision, user experience strategy & design ops', 8, 1, 0),
    (9, 'Senior UI/UX Designer', 2, 'User journey mapping, wireframing & high-fidelity UI', 9, 1, 0),
    (10, 'Design System Architect', 2, 'Reusable design tokens, component libraries & style guides', 10, 1, 0),
    (11, 'Principal AI Scientist', 3, 'Advanced machine learning, LLM fine-tuning & research', 11, 1, 0),
    (12, 'MLOps & Data Engineer', 3, 'Data pipelines, model deployment and vector databases', 12, 1, 0),
    (13, 'VP of Delivery & Operations', 4, 'Client delivery management and organizational execution', 13, 1, 0),
    (14, 'Senior Scrum Master', 4, 'Agile sprint facilitation and delivery management', 14, 1, 0),
    (15, 'Chief Revenue Officer', 5, 'Strategic revenue growth and corporate expansion', 15, 1, 0),
    (16, 'Enterprise Solutions Consultant', 5, 'Client discovery, solution design & proposal closure', 16, 1, 0),
    (17, 'Director of People Operations', 6, 'HR strategy, organizational culture and talent growth', 17, 1, 0),
    (18, 'Technical Talent Scout', 6, 'Top engineering talent recruitment and onboarding', 18, 1, 0);

    -- ============================================================
    -- 14. Update Existing Users with Default Department & Designation
    -- ============================================================
    UPDATE tbl_users SET DepartmentId = 1, DesignationId = 3 WHERE (DepartmentId IS NULL OR DesignationId IS NULL) AND Email LIKE '%manu%';
    UPDATE tbl_users SET DepartmentId = 1, DesignationId = 1 WHERE (DepartmentId IS NULL OR DesignationId IS NULL) AND (Email LIKE '%admin%' OR Email LIKE '%yash@%');
    UPDATE tbl_users SET DepartmentId = 1, DesignationId = 4 WHERE (DepartmentId IS NULL OR DesignationId IS NULL) AND Email LIKE '%yashdwivedi%';
    UPDATE tbl_users SET DepartmentId = 1, DesignationId = 3 WHERE DepartmentId IS NULL;

END $$

DELIMITER ;

CALL Sp_MigrateMasterTables();

DROP PROCEDURE IF EXISTS Sp_MigrateMasterTables;

-- Direct verification query
SELECT u.Id, u.FullName, u.Email, d.DepartmentName, des.DesignationName, r.RoleName
FROM tbl_users u
LEFT JOIN md_departments d ON u.DepartmentId = d.Id
LEFT JOIN md_designations des ON u.DesignationId = des.Id
LEFT JOIN md_roles r ON u.RoleId = r.Id;

SELECT '✅ All Master Tables, Department & Designation Columns Synchronized Successfully!' AS Status;
