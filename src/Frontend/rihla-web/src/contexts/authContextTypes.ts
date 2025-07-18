import { User } from '../types';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../services/authService';

export interface AuthContextType {
  user: User | null;
  loading: boolean;
  // eslint-disable-next-line no-unused-vars
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  // eslint-disable-next-line no-unused-vars
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

export type { User } from '../types';
export type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../services/authService';
