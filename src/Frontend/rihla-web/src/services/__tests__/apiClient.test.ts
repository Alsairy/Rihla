import { apiClient } from '../apiClient';

global.fetch = jest.fn();
const mockFetch = fetch as jest.MockedFunction<typeof fetch>;

describe('ApiClient', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  test('GET request with successful response', async () => {
    const mockData = { id: 1, name: 'Test' };
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockData,
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    const result = await apiClient.get('/api/test');

    expect(result).toEqual(mockData);
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/test'),
      expect.objectContaining({
        method: 'GET',
        headers: expect.objectContaining({
          'Content-Type': 'application/json'
        })
      })
    );
  });

  test('POST request with data', async () => {
    const mockData = { id: 1, name: 'Test' };
    const postData = { name: 'New Test' };
    
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockData,
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    const result = await apiClient.post('/api/test', postData);

    expect(result).toEqual(mockData);
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/test'),
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json'
        }),
        body: JSON.stringify(postData)
      })
    );
  });

  test('handles authentication headers when token exists', async () => {
    localStorage.setItem('authToken', 'test-token');
    
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({}),
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    await apiClient.get('/api/test');

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/test'),
      expect.objectContaining({
        headers: expect.objectContaining({
          'Authorization': 'Bearer test-token'
        })
      })
    );
  });

  test('handles 401 unauthorized response', async () => {
    localStorage.setItem('authToken', 'invalid-token');
    localStorage.setItem('refreshToken', 'refresh-token');
    localStorage.setItem('user', JSON.stringify({ id: 1 }));

    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 401,
      json: async () => ({ message: 'Unauthorized' })
    } as Response);

    await expect(apiClient.get('/api/test')).rejects.toThrow('Authentication required');

    expect(localStorage.getItem('authToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });

  test('handles network errors', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'));

    await expect(apiClient.get('/api/test')).rejects.toThrow('Network error');
  });

  test('handles non-JSON responses', async () => {
    const textResponse = 'Plain text response';
    mockFetch.mockResolvedValueOnce({
      ok: true,
      text: async () => textResponse,
      headers: new Headers({ 'content-type': 'text/plain' })
    } as Response);

    const result = await apiClient.get('/api/test');

    expect(result).toBe(textResponse);
  });

  test('PUT request works correctly', async () => {
    const mockData = { id: 1, name: 'Updated Test' };
    const putData = { name: 'Updated Test' };
    
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockData,
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    const result = await apiClient.put('/api/test/1', putData);

    expect(result).toEqual(mockData);
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/test/1'),
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify(putData)
      })
    );
  });

  test('DELETE request works correctly', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ success: true }),
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    const result = await apiClient.delete('/api/test/1');

    expect(result).toEqual({ success: true });
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/test/1'),
      expect.objectContaining({
        method: 'DELETE'
      })
    );
  });

  test('getRealtimeUpdates works correctly', async () => {
    const mockUpdates = [{ id: 1, message: 'Update 1' }];
    
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockUpdates,
      headers: new Headers({ 'content-type': 'application/json' })
    } as Response);

    const result = await apiClient.getRealtimeUpdates();

    expect(result).toEqual(mockUpdates);
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/realtime-updates'),
      expect.objectContaining({
        method: 'GET'
      })
    );
  });
});
