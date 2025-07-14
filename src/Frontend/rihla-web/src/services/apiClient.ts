class ApiClient {
  private baseURL: string;
  private isProduction: boolean;

  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL || 'http://localhost:5000';
    this.isProduction =
      window.location.hostname !== 'localhost' &&
      window.location.hostname !== '127.0.0.1';
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
    const response = await fetch(`${this.baseURL}${url}`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<T>(response);
  }

  async post<T>(url: string, data?: any, options?: { headers?: HeadersInit }): Promise<T> {
    const isFormData = data instanceof FormData;
    let headers: Record<string, string>;
    
    if (isFormData) {
      const authHeaders = this.getAuthHeaders() as Record<string, string>;
      headers = {};
      if (authHeaders.Authorization) {
        headers.Authorization = authHeaders.Authorization;
      }
      if (options?.headers) {
        Object.assign(headers, options.headers);
      }
    } else {
      headers = {
        ...this.getAuthHeaders(),
        ...options?.headers,
      } as Record<string, string>;
    }

    const response = await fetch(`${this.baseURL}${url}`, {
      method: 'POST',
      headers,
      body: isFormData ? data : (data ? JSON.stringify(data) : undefined),
    });

    return this.handleResponse<T>(response);
  }

  async put<T>(url: string, data?: any): Promise<T> {
    const response = await fetch(`${this.baseURL}${url}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: data ? JSON.stringify(data) : undefined,
    });

    return this.handleResponse<T>(response);
  }

  async delete<T>(url: string): Promise<T> {
    const response = await fetch(`${this.baseURL}${url}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<T>(response);
  }

  async getRealtimeUpdates(): Promise<any[]> {
    const response = await fetch(`${this.baseURL}/api/realtime-updates`, {
      method: 'GET',
      headers: this.getAuthHeaders(),
    });

    return this.handleResponse<any[]>(response);
  }
}

export const apiClient = new ApiClient();
