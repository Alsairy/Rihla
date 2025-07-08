import * as signalR from '@microsoft/signalr';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnected = false;

  async startConnection(): Promise<void> {
    if (this.connection && this.isConnected) {
      return;
    }

    const apiUrl = (window as any).ENV?.REACT_APP_API_URL || 
                  (process.env as any).REACT_APP_API_URL || 
                  'http://localhost:5000';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/notificationHub`, {
        accessTokenFactory: () => {
          return localStorage.getItem('authToken') || '';
        }
      })
      .withAutomaticReconnect()
      .build();

    try {
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
      console.error('SignalR Connection Error: ', err);
      this.isConnected = false;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.isConnected = false;
      console.log('SignalR Disconnected');
    }
  }

  onNotificationReceived(callback: (notification: any) => void): void {
    if (this.connection) {
      this.connection.on('ReceiveNotification', callback);
    }
  }

  onTripStatusUpdated(callback: (tripId: string, status: string) => void): void {
    if (this.connection) {
      this.connection.on('TripStatusUpdated', callback);
    }
  }

  onEmergencyAlert(callback: (alert: any) => void): void {
    if (this.connection) {
      this.connection.on('EmergencyAlert', callback);
    }
  }

  async joinTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('JoinTripGroup', tripId);
    }
  }

  async leaveTripGroup(tripId: string): Promise<void> {
    if (this.connection && this.isConnected) {
      await this.connection.invoke('LeaveTripGroup', tripId);
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
