import { Observable, OperatorFunction, throwError, of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs';
import { ApiResponse } from '@models';

export function unwrapResponse<T>(): OperatorFunction<ApiResponse<T>, T> {
  return (source: Observable<ApiResponse<T>>) =>
    source.pipe(
      mergeMap((response) => {
        if (!response.success) {
          return throwError(() => ({
            status: response.errorCode ?? 500,
            message: response.message ?? 'Error desconocido',
            errors: response.errors ?? [],
            fieldErrors: response.fieldErrors,
          }));
        }
        return of(response.data as T);
      }),
      catchError((error: unknown) => throwError(() => error)),
    );
}

export function handleApiError<T>(): OperatorFunction<T, T> {
  return (source: Observable<T>) =>
    source.pipe(
      catchError((error: unknown) => throwError(() => error)),
    );
}
