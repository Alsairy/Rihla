import * as signalR from '@microsoft/signalr';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnected = false;

  async startConnection(): Promise<void> {
    if (process.env.NODE_ENV === 'production') {
      this.isConnected = false;
      return;
    }

    if (this.connection && this.isConnected) {
      return;
    }

    const apiUrl = process.env.REACT_APP_API_URL || 'http://localhost:5000';

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiUrl}/notificationHub`, {
          accessTokenFactory: () => {
            return localStorage.getItem('authToken') || '';
          },
        })
        .withAutomaticReconnect()
        .build();

      await this.connection.start();
      this.isConnected = true;

      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        if (user.tenantId) {
          await this.connection.invoke(
            'JoinTenantGroup',
            user.tenantId.toString()
          );
        }
      }
    } catch {
      this.isConnected = false;
      this.connection = null;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        // eslint-disable-next-line no-empty
      } catch {
      } finally {
        this.isConnected = false;
        this.connection = null;
      }
    }
  }

  onNotificationReceived(callback: (_notification: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('ReceiveNotification', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onTripStatusUpdated(callback: (_tripUpdate: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('TripStatusUpdated', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onEmergencyAlert(callback: (_alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('EmergencyAlert', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onDriverCertificationUpdated(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('DriverCertificationUpdated', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onVehicleMaintenanceUpdated(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('VehicleMaintenanceUpdated', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onVehicleStatusChanged(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('VehicleStatusChanged', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onDriverStatusChanged(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('DriverStatusChanged', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onMaintenanceAlertCreated(callback: (_alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('MaintenanceAlertCreated', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onInsuranceExpirationAlert(callback: (_alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('InsuranceExpirationAlert', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onRouteOptimizationUpdate(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('RouteOptimizationUpdate', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onGPSLocationUpdate(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('GPSLocationUpdate', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onAttendanceMethodUpdate(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('AttendanceMethodUpdate', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  onPaymentStatusUpdate(callback: (_update: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('PaymentStatusUpdate', callback);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async joinTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinTripGroup', tripId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async leaveTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveTripGroup', tripId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async joinDriverGroup(driverId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinDriverGroup', driverId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async leaveDriverGroup(driverId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveDriverGroup', driverId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async joinVehicleGroup(vehicleId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinVehicleGroup', vehicleId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  async leaveVehicleGroup(vehicleId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveVehicleGroup', vehicleId);
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  removeAllListeners(): void {
    if (this.connection) {
      try {
        this.connection.off('ReceiveNotification');
        this.connection.off('TripStatusUpdated');
        this.connection.off('EmergencyAlert');
        this.connection.off('DriverCertificationUpdated');
        this.connection.off('VehicleMaintenanceUpdated');
        this.connection.off('VehicleStatusChanged');
        this.connection.off('DriverStatusChanged');
        this.connection.off('MaintenanceAlertCreated');
        this.connection.off('InsuranceExpirationAlert');
        this.connection.off('RouteOptimizationUpdate');
        this.connection.off('GPSLocationUpdate');
        this.connection.off('AttendanceMethodUpdate');
        this.connection.off('PaymentStatusUpdate');
        // eslint-disable-next-line no-empty
      } catch {}
    }
  }

  getConnectionState(): string {
    return this.connection?.state || 'Disconnected';
  }

  isConnectionActive(): boolean {
    return (
      this.isConnected &&
      this.connection?.state === signalR.HubConnectionState.Connected
    );
  }
}

export const signalRService = new SignalRService();
export default signalRService;
