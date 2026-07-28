export function formatDate(date: string | null | undefined): string {
  if (!date) return 'Sin especificar';
  const d = new Date(date);
  if (Number.isNaN(d.getTime())) return 'Sin especificar';
  return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export function formatDateTime(date: string | null | undefined): string {
  if (!date) return 'Sin especificar';
  const d = new Date(date);
  if (Number.isNaN(d.getTime())) return 'Sin especificar';
  return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })
    + ' ' + d.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
}
