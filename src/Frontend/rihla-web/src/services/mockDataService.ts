import {
  Student,
  Driver,
  Vehicle,
  Route,
  Trip,
  Attendance,
  Payment,
  MaintenanceRecord,
  Notification,
  User,
  DashboardStats,
} from '../types';

class MockDataService {
  private generateId(): number {
    return Math.floor(Math.random() * 10000) + 1;
  }

  private generateDate(daysAgo: number = 0): string {
    const date = new Date();
    date.setDate(date.getDate() - daysAgo);
    return date.toISOString();
  }

  getDashboardStatistics(): Promise<DashboardStats> {
    return Promise.resolve({
      totalStudents: 1250,
      totalDrivers: 45,
      totalVehicles: 38,
      activeTrips: 12,
      totalRoutes: 150,
      completedTrips: 156,
      pendingMaintenance: 5,
      pendingPayments: 23,
      maintenanceAlerts: 5,
      attendanceRate: 94.5,
    });
  }

  getStudents(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Student[]; total: number }> {
    const students: Student[] = [];
    const total = 1250;

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      students.push({
        id,
        firstName: `Student`,
        lastName: `${id}`,
        email: `student${id}@school.edu`,
        phoneNumber: `+966${Math.floor(Math.random() * 1000000000)}`,
        dateOfBirth: this.generateDate(Math.floor(Math.random() * 3650) + 2190),
        address: `Address ${id}, Riyadh, Saudi Arabia`,
        parentName: `Parent ${id}`,
        parentPhone: `+966${Math.floor(Math.random() * 1000000000)}`,
        routeId: Math.floor(Math.random() * 150) + 1,
        routeName: `Route ${Math.floor(Math.random() * 150) + 1}`,
        isActive: Math.random() > 0.1,
        grade: `Grade ${Math.floor(Math.random() * 12) + 1}`,
        school: `School ${Math.floor(Math.random() * 5) + 1}`,
        studentNumber: `STU${id.toString().padStart(6, '0')}`,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 365)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: students, total });
  }

  getDrivers(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Driver[]; total: number }> {
    const drivers: Driver[] = [];
    const total = 45;

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      drivers.push({
        id,
        firstName: `Driver`,
        lastName: `${id}`,
        email: `driver${id}@transport.com`,
        phoneNumber: `+966${Math.floor(Math.random() * 1000000000)}`,
        licenseNumber: `DL${id.toString().padStart(6, '0')}`,
        licenseExpiryDate: this.generateDate(
          -Math.floor(Math.random() * 365) + 365
        ),
        dateOfBirth: this.generateDate(Math.floor(Math.random() * 7300) + 7300),
        address: `Driver Address ${id}, Riyadh, Saudi Arabia`,
        emergencyContact: `+966${Math.floor(Math.random() * 1000000000)}`,
        isActive: Math.random() > 0.05,
        vehicleId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 38) + 1 : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 365)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: drivers, total });
  }

  getVehicles(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Vehicle[]; total: number }> {
    const vehicles: Vehicle[] = [];
    const total = 38;
    const makes = ['Toyota', 'Mercedes', 'Volvo', 'Isuzu', 'Mitsubishi'];
    const models = ['Coaster', 'Sprinter', 'Rosa', 'Civilian', 'Fuso'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      vehicles.push({
        id,
        plateNumber: `RYD-${id.toString().padStart(3, '0')}`,
        make: makes[Math.floor(Math.random() * makes.length)],
        model: models[Math.floor(Math.random() * models.length)],
        year: 2018 + Math.floor(Math.random() * 6),
        capacity: [25, 30, 35, 40, 45][Math.floor(Math.random() * 5)],
        status: Math.random() > 0.1 ? 'Active' : 'Maintenance',
        fuelType: Math.random() > 0.3 ? 'Diesel' : 'Gasoline',
        insuranceExpiryDate: this.generateDate(
          -Math.floor(Math.random() * 365) + 365
        ),
        registrationExpiryDate: this.generateDate(
          -Math.floor(Math.random() * 365) + 365
        ),
        lastMaintenanceDate: this.generateDate(Math.floor(Math.random() * 90)),
        nextMaintenanceDate: this.generateDate(
          -Math.floor(Math.random() * 30) + 30
        ),
        isActive: Math.random() > 0.05,
        driverId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 45) + 1 : undefined,
        driverName:
          Math.random() > 0.2
            ? `Driver ${Math.floor(Math.random() * 45) + 1}`
            : undefined,
        currentDriverId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 45) + 1 : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 365)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: vehicles, total });
  }

  getRoutes(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Route[]; total: number }> {
    const routes: Route[] = [];
    const total = 150;
    const areas = [
      'Al-Malaz',
      'Al-Olaya',
      'Al-Sulaimaniyah',
      'King Fahd',
      'Al-Nakheel',
      'Al-Wurud',
    ];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      routes.push({
        id,
        routeNumber: `R${id.toString().padStart(3, '0')}`,
        name: `Route ${id}`,
        description: `School transportation route covering ${areas[Math.floor(Math.random() * areas.length)]} area`,
        startLocation: `Start Point ${id}, Riyadh`,
        endLocation: 'International School of Riyadh',
        estimatedDuration: 30 + Math.floor(Math.random() * 30),
        distance: 15 + Math.floor(Math.random() * 20),
        isActive: Math.random() > 0.1,
        vehicleId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 38) + 1 : undefined,
        driverId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 45) + 1 : undefined,
        assignedVehicleId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 38) + 1 : undefined,
        assignedDriverId:
          Math.random() > 0.2 ? Math.floor(Math.random() * 45) + 1 : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 365)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: routes, total });
  }

  getTrips(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Trip[]; total: number }> {
    const trips: Trip[] = [];
    const total = 500;
    const statuses = ['Scheduled', 'InProgress', 'Completed', 'Cancelled'];
    const types = ['Pickup', 'Dropoff'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      const scheduledTime = new Date();
      scheduledTime.setHours(
        7 + Math.floor(Math.random() * 8),
        Math.floor(Math.random() * 60)
      );

      trips.push({
        id,
        routeId: Math.floor(Math.random() * 150) + 1,
        routeName: `Route ${Math.floor(Math.random() * 150) + 1}`,
        vehicleId: Math.floor(Math.random() * 38) + 1,
        vehiclePlateNumber: `RYD-${Math.floor(Math.random() * 38) + 1}`,
        driverId: Math.floor(Math.random() * 45) + 1,
        driverName: `Driver ${Math.floor(Math.random() * 45) + 1}`,
        scheduledStartTime: scheduledTime.toISOString(),
        actualStartTime:
          Math.random() > 0.3 ? scheduledTime.toISOString() : undefined,
        scheduledEndTime: new Date(
          scheduledTime.getTime() + 45 * 60000
        ).toISOString(),
        actualEndTime:
          Math.random() > 0.5
            ? new Date(scheduledTime.getTime() + 45 * 60000).toISOString()
            : undefined,
        status: statuses[Math.floor(Math.random() * statuses.length)],
        tripType: types[Math.floor(Math.random() * types.length)],
        notes: Math.random() > 0.7 ? `Trip notes for trip ${id}` : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 30)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 7)),
      });
    }

    return Promise.resolve({ data: trips, total });
  }

  getAttendance(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Attendance[]; total: number }> {
    const attendance: Attendance[] = [];
    const total = 2500;
    const statuses = ['Present', 'Absent', 'Late'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      attendance.push({
        id,
        studentId: Math.floor(Math.random() * 1250) + 1,
        tripId: Math.floor(Math.random() * 500) + 1,
        date: this.generateDate(Math.floor(Math.random() * 30)),
        status: statuses[Math.floor(Math.random() * statuses.length)],
        checkInTime: Math.random() > 0.2 ? this.generateDate(0) : undefined,
        checkOutTime: Math.random() > 0.3 ? this.generateDate(0) : undefined,
        notes:
          Math.random() > 0.8 ? `Attendance note for record ${id}` : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 30)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 7)),
      });
    }

    return Promise.resolve({ data: attendance, total });
  }

  getPayments(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Payment[]; total: number }> {
    const payments: Payment[] = [];
    const total = 800;
    const statuses = ['Paid', 'Pending', 'Overdue', 'Cancelled'];
    const methods = ['Cash', 'Card', 'Bank Transfer', 'Online'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      const amount = 500 + Math.floor(Math.random() * 1000);

      payments.push({
        id,
        studentId: Math.floor(Math.random() * 1250) + 1,
        amount,
        dueDate: this.generateDate(-Math.floor(Math.random() * 60) + 30),
        paidDate:
          Math.random() > 0.3
            ? this.generateDate(Math.floor(Math.random() * 30))
            : undefined,
        status: statuses[Math.floor(Math.random() * statuses.length)],
        paymentMethod:
          Math.random() > 0.3
            ? methods[Math.floor(Math.random() * methods.length)]
            : undefined,
        transactionId:
          Math.random() > 0.3
            ? `TXN${id.toString().padStart(8, '0')}`
            : undefined,
        description: `Monthly transportation fee - ${new Date().toLocaleDateString('en-US', { month: 'long', year: 'numeric' })}`,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 60)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: payments, total });
  }

  getMaintenanceRecords(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: MaintenanceRecord[]; total: number }> {
    const records: MaintenanceRecord[] = [];
    const total = 200;
    const types = ['Routine', 'Repair', 'Emergency', 'Inspection'];
    const statuses = ['Scheduled', 'InProgress', 'Completed', 'Cancelled'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      const cost = 200 + Math.floor(Math.random() * 2000);

      records.push({
        id,
        vehicleId: Math.floor(Math.random() * 38) + 1,
        maintenanceType: types[Math.floor(Math.random() * types.length)],
        description: `Maintenance work for record ${id}`,
        scheduledDate: this.generateDate(-Math.floor(Math.random() * 30) + 15),
        completedDate:
          Math.random() > 0.4
            ? this.generateDate(Math.floor(Math.random() * 15))
            : undefined,
        cost,
        status: statuses[Math.floor(Math.random() * statuses.length)],
        serviceProvider: `Service Provider ${Math.floor(Math.random() * 10) + 1}`,
        notes:
          Math.random() > 0.6
            ? `Additional notes for maintenance ${id}`
            : undefined,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 90)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 30)),
      });
    }

    return Promise.resolve({ data: records, total });
  }

  getNotifications(
    page: number = 1,
    pageSize: number = 10
  ): Promise<{ data: Notification[]; total: number }> {
    const notifications: Notification[] = [];
    const total = 150;
    const types = ['Info', 'Warning', 'Emergency', 'Maintenance'];
    const priorities = ['Low', 'Medium', 'High', 'Critical'];

    for (let i = 0; i < pageSize; i++) {
      const id = (page - 1) * pageSize + i + 1;
      if (id > total) break;

      notifications.push({
        id,
        title: `Notification ${id}`,
        message: `This is notification message ${id} for the transportation system.`,
        type: types[Math.floor(Math.random() * types.length)],
        priority: priorities[Math.floor(Math.random() * priorities.length)],
        isRead: Math.random() > 0.4,
        userId: Math.floor(Math.random() * 100) + 1,
        tenantId: 1,
        createdAt: this.generateDate(Math.floor(Math.random() * 7)),
        updatedAt: this.generateDate(Math.floor(Math.random() * 3)),
      });
    }

    return Promise.resolve({ data: notifications, total });
  }

  createStudent(student: Partial<Student>): Promise<Student> {
    const newStudent: Student = {
      id: this.generateId(),
      firstName: student.firstName || '',
      lastName: student.lastName || '',
      email: student.email || '',
      phoneNumber: student.phoneNumber || '',
      address: student.address || '',
      parentName: student.parentName || '',
      parentPhone: student.parentPhone || '',
      routeId: student.routeId,
      routeName: student.routeName,
      isActive: student.isActive ?? true,
      grade: student.grade || '',
      school: student.school || '',
      dateOfBirth: student.dateOfBirth,
      studentNumber: student.studentNumber,
      tenantId: 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    return Promise.resolve(newStudent);
  }

  updateStudent(id: number, student: Partial<Student>): Promise<Student> {
    return this.getStudents(1, 1).then((result) => {
      const existing = result.data[0];
      const updated = {
        ...existing,
        ...student,
        id,
        updatedAt: new Date().toISOString(),
      };
      return updated;
    });
  }

  deleteStudent(id: number): Promise<void> {
    return Promise.resolve();
  }

  createDriver(driver: Partial<Driver>): Promise<Driver> {
    const newDriver: Driver = {
      id: this.generateId(),
      firstName: driver.firstName || '',
      lastName: driver.lastName || '',
      email: driver.email || '',
      phoneNumber: driver.phoneNumber || '',
      licenseNumber: driver.licenseNumber || '',
      licenseExpiryDate: driver.licenseExpiryDate || '',
      isActive: driver.isActive ?? true,
      dateOfBirth: driver.dateOfBirth,
      address: driver.address,
      emergencyContact: driver.emergencyContact,
      vehicleId: driver.vehicleId,
      tenantId: 1,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    return Promise.resolve(newDriver);
  }

  login(
    email: string,
    password: string
  ): Promise<{ user: User; token: string }> {
    const mockUsers = [
      {
        id: 1,
        email: 'admin@rihla.com',
        username: 'admin',
        role: 'Admin',
        tenantId: 1,
      },
      {
        id: 2,
        email: 'parent@rihla.com',
        username: 'parent',
        role: 'Parent',
        tenantId: 1,
      },
      {
        id: 3,
        email: 'driver@rihla.com',
        username: 'driver',
        role: 'Driver',
        tenantId: 1,
      },
    ];

    const user = mockUsers.find((u) => u.email === email);
    if (user) {
      return Promise.resolve({
        user,
        token: `mock-jwt-token-${user.id}-${Date.now()}`,
      });
    }

    return Promise.reject(new Error('Invalid credentials'));
  }

  getVehicleLocation(
    vehicleId: number
  ): Promise<{ lat: number; lng: number; timestamp: string }> {
    return Promise.resolve({
      lat: 24.7136 + (Math.random() - 0.5) * 0.1,
      lng: 46.6753 + (Math.random() - 0.5) * 0.1,
      timestamp: new Date().toISOString(),
    });
  }

  getRealtimeUpdates(): Promise<any[]> {
    return Promise.resolve([
      {
        type: 'trip_status',
        tripId: Math.floor(Math.random() * 500) + 1,
        status: 'InProgress',
        timestamp: new Date().toISOString(),
      },
      {
        type: 'emergency_alert',
        vehicleId: Math.floor(Math.random() * 38) + 1,
        message: 'Emergency button pressed',
        timestamp: new Date().toISOString(),
      },
    ]);
  }
}

export const mockDataService = null;
export default null;
