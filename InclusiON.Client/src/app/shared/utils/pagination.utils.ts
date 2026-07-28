import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PaginationCount {
  totalCount: number;
  totalPages: number;
  currentPage: number;
}

/**
 * Hace una petición HEAD al endpoint dado y lee los headers de paginación.
 * Requiere que el backend exponga X-Total-Count, X-Total-Pages, X-Current-Page.
 *
 * @param http      HttpClient inyectado en el componente/servicio
 * @param url       URL del endpoint paginado
 * @param params    Query params opcionales (mismos que usarías en GET)
 */
export function getCount(
  http: HttpClient,
  url: string,
  params?: HttpParams | Record<string, string | number | boolean>
): Observable<PaginationCount> {
  return http
    .head(url, { observe: 'response', params: params as HttpParams })
    .pipe(
      map(res => ({
        totalCount:  +(res.headers.get('X-Total-Count')  ?? '0'),
        totalPages:  +(res.headers.get('X-Total-Pages')  ?? '0'),
        currentPage: +(res.headers.get('X-Current-Page') ?? '0'),
      }))
    );
}
