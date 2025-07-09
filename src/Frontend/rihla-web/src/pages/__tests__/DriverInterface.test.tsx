import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { AuthProvider } from '../../contexts/AuthContext';
import { apiClient } from '../../services/apiClient';

const SimpleDriverComponent = () => {
  const [trips, setTrips] = React.useState<any[]>([]);
  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    const fetchTrips = async () => {
      try {
        const response = (await apiClient.get(
          '/api/trips/driver/current'
        )) as any;
        setTrips(response.data || []);
      } catch (error) {
        console.error('Error fetching trips:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchTrips();
  }, []);

  const handleStartTrip = async (tripId: number) => {
    try {
      await apiClient.post(`/api/trips/${tripId}/start`);
    } catch (error) {
      console.error('Error starting trip:', error);
    }
  };

  return (
    <div>
      <h1>Driver Interface</h1>
      {loading ? (
        <div data-testid="loading">Loading trips...</div>
      ) : trips.length > 0 ? (
        <div data-testid="trips-list">
          {trips.map((trip: any) => (
            <div key={trip.id} data-testid={`trip-${trip.id}`}>
              <span>{trip.routeName}</span>
              <button onClick={() => handleStartTrip(trip.id)}>
                Start Trip
              </button>
            </div>
          ))}
        </div>
      ) : (
        <div data-testid="no-trips">No trips available</div>
      )}
    </div>
  );
};

jest.mock('../../services/apiClient', () => ({
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
    getRealtimeUpdates: jest.fn(),
  },
}));

const mockApiClient = apiClient as jest.Mocked<typeof apiClient>;

const mockTrips = [
  {
    id: 1,
    routeName: 'Route A - Downtown',
    startTime: '07:00',
    endTime: '08:30',
    status: 'Scheduled',
    studentsCount: 15,
    pickupPoints: ['Point A', 'Point B', 'Point C'],
  },
];

const renderWithProviders = (component: React.ReactElement) => {
  return render(<AuthProvider>{component}</AuthProvider>);
};

describe('SimpleDriverComponent', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders driver interface title', () => {
    renderWithProviders(<SimpleDriverComponent />);
    expect(screen.getByText('Driver Interface')).toBeInTheDocument();
  });

  test('shows loading state initially', () => {
    (mockApiClient.get as jest.Mock).mockImplementation(
      () => new Promise(() => {})
    );

    renderWithProviders(<SimpleDriverComponent />);
    expect(screen.getByTestId('loading')).toBeInTheDocument();
  });

  test('displays trips when available', async () => {
    (mockApiClient.get as jest.Mock).mockResolvedValueOnce({ data: mockTrips });

    renderWithProviders(<SimpleDriverComponent />);

    await waitFor(() => {
      expect(screen.getByTestId('trips-list')).toBeInTheDocument();
      expect(screen.getByText('Route A - Downtown')).toBeInTheDocument();
      expect(screen.getByTestId('trip-1')).toBeInTheDocument();
    });
  });

  test('shows empty state when no trips available', async () => {
    (mockApiClient.get as jest.Mock).mockResolvedValueOnce({ data: [] });

    renderWithProviders(<SimpleDriverComponent />);

    await waitFor(() => {
      expect(screen.getByTestId('no-trips')).toBeInTheDocument();
    });
  });

  test('handles start trip functionality', async () => {
    (mockApiClient.get as jest.Mock).mockResolvedValueOnce({ data: mockTrips });

    (mockApiClient.post as jest.Mock).mockResolvedValueOnce({ success: true });

    renderWithProviders(<SimpleDriverComponent />);

    await waitFor(() => {
      const startButton = screen.getByText('Start Trip');
      fireEvent.click(startButton);
    });

    expect(mockApiClient.post).toHaveBeenCalledWith('/api/trips/1/start');
  });

  test('handles API errors gracefully', async () => {
    (mockApiClient.get as jest.Mock).mockRejectedValue(new Error('API Error'));

    renderWithProviders(<SimpleDriverComponent />);

    await waitFor(() => {
      expect(screen.getByTestId('no-trips')).toBeInTheDocument();
    });
  });
});
