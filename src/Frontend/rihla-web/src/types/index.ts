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
  dateOfBirth?: string;
  studentNumber?: string;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
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
  dateOfBirth?: string;
  address?: string;
  emergencyContact?: string;
  vehicleId?: number;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
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
  make?: string;
  fuelType?: string;
  insuranceExpiryDate?: string;
  registrationExpiryDate?: string;
  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  isActive?: boolean;
  currentDriverId?: number;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
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
  distance?: number;
  assignedVehicleId?: number;
  assignedDriverId?: number;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface Trip {
  id: number;
  routeId: number;
  routeName?: string;
  vehicleId: number;
  vehiclePlateNumber?: string;
  driverId: number;
  driverName?: string;
  scheduledStartTime: string;
  actualStartTime?: string;
  scheduledEndTime: string;
  actualEndTime?: string;
  status: string;
  tripType?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface Attendance {
  id: number;
  studentId: number;
  tripId: number;
  date: string;
  status: string;
  checkInTime?: string;
  checkOutTime?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface Payment {
  id: number;
  studentId: number;
  amount: number;
  dueDate: string;
  paidDate?: string;
  status: string;
  paymentMethod?: string;
  transactionId?: string;
  description?: string;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface MaintenanceRecord {
  id: number;
  vehicleId: number;
  maintenanceType: string;
  description: string;
  scheduledDate: string;
  completedDate?: string;
  cost: number;
  status: string;
  serviceProvider?: string;
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface Notification {
  id: number;
  title: string;
  message: string;
  type: string;
  priority: string;
  isRead: boolean;
  userId?: number;
  createdAt?: string;
  updatedAt?: string;
  tenantId?: number;
}

export interface DashboardStats {
  totalStudents: number;
  totalDrivers: number;
  totalVehicles: number;
  totalRoutes: number;
  activeTrips: number;
  completedTrips: number;
  pendingMaintenance: number;
  pendingPayments?: number;
  maintenanceAlerts?: number;
  attendanceRate?: number;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}
