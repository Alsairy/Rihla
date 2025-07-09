const mockData = {
  dashboardStats: {
    totalStudents: 3247,
    totalDrivers: 523,
    totalVehicles: 312,
    activeTrips: 28,
    studentsGrowth: 12.5,
    driversGrowth: -8.2,
    vehiclesGrowth: 15.7,
    tripsGrowth: 22.1
  },
  students: Array.from({ length: 3247 }, (_, i) => ({
    id: i + 1,
    firstName: `Student${i + 1}`,
    lastName: `LastName${i + 1}`,
    email: `student${i + 1}@school.edu.sa`,
    phoneNumber: `+966${String(Math.floor(Math.random() * 900000000) + 100000000)}`,
    dateOfBirth: new Date(2010 + Math.floor(Math.random() * 8), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    grade: `Grade ${Math.floor(Math.random() * 12) + 1}`,
    address: `${Math.floor(Math.random() * 9999) + 1} King Fahd Road, Riyadh`,
    parentName: `Parent${i + 1}`,
    parentPhone: `+966${String(Math.floor(Math.random() * 900000000) + 100000000)}`,
    routeId: Math.floor(Math.random() * 50) + 1,
    routeName: `Route ${Math.floor(Math.random() * 50) + 1}`,
    isActive: Math.random() > 0.1
  })),
  drivers: Array.from({ length: 523 }, (_, i) => ({
    id: i + 1,
    firstName: `Driver${i + 1}`,
    lastName: `LastName${i + 1}`,
    email: `driver${i + 1}@rihla.sa`,
    phoneNumber: `+966${String(Math.floor(Math.random() * 900000000) + 100000000)}`,
    licenseNumber: `LIC${String(i + 1).padStart(6, '0')}`,
    licenseExpiryDate: new Date(2025 + Math.floor(Math.random() * 5), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    dateOfBirth: new Date(1970 + Math.floor(Math.random() * 30), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    address: `${Math.floor(Math.random() * 9999) + 1} Prince Sultan Road, Riyadh`,
    emergencyContact: `+966${String(Math.floor(Math.random() * 900000000) + 100000000)}`,
    vehicleId: Math.floor(Math.random() * 312) + 1,
    isActive: Math.random() > 0.05,
    rating: (Math.random() * 2 + 3).toFixed(1)
  })),
  vehicles: Array.from({ length: 312 }, (_, i) => ({
    id: i + 1,
    plateNumber: `${String.fromCharCode(65 + Math.floor(Math.random() * 26))}${String.fromCharCode(65 + Math.floor(Math.random() * 26))}${String.fromCharCode(65 + Math.floor(Math.random() * 26))}-${String(Math.floor(Math.random() * 9000) + 1000)}`,
    make: ['Toyota', 'Mercedes', 'Hyundai', 'Ford', 'Nissan'][Math.floor(Math.random() * 5)],
    model: ['Coaster', 'Sprinter', 'County', 'Transit', 'Civilian'][Math.floor(Math.random() * 5)],
    year: 2018 + Math.floor(Math.random() * 7),
    capacity: [25, 30, 35, 40, 45][Math.floor(Math.random() * 5)],
    color: ['White', 'Yellow', 'Blue', 'Silver', 'Red'][Math.floor(Math.random() * 5)],
    registrationExpiry: new Date(2024 + Math.floor(Math.random() * 3), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    insuranceExpiry: new Date(2024 + Math.floor(Math.random() * 2), Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    lastMaintenanceDate: new Date(2024, Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1).toISOString(),
    mileage: Math.floor(Math.random() * 200000) + 50000,
    fuelType: ['Diesel', 'Gasoline'][Math.floor(Math.random() * 2)],
    isActive: Math.random() > 0.02,
    gpsDeviceId: `GPS${String(i + 1).padStart(6, '0')}`,
    currentLocation: { lat: 24.7136 + (Math.random() - 0.5) * 0.1, lng: 46.6753 + (Math.random() - 0.5) * 0.1 }
  })),
  notifications: Array.from({ length: 234 }, (_, i) => ({
    id: i + 1,
    title: `Notification ${i + 1}`,
    message: `This is notification message ${i + 1} for the transportation system.`,
    type: ['Info', 'Warning', 'Error', 'Success'][Math.floor(Math.random() * 4)],
    priority: ['Low', 'Medium', 'High', 'Critical'][Math.floor(Math.random() * 4)],
    createdAt: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000).toISOString(),
    isRead: Math.random() > 0.3,
    userId: Math.floor(Math.random() * 100) + 1,
    relatedEntityType: ['Trip', 'Vehicle', 'Student', 'Driver'][Math.floor(Math.random() * 4)],
    relatedEntityId: Math.floor(Math.random() * 1000) + 1
  }))
};

class ApiClient {
  private baseURL: string;
  private isProduction: boolean;

  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000';
    this.isProduction = window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1';
    console.log(`ApiClient initialized with base URL: ${this.baseURL}, isProduction: ${this.isProduction}, hostname: ${window.location.hostname}`);
  }

  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    };
  }

  private async mockRequest<T>(url: string, method: string = 'GET', data?: any): Promise<T> {
    console.log(`Mock API ${method}: ${this.baseURL}${url}`);
    
    await new Promise(resolve => setTimeout(resolve, Math.random() * 500 + 200));

    if (url === '/api/auth/login') {
      const body = data;
      if (body.email === 'admin@rihla.com' && body.password === 'password123') {
        const token = 'mock-jwt-token-' + Date.now();
        const user = {
          id: 1,
          email: 'admin@rihla.com',
          firstName: 'Admin',
          lastName: 'User',
          role: 'SuperAdmin',
          tenantId: 1
        };
        return { token, user } as T;
      } else {
        throw new Error('Invalid credentials');
      }
    }

    if (url === '/api/dashboard/statistics') {
      return mockData.dashboardStats as T;
    }

    const pageMatch = url.match(/[?&]page=(\d+)/);
    const pageSizeMatch = url.match(/[?&]pageSize=(\d+)/);
    const page = pageMatch ? parseInt(pageMatch[1]) : 1;
    const pageSize = pageSizeMatch ? parseInt(pageSizeMatch[1]) : 10;

    if (url.includes('/api/students')) {
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      return {
        data: mockData.students.slice(startIndex, endIndex),
        totalCount: mockData.students.length,
        page,
        pageSize,
        totalPages: Math.ceil(mockData.students.length / pageSize)
      } as T;
    }

    if (url.includes('/api/drivers')) {
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      return {
        data: mockData.drivers.slice(startIndex, endIndex),
        totalCount: mockData.drivers.length,
        page,
        pageSize,
        totalPages: Math.ceil(mockData.drivers.length / pageSize)
      } as T;
    }

    if (url.includes('/api/vehicles')) {
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      return {
        data: mockData.vehicles.slice(startIndex, endIndex),
        totalCount: mockData.vehicles.length,
        page,
        pageSize,
        totalPages: Math.ceil(mockData.vehicles.length / pageSize)
      } as T;
    }

    if (url.includes('/api/notifications')) {
      return mockData.notifications.slice(0, 10) as T;
    }

    return { success: true, message: 'Mock response' } as T;
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      if (response.status === 401) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        window.location.href = '/login';
        throw new Error('Authentication required');
      }
      
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errorData = await response.json();
        errorMessage = errorData.message || errorMessage;
      } catch {
      }
      
      throw new Error(errorMessage);
    }
    
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }
    
    return response.text() as unknown as T;
  }

  async get<T>(url: string): Promise<T> {
    if (this.isProduction) {
      return this.mockRequest<T>(url, 'GET');
    }

    console.log(`API GET: ${this.baseURL}${url}`);
    
    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      });
      
      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API GET error for ${url}:`, error);
      return this.mockRequest<T>(url, 'GET');
    }
  }

  async post<T>(url: string, data?: any): Promise<T> {
    if (this.isProduction) {
      return this.mockRequest<T>(url, 'POST', data);
    }

    console.log(`API POST: ${this.baseURL}${url}`, data);
    
    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: data ? JSON.stringify(data) : undefined,
      });
      
      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API POST error for ${url}:`, error);
      return this.mockRequest<T>(url, 'POST', data);
    }
  }

  async put<T>(url: string, data?: any): Promise<T> {
    if (this.isProduction) {
      return this.mockRequest<T>(url, 'PUT', data);
    }

    console.log(`API PUT: ${this.baseURL}${url}`, data);
    
    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: data ? JSON.stringify(data) : undefined,
      });
      
      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API PUT error for ${url}:`, error);
      return this.mockRequest<T>(url, 'PUT', data);
    }
  }

  async delete<T>(url: string): Promise<T> {
    if (this.isProduction) {
      return this.mockRequest<T>(url, 'DELETE');
    }

    console.log(`API DELETE: ${this.baseURL}${url}`);
    
    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'DELETE',
        headers: this.getAuthHeaders(),
      });
      
      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API DELETE error for ${url}:`, error);
      return this.mockRequest<T>(url, 'DELETE');
    }
  }

  async getRealtimeUpdates(): Promise<any[]> {
    return [];
  }
}

export const apiClient = new ApiClient();
