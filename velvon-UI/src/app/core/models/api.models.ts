export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  statusCode: number;
}

// User Models
export interface UserDto {
  id: number;
  roleId: number;
  roleName: string;
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
  token?: string;
}

// Role Models
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

// Menu Models
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

// Permission Models
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

// Service Master Models
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

// Setting Models
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

// All Masters Overview
export interface AllMastersDto {
  roles: RoleDto[];
  menus: MenuDto[];
  services: ServiceMasterDto[];
  settings: SettingDto[];
}

// Blog Models
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
  updatedBy?: number;
}

// Portfolio Models
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

// Team Member Models
export interface TeamMemberDto {
  id: number;
  fullName: string;
  designation: string;
  department?: string;
  email?: string;
  profileImage?: string;
  bio?: string;
  linkedInUrl?: string;
  githubUrl?: string;
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
  profileImage?: string;
  bio?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  isActive: boolean;
  updatedBy?: number;
}

// Testimonial Models
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

// Contact Inquiry Models
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
}

// Audit Log Model
export interface AuditLogDto {
  id: number;
  action: string;
  tableName: string;
  recordId?: number;
  userId?: number;
  userEmail?: string;
  details?: string;
  ipAddress?: string;
  createdAt: string;
}
