// ==========================================
// COMMON
// ==========================================
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  statusCode: number;
}

// ==========================================
// USER MODELS
// ==========================================
export interface UserDto {
  id: number;
  roleId: number;
  roleName: string;
  departmentId?: number;
  departmentName?: string;
  designationId?: number;
  designationName?: string;
  fullName: string;
  email: string;
  mobile?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveUserDto {
  id?: number;
  roleId: number;
  departmentId?: number;
  designationId?: number;
  fullName: string;
  email: string;
  mobile?: string;
  password?: string;
  isActive: boolean;
  updatedBy?: number;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  userId: number;
  fullName: string;
  email: string;
  roleId: number;
  roleName: string;
  departmentId?: number;
  departmentName?: string;
  designationId?: number;
  designationName?: string;
  token?: string;
}

// ==========================================
// ROLE MODELS
// ==========================================
export interface RoleDto {
  id: number;
  roleName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveRoleDto {
  id?: number;
  roleName: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// MENU MODELS
// ==========================================
export interface MenuDto {
  id: number;
  menuName: string;
  routeUrl?: string;
  icon?: string;
  parentMenuId?: number;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  subMenus?: MenuDto[];
}

export interface SaveMenuDto {
  id?: number;
  menuName: string;
  routeUrl?: string;
  icon?: string;
  parentMenuId?: number;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// PERMISSION MODELS
// ==========================================
export interface RoleMenuPermissionDto {
  id: number;
  roleId: number;
  roleName: string;
  menuId: number;
  menuName: string;
  routeUrl?: string;
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface MenuPermissionItemDto {
  menuId: number;
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

export interface BulkSaveRolePermissionDto {
  roleId: number;
  permissions: MenuPermissionItemDto[];
  updatedBy?: number;
}

// ==========================================
// SERVICE MASTER MODELS
// ==========================================
export interface ServiceMasterDto {
  id: number;
  serviceName: string;
  description?: string;
  iconUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveServiceMasterDto {
  id?: number;
  serviceName: string;
  description?: string;
  iconUrl?: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// SETTING MODELS
// ==========================================
export interface SettingDto {
  id: number;
  settingKey: string;
  settingValue: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveSettingDto {
  id?: number;
  settingKey: string;
  settingValue: string;
  description?: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// CATEGORY & INDUSTRY MODELS
// ==========================================
export interface CategoryDto {
  id: number;
  categoryName: string;
  slug?: string;
  description?: string;
  icon?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveCategoryDto {
  id?: number;
  categoryName: string;
  slug?: string;
  description?: string;
  icon?: string;
  displayOrder?: number;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// DEPARTMENT MODELS
// ==========================================
export interface DepartmentDto {
  id: number;
  departmentName: string;
  description?: string;
  designations?: string;
  headOfDepartment?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveDepartmentDto {
  id?: number;
  departmentName: string;
  description?: string;
  designations?: string;
  headOfDepartment?: string;
  displayOrder?: number;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// DESIGNATION MODELS
// ==========================================
export interface DesignationDto {
  id: number;
  designationName: string;
  departmentId?: number;
  departmentName?: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveDesignationDto {
  id?: number;
  designationName: string;
  departmentId?: number;
  description?: string;
  displayOrder?: number;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// CLIENT & PARTNER MODELS
// ==========================================
export interface ClientDto {
  id: number;
  clientName: string;
  logoUrl?: string;
  websiteUrl?: string;
  industry?: string;
  partnerTier?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveClientDto {
  id?: number;
  clientName: string;
  logoUrl?: string;
  websiteUrl?: string;
  industry?: string;
  partnerTier?: string;
  displayOrder?: number;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// FAQ & KNOWLEDGE BASE MODELS
// ==========================================
export interface FaqDto {
  id: number;
  question: string;
  answer: string;
  category?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveFaqDto {
  id?: number;
  question: string;
  answer: string;
  category?: string;
  displayOrder?: number;
  isActive: boolean;
  updatedBy?: number;
}

// All Masters Overview
export interface AllMastersDto {
  roles: RoleDto[];
  menus: MenuDto[];
  services: ServiceMasterDto[];
  settings: SettingDto[];
  categories: CategoryDto[];
  departments: DepartmentDto[];
  designations: DesignationDto[];
  clients: ClientDto[];
  faqs: FaqDto[];
}

// ==========================================
// BLOG MODELS — Synced with API
// ==========================================
export interface BlogDto {
  id: number;
  title: string;
  slug?: string;
  summary?: string;
  content: string;
  authorId?: number;
  authorName?: string;
  category?: string;
  tags?: string;
  bannerImage?: string;
  isPublished: boolean;
  publishedAt?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveBlogDto {
  id?: number;
  title: string;
  slug?: string;
  summary?: string;
  content: string;
  authorId?: number;
  category?: string;
  tags?: string;
  bannerImage?: string;
  isPublished: boolean;
  isActive?: boolean;
  updatedBy?: number;
}

// ==========================================
// PORTFOLIO MODELS — Synced with API
// ==========================================
export interface PortfolioDto {
  id: number;
  title: string;
  clientName?: string;
  category?: string;
  projectUrl?: string;
  thumbnailUrl?: string;
  description?: string;
  technologies?: string;
  completionDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SavePortfolioDto {
  id?: number;
  title: string;
  clientName?: string;
  category?: string;
  projectUrl?: string;
  thumbnailUrl?: string;
  description?: string;
  technologies?: string;
  completionDate?: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// TEAM MEMBER MODELS — Synced with API & User Login
// ==========================================
export interface TeamMemberDto {
  id: number;
  fullName: string;
  designation: string;
  department?: string;
  email?: string;
  mobile?: string;
  profileImage?: string;
  bio?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  userId?: number;
  roleId?: number;
  roleName?: string;
  hasLoginAccess?: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveTeamMemberDto {
  id?: number;
  fullName: string;
  designation: string;
  department?: string;
  email?: string;
  mobile?: string;
  profileImage?: string;
  bio?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  enableLoginAccess?: boolean;
  roleId?: number;
  password?: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// TESTIMONIAL MODELS — Synced with API
// ==========================================
export interface TestimonialDto {
  id: number;
  clientName: string;
  clientDesignation?: string;
  companyName?: string;
  avatarUrl?: string;
  rating: number;
  feedbackText: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveTestimonialDto {
  id?: number;
  clientName: string;
  clientDesignation?: string;
  companyName?: string;
  avatarUrl?: string;
  rating: number;
  feedbackText: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// CONTACT INQUIRY MODELS — Synced with API
// ==========================================
export interface ContactInquiryDto {
  id: number;
  fullName: string;
  email: string;
  phone?: string;
  subject?: string;
  message: string;
  isResolved: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveContactInquiryDto {
  id?: number;
  fullName: string;
  email: string;
  phone?: string;
  subject?: string;
  message: string;
  isResolved?: boolean;
  updatedBy?: number;
}

// ==========================================
// AUDIT LOG MODEL
// ==========================================
export interface AuditLogDto {
  id: number;
  userId?: number;
  userName?: string;
  actionName?: string;
  tableName?: string;
  recordId?: number;
  logDetails?: string;
  iPAddress?: string;
  createdAt: string;
}

// ==========================================
// JOB POSTING MODELS — New
// ==========================================
export interface JobPostingDto {
  id: number;
  jobTitle: string;
  department?: string;
  location?: string;
  jobType?: string;
  experienceLevel?: string;
  description?: string;
  requiredSkills?: string;
  salaryRange?: string;
  lastDateToApply?: string;
  applicationCount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SaveJobPostingDto {
  id?: number;
  jobTitle: string;
  department?: string;
  location?: string;
  jobType?: string;
  experienceLevel?: string;
  description?: string;
  requiredSkills?: string;
  salaryRange?: string;
  lastDateToApply?: string;
  isActive: boolean;
  updatedBy?: number;
}

// ==========================================
// JOB APPLICATION MODELS — New
// ==========================================
export interface JobApplicationDto {
  id: number;
  jobPostingId: number;
  jobTitle?: string;
  applicantName: string;
  email: string;
  phone?: string;
  resumeUrl?: string;
  coverLetter?: string;
  status: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

export interface SaveJobApplicationDto {
  id?: number;
  jobPostingId: number;
  applicantName: string;
  email: string;
  phone?: string;
  resumeUrl?: string;
  coverLetter?: string;
  status?: string;
  notes?: string;
  updatedBy?: number;
}

// ==========================================
// DASHBOARD STATS — New
// ==========================================
export interface DashboardStatsDto {
  totalUsers: number;
  totalRoles: number;
  totalBlogs: number;
  publishedBlogs: number;
  totalPortfolios: number;
  totalTeamMembers: number;
  totalTestimonials: number;
  totalInquiries: number;
  unresolvedInquiries: number;
  activeJobPostings: number;
  totalJobApplications: number;
  recentInquiries: ContactInquiryDto[];
  recentAuditLogs: AuditLogDto[];
}
