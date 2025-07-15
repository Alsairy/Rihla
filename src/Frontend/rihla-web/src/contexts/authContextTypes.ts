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

export { User } from '../types';
export {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../services/authService';
