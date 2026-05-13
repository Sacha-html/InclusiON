export interface RegisterUserRequest {
  email: string;
  password: string;
  confirmPassword: string;
  name: string;
  surname: string;
  acceptTerms: boolean;
}
