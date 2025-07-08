import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User } from '../types';
import { authService, LoginRequest, RegisterRequest } from '../services/authService';

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
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

  const mockUsers = {
    'admin@rihla.com': {
      id: 1,
      email: 'admin@rihla.com',
      username: 'admin',
      role: 'Admin',
      tenantId: 1,
      firstName: 'Ahmed',
      lastName: 'Al-Mansouri'
    },
    'superadmin@rihla.com': {
      id: 2,
      email: 'superadmin@rihla.com',
      username: 'superadmin',
      role: 'SuperAdmin',
      tenantId: 1,
      firstName: 'Mohammed',
      lastName: 'Al-Rashid'
    },
    'schooladmin@rihla.com': {
      id: 3,
      email: 'schooladmin@rihla.com',
      username: 'schooladmin',
      role: 'SchoolAdmin',
      tenantId: 1,
      firstName: 'Fatima',
      lastName: 'Al-Zahra'
    },
    'driver@rihla.com': {
      id: 4,
      email: 'driver@rihla.com',
      username: 'driver',
      role: 'Driver',
      tenantId: 1,
      firstName: 'Khalid',
      lastName: 'Al-Otaibi'
    },
    'parent@rihla.com': {
      id: 5,
      email: 'parent@rihla.com',
      username: 'parent',
      role: 'Parent',
      tenantId: 1,
      firstName: 'Layla',
      lastName: 'Al-Mansouri'
    },
    'teacher@rihla.com': {
      id: 6,
      email: 'teacher@rihla.com',
      username: 'teacher',
      role: 'Teacher',
      tenantId: 1,
      firstName: 'Nadia',
      lastName: 'Al-Harbi'
    },
    'supervisor@rihla.com': {
      id: 7,
      email: 'supervisor@rihla.com',
      username: 'supervisor',
      role: 'Supervisor',
      tenantId: 1,
      firstName: 'Omar',
      lastName: 'Al-Saud'
    }
  };

  useEffect(() => {
    const initializeAuth = () => {
      try {
        const storedUser = localStorage.getItem('rihla_user');
        if (storedUser) {
          const parsedUser = JSON.parse(storedUser);
          setUser(parsedUser);
        } else {
          const defaultUser = mockUsers['admin@rihla.com'];
          setUser(defaultUser);
          localStorage.setItem('rihla_user', JSON.stringify(defaultUser));
        }
      } catch (error) {
        console.error('Error initializing auth:', error);
        const defaultUser = mockUsers['admin@rihla.com'];
        setUser(defaultUser);
        localStorage.setItem('rihla_user', JSON.stringify(defaultUser));
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (credentials: LoginRequest) => {
    setLoading(true);
    try {
      const mockUser = mockUsers[credentials.email as keyof typeof mockUsers];
      
      if (mockUser) {
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        setUser(mockUser);
        localStorage.setItem('rihla_user', JSON.stringify(mockUser));
        localStorage.setItem('rihla_token', 'mock-jwt-token-' + mockUser.id);
      } else {
        throw new Error('Invalid credentials. Available test accounts: admin@rihla.com, superadmin@rihla.com, schooladmin@rihla.com, driver@rihla.com, parent@rihla.com, teacher@rihla.com, supervisor@rihla.com');
      }
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const register = async (userData: RegisterRequest) => {
    setLoading(true);
    try {
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      const newUser = {
        id: Date.now(), // Simple ID generation
        email: userData.email,
        username: userData.username || userData.email.split('@')[0],
        role: 'Parent', // Default role for new registrations
        tenantId: 1,
        firstName: 'New',
        lastName: 'User'
      };
      
      setUser(newUser);
      localStorage.setItem('rihla_user', JSON.stringify(newUser));
      localStorage.setItem('rihla_token', 'mock-jwt-token-' + newUser.id);
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const logout = async () => {
    setLoading(true);
    try {
      await new Promise(resolve => setTimeout(resolve, 500));
      
      setUser(null);
      localStorage.removeItem('rihla_user');
      localStorage.removeItem('rihla_token');
    } catch (error) {
      console.error('Logout error:', error);
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
