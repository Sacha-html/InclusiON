import { User } from "./user";

export interface AuthData {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  mustChangePassword?: boolean;
}
