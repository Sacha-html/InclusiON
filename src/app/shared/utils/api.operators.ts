import { Observable, OperatorFunction, throwError } from 'rxjs';
import { catchError, map } from 'rxjs';
import { ApiResponse } from '@models';

/**
 * Operador RxJS que extrae `data` de un `ApiResponse<T>` y maneja errores.
 */
export function unwrapResponse<T>(): OperatorFunction<ApiResponse<T>, T> {
  return (source: Observable<ApiResponse<T>>) =>
    source.pipe(
      map((response) => response.data),
      catchError((error: unknown) => throwError(() => error)),
    );
}

/**
 * Operador RxJS que solo maneja errores (para endpoints que no retornan ApiResponse).
 */
export function handleApiError<T>(): OperatorFunction<T, T> {
  return (source: Observable<T>) =>
    source.pipe(
      catchError((error: unknown) => throwError(() => error)),
    );
}
