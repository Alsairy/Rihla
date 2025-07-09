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
    } catch (err) {
      this.isConnected = false;
      this.connection = null;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
      } catch (err) {
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
      } catch (err) {
      }
    }
  }

  onTripStatusUpdated(
    callback: (tripId: string, status: string) => void
  ): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('TripStatusUpdated', callback);
      } catch (err) {
      }
    }
  }

  onEmergencyAlert(callback: (alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('EmergencyAlert', callback);
      } catch (err) {
      }
    }
  }

  async joinTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinTripGroup', tripId);
      } catch (err) {
      }
    }
  }

  async leaveTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveTripGroup', tripId);
      } catch (err) {
      }
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
