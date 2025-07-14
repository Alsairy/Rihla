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
      } catch {
      } finally {
        this.isConnected = false;
        this.connection = null;
      }
    }
  }

  onNotificationReceived(callback: (notification: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('ReceiveNotification', callback);
      } catch {}
    }
  }

  onTripStatusUpdated(
    callback: (tripId: string, status: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('TripStatusUpdated', callback);
      } catch {}
    }
  }

  onEmergencyAlert(callback: (alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('EmergencyAlert', callback);
      } catch {}
    }
  }

  onDriverCertificationUpdated(
    callback: (driverId: string, certificationType: string, status: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('DriverCertificationUpdated', callback);
      } catch {}
    }
  }

  onVehicleMaintenanceUpdated(
    callback: (vehicleId: string, maintenanceType: string, status: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('VehicleMaintenanceUpdated', callback);
      } catch {}
    }
  }

  onVehicleStatusChanged(
    callback: (vehicleId: string, oldStatus: string, newStatus: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('VehicleStatusChanged', callback);
      } catch {}
    }
  }

  onDriverStatusChanged(
    callback: (driverId: string, oldStatus: string, newStatus: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('DriverStatusChanged', callback);
      } catch {}
    }
  }

  onMaintenanceAlertCreated(
    callback: (alert: any) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('MaintenanceAlertCreated', callback);
      } catch {}
    }
  }

  onInsuranceExpirationAlert(
    callback: (alert: any) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('InsuranceExpirationAlert', callback);
      } catch {}
    }
  }

  async joinTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinTripGroup', tripId);
      } catch {}
    }
  }

  async leaveTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveTripGroup', tripId);
      } catch {}
    }
  }

  async joinDriverGroup(driverId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinDriverGroup', driverId);
      } catch {}
    }
  }

  async leaveDriverGroup(driverId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveDriverGroup', driverId);
      } catch {}
    }
  }

  async joinVehicleGroup(vehicleId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinVehicleGroup', vehicleId);
      } catch {}
    }
  }

  async leaveVehicleGroup(vehicleId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveVehicleGroup', vehicleId);
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
