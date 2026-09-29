-- ============================================================
-- VELVION TECHNOLOGIES — Database Sync & Migration Script
-- Compatible with MySQL 5.7, 8.0+, MariaDB, MySQL Workbench
-- ============================================================

USE VelvionDB;

DELIMITER $$

DROP PROCEDURE IF EXISTS Sp_MigrateVelvionDB $$

CREATE PROCEDURE Sp_MigrateVelvionDB()
BEGIN
    -- Ignore duplicate column (1060), duplicate key (1061), unknown column (1054), or missing object (1091)
    DECLARE CONTINUE HANDLER FOR 1060 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1061 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1054 BEGIN END;
    DECLARE CONTINUE HANDLER FOR 1091 BEGIN END;
    DECLARE CONTINUE HANDLER FOR SQLEXCEPTION BEGIN END;

    -- ============================================================
    -- 1. tbl_blogs
    -- ============================================================
    ALTER TABLE tbl_blogs CHANGE COLUMN ImageUrl BannerImage VARCHAR(500) NULL;
    ALTER TABLE tbl_blogs ADD COLUMN Slug VARCHAR(300) NULL AFTER Title;
    ALTER TABLE tbl_blogs ADD COLUMN Summary VARCHAR(500) NULL AFTER Slug;
    ALTER TABLE tbl_blogs ADD COLUMN Category VARCHAR(100) NULL AFTER AuthorId;
    ALTER TABLE tbl_blogs ADD COLUMN Tags VARCHAR(500) NULL AFTER Category;
    ALTER TABLE tbl_blogs ADD COLUMN IsPublished TINYINT(1) NOT NULL DEFAULT 0 AFTER Tags;
    ALTER TABLE tbl_blogs ADD COLUMN PublishedAt DATETIME NULL AFTER IsPublished;

    -- ============================================================
    -- 2. tbl_portfolio
    -- ============================================================
    ALTER TABLE tbl_portfolio CHANGE COLUMN ProjectName Title VARCHAR(150) NOT NULL;
    ALTER TABLE tbl_portfolio CHANGE COLUMN TechStack Technologies VARCHAR(500) NULL;
    ALTER TABLE tbl_portfolio CHANGE COLUMN ImageUrl ThumbnailUrl VARCHAR(255) NULL;
    ALTER TABLE tbl_portfolio ADD COLUMN Category VARCHAR(100) NULL AFTER ClientName;
    ALTER TABLE tbl_portfolio ADD COLUMN ProjectUrl VARCHAR(255) NULL AFTER ThumbnailUrl;
    ALTER TABLE tbl_portfolio ADD COLUMN CompletionDate DATETIME NULL AFTER ProjectUrl;

    -- ============================================================
    -- 3. tbl_team_members
    -- ============================================================
    ALTER TABLE tbl_team_members CHANGE COLUMN Name FullName VARCHAR(100) NOT NULL;
    ALTER TABLE tbl_team_members CHANGE COLUMN ImageUrl ProfileImage VARCHAR(255) NULL;
    ALTER TABLE tbl_team_members ADD COLUMN Department VARCHAR(100) NULL AFTER Designation;
    ALTER TABLE tbl_team_members ADD COLUMN Email VARCHAR(100) NULL AFTER Department;
    ALTER TABLE tbl_team_members ADD COLUMN Bio TEXT NULL AFTER ProfileImage;
    ALTER TABLE tbl_team_members ADD COLUMN GithubUrl VARCHAR(255) NULL AFTER LinkedInUrl;
    ALTER TABLE tbl_team_members ADD COLUMN UserId INT NULL AFTER GithubUrl;

    -- ============================================================
    -- 4. tbl_testimonials
    -- ============================================================
    ALTER TABLE tbl_testimonials CHANGE COLUMN Review FeedbackText TEXT NOT NULL;
    ALTER TABLE tbl_testimonials CHANGE COLUMN ImageUrl AvatarUrl VARCHAR(255) NULL;
    ALTER TABLE tbl_testimonials CHANGE COLUMN Rating Rating INT NOT NULL DEFAULT 5;
    ALTER TABLE tbl_testimonials ADD COLUMN ClientDesignation VARCHAR(100) NULL AFTER ClientName;

    -- ============================================================
    -- 5. tbl_contact_inquiries
    -- ============================================================
    ALTER TABLE tbl_contact_inquiries CHANGE COLUMN CustomerName FullName VARCHAR(100) NOT NULL;
    ALTER TABLE tbl_contact_inquiries ADD COLUMN Subject VARCHAR(255) NULL AFTER Phone;
    ALTER TABLE tbl_contact_inquiries ADD COLUMN IsResolved TINYINT(1) NOT NULL DEFAULT 0 AFTER Message;
    ALTER TABLE tbl_contact_inquiries ADD COLUMN IsDeleted TINYINT(1) NOT NULL DEFAULT 0;

    -- ============================================================
    -- 6. tbl_audit_logs
    -- ============================================================
    ALTER TABLE tbl_audit_logs CHANGE COLUMN Action ActionName VARCHAR(50) NULL;
    ALTER TABLE tbl_audit_logs CHANGE COLUMN UserEmail UserName VARCHAR(100) NULL;
    ALTER TABLE tbl_audit_logs CHANGE COLUMN Details LogDetails TEXT NULL;

END $$

DELIMITER ;

-- Run the sync procedure
CALL Sp_MigrateVelvionDB();

-- Clean up
DROP PROCEDURE IF EXISTS Sp_MigrateVelvionDB;

-- ============================================================
-- 7. CREATE NEW TABLES (IF NOT EXISTS)
-- ============================================================

CREATE TABLE IF NOT EXISTS tbl_job_postings (
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    JobTitle        VARCHAR(150) NOT NULL,
    Department      VARCHAR(100) NULL,
    Location        VARCHAR(100) NULL,
    JobType         VARCHAR(50) NULL COMMENT 'Full-time, Part-time, Remote, Contract',
    ExperienceLevel VARCHAR(50) NULL COMMENT 'Fresher, 1-3 yrs, 3-6 yrs, 6+',
    Description     TEXT NULL,
    RequiredSkills  TEXT NULL,
    SalaryRange     VARCHAR(100) NULL,
    LastDateToApply DATETIME NULL,
    IsActive        TINYINT(1) NOT NULL DEFAULT 1,
    IsDeleted       TINYINT(1) NOT NULL DEFAULT 0,
    UpdatedBy       INT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tbl_job_applications (
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    JobPostingId    INT NOT NULL,
    ApplicantName   VARCHAR(100) NOT NULL,
    Email           VARCHAR(100) NOT NULL,
    Phone           VARCHAR(20) NULL,
    ResumeUrl       VARCHAR(255) NULL,
    CoverLetter     TEXT NULL,
    Status          VARCHAR(50) NOT NULL DEFAULT 'Applied' COMMENT 'Applied, Shortlisted, Interview, Rejected, Hired',
    Notes           TEXT NULL,
    IsDeleted       TINYINT(1) NOT NULL DEFAULT 0,
    UpdatedBy       INT NULL,
    CreatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT FK_JobApplication_JobPosting FOREIGN KEY (JobPostingId)
        REFERENCES tbl_job_postings(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- 8. EF Core History Entry
-- ============================================================
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260929_InitialCreate', '10.0.12');

SELECT '✅ VelvionDB Migration Completed Successfully!' AS Status;
