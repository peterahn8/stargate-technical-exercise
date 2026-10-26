import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { BaseResponse } from './models';

/** A failed API call, carrying a message that is safe to show the user. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/**
 * Turns every failed HTTP call into an {@link ApiError}. The API's own message is used when it sent
 * one (every API error uses the BaseResponse shape, D15), so screens can show it as-is.
 */
export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(catchError((error: unknown) => throwError(() => toApiError(error))));

function toApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return new ApiError(0, 'An unexpected error occurred.');
  }

  const body = error.error as Partial<BaseResponse> | null;
  if (typeof body?.message === 'string') {
    return new ApiError(error.status, body.message);
  }

  // Status 0 means the browser couldn't connect. Through the dev server's proxy, a stopped API shows up
  // as a gateway error (502-504) instead, without the API's message.
  return error.status === 0 || [502, 503, 504].includes(error.status)
    ? new ApiError(error.status, "The API couldn't be reached. Is it running?")
    : new ApiError(error.status, `The request failed (HTTP ${error.status}).`);
}
