import { HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { SpinnerService } from '@services';

const EXCLUDED_URLS = [
  '/api/health',
  '/api/maintenance',
];

export const spinnerInterceptor: HttpInterceptorFn = (req, next) => {
  const spinnerService = inject(SpinnerService);

  if (shouldSkip(req)) {
    return next(req);
  }

  spinnerService.show();

  return next(req).pipe(
    tap({
      next: () => spinnerService.hide(),
      error: () => spinnerService.hide(),
    })
  );
};

function shouldSkip(req: HttpRequest<unknown>): boolean {
  return EXCLUDED_URLS.some(url => req.url.includes(url));
}