export interface User {
  id: number;
  email: string;
  username: string;
  role: string;
  tenantId: number;
}

export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  address: string;
  parentName: string;
  parentPhone: string;
  routeId?: number;
  routeName?: string;
  isActive: boolean;
  grade: string;
  school: string;
}

export interface Driver {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  isActive: boolean;
}

export interface Vehicle {
  id: number;
  plateNumber: string;
  model: string;
  year: number;
  capacity: number;
  status: string;
  driverId?: number;
  driverName?: string;
}

export interface Route {
  id: number;
  routeNumber: string;
  name: string;
  description: string;
  startLocation: string;
  endLocation: string;
  estimatedDuration: number;
  isActive: boolean;
  vehicleId?: number;
  driverId?: number;
}

export interface Trip {
  id: number;
  routeId: number;
  routeName: string;
  vehicleId: number;
  vehiclePlateNumber: string;
  driverId: number;
  driverName: string;
  scheduledStartTime: string;
  actualStartTime?: string;
  scheduledEndTime: string;
  actualEndTime?: string;
  status: string;
}

export interface DashboardStats {
  totalStudents: number;
  totalDrivers: number;
  totalVehicles: number;
  totalRoutes: number;
  activeTrips: number;
  completedTrips: number;
  pendingMaintenance: number;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}
