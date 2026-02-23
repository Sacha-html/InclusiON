import { ErrorCode } from '../../error-code.enum';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
  /** Código de error tipado del backend */
  errorCode?: ErrorCode;
  /** Errores de validación por campo */
  fieldErrors?: Record<string, string[]>;
}
