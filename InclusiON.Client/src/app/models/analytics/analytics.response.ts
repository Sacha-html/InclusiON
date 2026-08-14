export interface LevelDistributionItem {
  nivel: number;
  nombreActividad: string;
  alumnosEstancados: number;
  alumnosSuperaron: number;
  totalAlumnos: number;
}

export interface LevelDropoutRateItem {
  nivel: number;
  nombreActividad: string;
  abandonoPct: number;
  alumnosAbandono: number;
  totalIntentos: number;
}

export interface TimeProgressItem {
  dentroDeTiempoPct: number;
  masTiempoPct: number;
  totalSesionesAnalizadas: number;
}

export interface CategoryPerformanceItem {
  categoria: string;
  promedioExito: number;
  totalSesiones: number;
  color: string;
}

export interface ClassroomRankingItem {
  classroomId: string;
  nombreAula: string;
  totalAlumnos: number;
  promedioExitoAula: number;
  totalSesiones: number;
}

export interface FrustrationDetailResponse {
  studentId: string;
  nombreAlumno: string;
  activityId: number;
  nombreActividad: string;
  categoriaPedagogica: string;
  cantidadErrores: number;
  successRate: number;
  timeSpentSeconds: number;
  gasScore: number;
  fecha: string;
  motivoFrustracion: string;
}

export interface AnalyticsDashboardResponse {
  promedio_GAS: number;
  tiempo_Promedio_Nivel: number;
  personasActivas: number;
  totalActividadesCompletadas: number;
  promedioExito: number;
  alertasFrustracion: number;
  distribucion_Por_Nivel: LevelDistributionItem[];
  tasa_Abandono_Por_Nivel: LevelDropoutRateItem[];
  avance_Tiempo_Estimado: TimeProgressItem;
  rendimiento_Por_Categoria: CategoryPerformanceItem[];
  rankingMisAulas: ClassroomRankingItem[];
}

export interface ProfessionalReportsProductivityItem {
  professionalId: string;
  nombreProfesional: string;
  reportesAprobados: number;
  totalReportes: number;
}

export interface ReportStatusDistributionItem {
  estado: string;
  estadoKey: string;
  cantidad: number;
  porcentaje: number;
  color: string;
}

export interface AdminReportsAnalyticsResponse {
  pendientes_Revision: number;
  tasa_Rechazo: number;
  indice_Lectura_Familiar: number;
  total_Reportes: number;
  aprobados_Total: number;
  borradores_Total: number;
  rechazados_Total: number;
  ranking_Profesionales: ProfessionalReportsProductivityItem[];
  distribucion_Estados: ReportStatusDistributionItem[];
}
