export type ColumnType = 'text' | 'badge' | 'date' | 'avatar' | 'boolean' | 'number' | 'custom';

export interface TableColumn<T = any> {
  key: string;
  label: string;
  sortable?: boolean;
  type?: ColumnType;
  width?: string;
  align?: 'left' | 'center' | 'right';
  badgeTypeMap?: Record<string, 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral'>;
  formatter?: (value: any, row: T) => string;
}

export interface TableAction<T = any> {
  id: string;
  label: string;
  icon?: string;
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  isVisible?: (row: T) => boolean;
}

export interface TableBulkAction<T = any> {
  id: string;
  label: string;
  icon?: string;
  variant?: 'primary' | 'secondary' | 'danger';
}
