import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from 'react';
import { User } from '../types';
import {
  authService,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../services/authService';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginRequest) => Promise<LoginResponse>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = () => {
      try {
        const storedUser = localStorage.getItem('rihla_user');
        const storedToken = localStorage.getItem('rihla_token');

        if (storedUser && storedToken) {
          const parsedUser = JSON.parse(storedUser);
          setUser(parsedUser);
        } else {
          setUser(null);
          localStorage.removeItem('rihla_user');
          localStorage.removeItem('rihla_token');
        }
      } catch (error) {
        setUser(null);
        localStorage.removeItem('rihla_user');
        localStorage.removeItem('rihla_token');
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
    setLoading(true);
    try {
      const response = await authService.login(credentials);

      if (response.requiresMfa) {
        setLoading(false);
        return response;
      }

      if (response.user && response.token) {
        setUser(response.user);
        localStorage.setItem('rihla_user', JSON.stringify(response.user));
        localStorage.setItem('rihla_token', response.token);
        return response;
      } else {
        throw new Error('Invalid response from authentication service');
      }
    } catch (error) {
      setUser(null);
      localStorage.removeItem('rihla_user');
      localStorage.removeItem('rihla_token');
      setLoading(false);
      throw error;
    } finally {
      if (!credentials.mfaCode) {
        setLoading(false);
      }
    }
  };

  const register = async (userData: RegisterRequest) => {
    setLoading(true);
    try {
      const response = await authService.register(userData);

      if (response.user && response.token) {
        setUser(response.user);
        localStorage.setItem('rihla_user', JSON.stringify(response.user));
        localStorage.setItem('rihla_token', response.token);
      } else {
        throw new Error('Invalid response from registration service');
      }
    } catch (error) {
      setUser(null);
      localStorage.removeItem('rihla_user');
      localStorage.removeItem('rihla_token');
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const logout = async () => {
    setLoading(true);
    try {
      await authService.logout();

      setUser(null);
      localStorage.removeItem('rihla_user');
      localStorage.removeItem('rihla_token');
    } catch (error) {
      setUser(null);
      localStorage.removeItem('rihla_user');
      localStorage.removeItem('rihla_token');
    } finally {
      setLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    loading,
    login,
    register,
    logout,
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
