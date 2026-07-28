export function getInvitationStatusColor(status: string): string {
  switch (status) {
    case 'Enviada': return 'info';
    case 'Aceptada': return 'success';
    case 'Expirada': return 'danger';
    default: return 'secondary';
  }
}

export function getActiveStatusColor(isActive: boolean): string {
  return isActive ? 'success' : 'danger';
}
