export interface WeeklyProgressResponse {
  periodStart:       string;
  periodEnd:         string;
  personCount:       number;
  totalCompleted:    number;
  avgSuccess:        number;
  frustrationAlerts: number;
}
