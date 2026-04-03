export interface TableColumn {
  key: string;
  label: string;
  type?: 'text' | 'number' | 'date' | 'boolean' | 'badge' | 'actions';
  sortable?: boolean;
  actions?: ActionItem[];
}

export interface ActionItem {
  action: string;
  label: string;
  icon?: string;
  color?: string;
  /** Función que recibe el item y devuelve si la acción es visible */
  visible?: (item: any) => boolean;
}

export interface HeaderButton {
  action: string;
  label: string;
  color?: string;
}
