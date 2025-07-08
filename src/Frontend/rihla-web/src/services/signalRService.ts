import * as signalR from '@microsoft/signalr';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnected = false;

  async startConnection(): Promise<void> {
    if (process.env.NODE_ENV === 'production') {
      console.log('SignalR disabled in production environment');
      this.isConnected = false;
      return;
    }

    if (this.connection && this.isConnected) {
      return;
    }

    const apiUrl = (window as any).ENV?.REACT_APP_API_URL || 
                  (process.env as any).REACT_APP_API_URL || 
                  'https://jsonplaceholder.typicode.com';
    
    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiUrl}/notificationHub`, {
          accessTokenFactory: () => {
            return localStorage.getItem('authToken') || '';
          }
        })
        .withAutomaticReconnect()
        .build();

      await this.connection.start();
      this.isConnected = true;
      console.log('SignalR Connected');
      
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        if (user.tenantId) {
          await this.connection.invoke('JoinTenantGroup', user.tenantId.toString());
        }
      }
    } catch (err) {
      console.warn('SignalR connection failed, continuing without real-time features:', err);
      this.isConnected = false;
      this.connection = null;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log('SignalR Disconnected');
      } catch (err) {
        console.warn('Error stopping SignalR connection:', err);
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
        console.warn('Failed to register notification callback:', err);
      }
    } else {
      console.log('SignalR not connected, notification callback not registered');
    }
  }

  onTripStatusUpdated(callback: (tripId: string, status: string) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('TripStatusUpdated', callback);
      } catch (err) {
        console.warn('Failed to register trip status callback:', err);
      }
    } else {
      console.log('SignalR not connected, trip status callback not registered');
    }
  }

  onEmergencyAlert(callback: (alert: any) => void): void {
    if (this.connection && this.isConnected) {
      try {
        this.connection.on('EmergencyAlert', callback);
      } catch (err) {
        console.warn('Failed to register emergency alert callback:', err);
      }
    } else {
      console.log('SignalR not connected, emergency alert callback not registered');
    }
  }

  async joinTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('JoinTripGroup', tripId);
      } catch (err) {
        console.warn('Failed to join trip group:', err);
      }
    } else {
      console.log('SignalR not connected, cannot join trip group');
    }
  }

  async leaveTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      try {
        await this.connection.invoke('LeaveTripGroup', tripId);
      } catch (err) {
        console.warn('Failed to leave trip group:', err);
      }
    } else {
      console.log('SignalR not connected, cannot leave trip group');
    }
  }

  getConnectionState(): string {
    return this.connection?.state || 'Disconnected';
  }

  isConnectionActive(): boolean {
    return this.isConnected && this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const signalRService = new SignalRService();
export default signalRService;
