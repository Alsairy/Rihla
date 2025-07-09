class ApiClient {
  private baseURL: string;
  private isProduction: boolean;

  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000';
    this.isProduction =
      window.location.hostname !== 'localhost' &&
      window.location.hostname !== '127.0.0.1';
    console.log(
      `ApiClient initialized with base URL: ${this.baseURL}, isProduction: ${this.isProduction}, hostname: ${window.location.hostname}`
    );
  }

  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('authToken');
    return {
      'Content-Type': 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
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
      } catch {}

      throw new Error(errorMessage);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return response.json();
    }

    return response.text() as unknown as T;
  }

  async get<T>(url: string): Promise<T> {
    console.log(`API GET: ${this.baseURL}${url}`);

    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      });

      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API GET error for ${url}:`, error);
      throw error;
    }
  }

  async post<T>(url: string, data?: any): Promise<T> {
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
      throw error;
    }
  }

  async put<T>(url: string, data?: any): Promise<T> {
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
      throw error;
    }
  }

  async delete<T>(url: string): Promise<T> {
    console.log(`API DELETE: ${this.baseURL}${url}`);

    try {
      const response = await fetch(`${this.baseURL}${url}`, {
        method: 'DELETE',
        headers: this.getAuthHeaders(),
      });

      return this.handleResponse<T>(response);
    } catch (error) {
      console.error(`API DELETE error for ${url}:`, error);
      throw error;
    }
  }

  async getRealtimeUpdates(): Promise<any[]> {
    console.log(`API GET: ${this.baseURL}/api/realtime-updates`);

    try {
      const response = await fetch(`${this.baseURL}/api/realtime-updates`, {
        method: 'GET',
        headers: this.getAuthHeaders(),
      });

      return this.handleResponse<any[]>(response);
    } catch (error) {
      console.error(`API GET error for /api/realtime-updates:`, error);
      throw error;
    }
  }
}

export const apiClient = new ApiClient();
