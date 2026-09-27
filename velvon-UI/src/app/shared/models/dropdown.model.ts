export interface DropdownOption<T = any> {
  label: string;
  value: T;
  icon?: string;
  badge?: string;
  badgeType?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  disabled?: boolean;
  group?: string;
  description?: string;
}
