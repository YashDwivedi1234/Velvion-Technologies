import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AdminDataService } from '../../core/services/admin-data.service';
import { ToastService, LogoComponent, IconComponent, DropdownComponent, DropdownOption } from '../../shared';
import { RoleDto } from '../../core/models/api.models';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    LogoComponent,
    IconComponent,
    DropdownComponent
  ],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);

  mode = signal<'login' | 'register'>('login');
  loading = signal<boolean>(false);
  showPassword = signal<boolean>(false);
  showConfirmPassword = signal<boolean>(false);

  loginForm!: FormGroup;
  registerForm!: FormGroup;

  roles = signal<RoleDto[]>([]);
  roleOptions = signal<DropdownOption[]>([]);
  selectedRole = signal<number>(1);
  currentYear = new Date().getFullYear();

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['mode'] === 'register') {
        this.mode.set('register');
      } else {
        this.mode.set('login');
      }
    });

    this.initForms();
    this.loadRoles();
  }

  private initForms(): void {
    this.loginForm = this.fb.group({
      email: ['admin@velvion.com', [Validators.required, Validators.email]],
      password: ['Admin@123', [Validators.required, Validators.minLength(4)]],
      rememberMe: [true]
    });

    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      mobile: ['', [Validators.pattern('^[0-9+\\-\\s()]{7,15}$')]],
      roleId: [1, Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(g: FormGroup) {
    const pass = g.get('password')?.value;
    const confirm = g.get('confirmPassword')?.value;
    return pass === confirm ? null : { mismatch: true };
  }

  private loadRoles(): void {
    this.adminDataService.getRoles(true).subscribe(res => {
      if (res.success && res.data && res.data.length > 0) {
        this.roles.set(res.data);
        const options: DropdownOption[] = res.data.map(r => ({
          label: r.roleName,
          value: r.id.toString(),
          icon: 'shield'
        }));
        this.roleOptions.set(options);
        this.registerForm.patchValue({ roleId: res.data[0].id });
        this.selectedRole.set(res.data[0].id);
      } else {
        // Default roles if API roles not yet populated
        this.roleOptions.set([
          { label: 'Super Admin', value: '1', icon: 'shield' },
          { label: 'Manager', value: '2', icon: 'user' },
          { label: 'Staff Member', value: '3', icon: 'users' }
        ]);
      }
    });
  }

  switchMode(newMode: 'login' | 'register'): void {
    this.mode.set(newMode);
  }

  onRoleChange(val: string | string[]): void {
    const roleId = Number(val);
    this.selectedRole.set(roleId);
    this.registerForm.patchValue({ roleId });
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.toastService.error('Please enter valid email and password credentials.', 'Validation Error');
      return;
    }

    this.loading.set(true);
    const { email, password } = this.loginForm.value;

    this.authService.login({ email, password }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.toastService.success(`Welcome back, ${res.data.fullName}! (${res.data.roleName})`, 'Login Successful');
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.toastService.error(res.message || 'Invalid email or password. Please try again.', 'Authentication Failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(err.message || 'Login connection failed', 'Error');
      }
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      if (this.registerForm.errors?.['mismatch']) {
        this.toastService.error('Passwords do not match.', 'Registration Error');
      } else {
        this.toastService.error('Please fill in all required registration fields correctly.', 'Validation Error');
      }
      return;
    }

    this.loading.set(true);
    const val = this.registerForm.value;

    this.authService.register({
      roleId: Number(val.roleId) || 1,
      fullName: val.fullName.trim(),
      email: val.email.trim().toLowerCase(),
      mobile: val.mobile?.trim(),
      password: val.password,
      isActive: true
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toastService.success(`Account created successfully for ${val.fullName}! You can now login.`, 'Registration Complete');
          // Autofill login form
          this.loginForm.patchValue({
            email: val.email,
            password: val.password
          });
          this.mode.set('login');
        } else {
          this.toastService.error(res.message || 'Failed to create user account.', 'Registration Failed');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(err.message || 'Registration error occurred', 'Error');
      }
    });
  }
}
