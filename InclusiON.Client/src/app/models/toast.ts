export interface Toast {
  id?: number;
  title?: string;
  message: string;
  color: 'success' | 'danger' | 'warning' | 'info';
  autohide?: boolean;
  delay?: number;
}
