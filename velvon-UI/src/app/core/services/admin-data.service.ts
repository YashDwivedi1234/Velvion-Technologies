import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  ApiResponse,
  UserDto, SaveUserDto,
  RoleDto, SaveRoleDto,
  MenuDto, SaveMenuDto,
  ServiceMasterDto, SaveServiceMasterDto,
  SettingDto, SaveSettingDto,
  AllMastersDto,
  RoleMenuPermissionDto, BulkSaveRolePermissionDto,
  BlogDto, SaveBlogDto,
  PortfolioDto, SavePortfolioDto,
  TeamMemberDto, SaveTeamMemberDto,
  TestimonialDto, SaveTestimonialDto,
  ContactInquiryDto, SaveContactInquiryDto,
  AuditLogDto
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class AdminDataService {
  private api = inject(ApiService);

  // ==================== USERS ====================
  getUsers(activeOnly = false): Observable<ApiResponse<UserDto[]>> {
    return this.api.get<UserDto[]>('Users', { activeOnly });
  }

  getUserById(id: number): Observable<ApiResponse<UserDto>> {
    return this.api.get<UserDto>(`Users/${id}`);
  }

  saveUser(dto: SaveUserDto): Observable<ApiResponse<UserDto>> {
    return this.api.post<UserDto>('Users/save', dto);
  }

  deleteUser(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Users/delete/${id}`, {});
  }

  // ==================== ROLES ====================
  getRoles(activeOnly = false): Observable<ApiResponse<RoleDto[]>> {
    return this.api.get<RoleDto[]>('Master/roles', { activeOnly });
  }

  getRoleById(id: number): Observable<ApiResponse<RoleDto>> {
    return this.api.get<RoleDto>(`Master/roles/${id}`);
  }

  saveRole(dto: SaveRoleDto): Observable<ApiResponse<RoleDto>> {
    return this.api.post<RoleDto>('Master/roles/save', dto);
  }

  deleteRole(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Master/roles/delete/${id}`, {});
  }

  // ==================== MENUS ====================
  getMenus(activeOnly = false): Observable<ApiResponse<MenuDto[]>> {
    return this.api.get<MenuDto[]>('Master/menus', { activeOnly });
  }

  getMenuTree(): Observable<ApiResponse<MenuDto[]>> {
    return this.api.get<MenuDto[]>('Master/menus/tree');
  }

  saveMenu(dto: SaveMenuDto): Observable<ApiResponse<MenuDto>> {
    return this.api.post<MenuDto>('Master/menus/save', dto);
  }

  deleteMenu(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Master/menus/delete/${id}`, {});
  }

  // ==================== SERVICES ====================
  getServices(activeOnly = false): Observable<ApiResponse<ServiceMasterDto[]>> {
    return this.api.get<ServiceMasterDto[]>('Master/services', { activeOnly });
  }

  saveService(dto: SaveServiceMasterDto): Observable<ApiResponse<ServiceMasterDto>> {
    return this.api.post<ServiceMasterDto>('Master/services/save', dto);
  }

  deleteService(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Master/services/delete/${id}`, {});
  }

  // ==================== SETTINGS ====================
  getSettings(activeOnly = false): Observable<ApiResponse<SettingDto[]>> {
    return this.api.get<SettingDto[]>('Master/settings', { activeOnly });
  }

  saveSetting(dto: SaveSettingDto): Observable<ApiResponse<SettingDto>> {
    return this.api.post<SettingDto>('Master/settings/save', dto);
  }

  deleteSetting(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Master/settings/delete/${id}`, {});
  }

  getAllMasters(): Observable<ApiResponse<AllMastersDto>> {
    return this.api.get<AllMastersDto>('Master/all');
  }

  // ==================== PERMISSIONS ====================
  getAllPermissions(): Observable<ApiResponse<RoleMenuPermissionDto[]>> {
    return this.api.get<RoleMenuPermissionDto[]>('Permissions');
  }

  getPermissionsByRoleId(roleId: number): Observable<ApiResponse<RoleMenuPermissionDto[]>> {
    return this.api.get<RoleMenuPermissionDto[]>(`Permissions/role/${roleId}`);
  }

  saveRolePermissions(dto: BulkSaveRolePermissionDto): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>('Permissions/bulk-save', dto);
  }

  // ==================== BLOGS ====================
  getBlogs(activeOnly = false): Observable<ApiResponse<BlogDto[]>> {
    return this.api.get<BlogDto[]>('Blogs', { activeOnly });
  }

  saveBlog(dto: SaveBlogDto): Observable<ApiResponse<BlogDto>> {
    return this.api.post<BlogDto>('Blogs/save', dto);
  }

  deleteBlog(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Blogs/delete/${id}`, {});
  }

  // ==================== PORTFOLIO ====================
  getPortfolios(activeOnly = false): Observable<ApiResponse<PortfolioDto[]>> {
    return this.api.get<PortfolioDto[]>('Portfolio', { activeOnly });
  }

  savePortfolio(dto: SavePortfolioDto): Observable<ApiResponse<PortfolioDto>> {
    return this.api.post<PortfolioDto>('Portfolio/save', dto);
  }

  deletePortfolio(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Portfolio/delete/${id}`, {});
  }

  // ==================== TEAM MEMBERS ====================
  getTeamMembers(activeOnly = false): Observable<ApiResponse<TeamMemberDto[]>> {
    return this.api.get<TeamMemberDto[]>('TeamMembers', { activeOnly });
  }

  saveTeamMember(dto: SaveTeamMemberDto): Observable<ApiResponse<TeamMemberDto>> {
    return this.api.post<TeamMemberDto>('TeamMembers/save', dto);
  }

  deleteTeamMember(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`TeamMembers/delete/${id}`, {});
  }

  // ==================== TESTIMONIALS ====================
  getTestimonials(activeOnly = false): Observable<ApiResponse<TestimonialDto[]>> {
    return this.api.get<TestimonialDto[]>('Testimonials', { activeOnly });
  }

  saveTestimonial(dto: SaveTestimonialDto): Observable<ApiResponse<TestimonialDto>> {
    return this.api.post<TestimonialDto>('Testimonials/save', dto);
  }

  deleteTestimonial(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`Testimonials/delete/${id}`, {});
  }

  // ==================== CONTACT INQUIRIES ====================
  getInquiries(activeOnly = false): Observable<ApiResponse<ContactInquiryDto[]>> {
    return this.api.get<ContactInquiryDto[]>('ContactInquiries', { activeOnly });
  }

  saveInquiry(dto: SaveContactInquiryDto): Observable<ApiResponse<ContactInquiryDto>> {
    return this.api.post<ContactInquiryDto>('ContactInquiries/save', dto);
  }

  deleteInquiry(id: number): Observable<ApiResponse<boolean>> {
    return this.api.post<boolean>(`ContactInquiries/delete/${id}`, {});
  }

  // ==================== AUDIT LOGS ====================
  getAuditLogs(limit = 100): Observable<ApiResponse<AuditLogDto[]>> {
    return this.api.get<AuditLogDto[]>('AuditLogs', { limit });
  }
}
