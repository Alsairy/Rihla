import { mockDataService } from './mockDataService';
import { 
  DashboardStats, 
  Student, 
  Driver, 
  Vehicle, 
  Route, 
  Trip, 
  Attendance, 
  Payment, 
  MaintenanceRecord, 
  Notification 
} from '../types';

class ApiClient {
  constructor() {
    console.log('ApiClient initialized with mock data service');
  }

  async get<T>(url: string): Promise<T> {
    console.log(`Mock API GET: ${url}`);
    
    if (url.includes('/dashboard/statistics') || url === '/api/dashboard/statistics' || url.includes('/dashboard/stats') || url === '/api/dashboard/stats') {
      return mockDataService.getDashboardStatistics() as Promise<T>;
    }
    
    if (url.includes('/students') || url === '/api/students') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getStudents(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/drivers') || url === '/api/drivers') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getDrivers(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/vehicles') || url === '/api/vehicles') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getVehicles(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/routes') || url === '/api/routes') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getRoutes(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/trips') || url === '/api/trips') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getTrips(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/attendance') || url === '/api/attendance') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getAttendance(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/payments') || url === '/api/payments') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getPayments(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/maintenance') || url === '/api/maintenance') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getMaintenanceRecords(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/notifications') || url === '/api/notifications') {
      const urlParams = new URLSearchParams(url.split('?')[1] || '');
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      return mockDataService.getNotifications(page, pageSize) as Promise<T>;
    }
    
    if (url.includes('/vehicle-location') || url.includes('/gps')) {
      const vehicleId = parseInt(url.split('/').pop() || '1');
      return mockDataService.getVehicleLocation(vehicleId) as Promise<T>;
    }
    
    console.warn(`Unmatched API endpoint: ${url}, returning empty response`);
    return Promise.resolve([] as unknown as T);
  }

  async post<T>(url: string, data?: any): Promise<T> {
    console.log(`Mock API POST: ${url}`, data);
    
    if (url.includes('/students') || url === '/api/students') {
      return mockDataService.createStudent(data) as Promise<T>;
    }
    
    if (url.includes('/drivers') || url === '/api/drivers') {
      return mockDataService.createDriver(data) as Promise<T>;
    }
    
    if (url.includes('/auth/login') || url === '/api/auth/login') {
      return mockDataService.login(data.email, data.password) as Promise<T>;
    }
    
    return Promise.resolve({ ...data, id: Math.floor(Math.random() * 10000) } as unknown as T);
  }

  async put<T>(url: string, data?: any): Promise<T> {
    console.log(`Mock API PUT: ${url}`, data);
    
    if (url.includes('/students')) {
      return mockDataService.updateStudent(data.id, data) as Promise<T>;
    }
    
    return Promise.resolve(data as unknown as T);
  }

  async delete<T>(url: string): Promise<T> {
    console.log(`Mock API DELETE: ${url}`);
    
    if (url.includes('/students')) {
      const id = parseInt(url.split('/').pop() || '0');
      return mockDataService.deleteStudent(id) as Promise<T>;
    }
    
    return Promise.resolve({ success: true } as unknown as T);
  }

  async getRealtimeUpdates(): Promise<any[]> {
    return mockDataService.getRealtimeUpdates();
  }
}

export const apiClient = new ApiClient();
