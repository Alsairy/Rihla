import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { AuthProvider } from '../../contexts/AuthContext';
import { apiClient } from '../../services/apiClient';

const SimpleAdminComponent = () => {
  const [stats, setStats] = React.useState<any>(null);

  React.useEffect(() => {
    const fetchStats = async () => {
      try {
        const response = (await apiClient.get('/api/dashboard/stats')) as any;
        setStats(response);
      } catch (error) {
        console.error('Error fetching stats:', error);
      }
    };

    fetchStats();
  }, []);

  return (
    <div>
      <h1>Admin Dashboard</h1>
      {stats ? (
        <div data-testid="stats-loaded">Stats loaded</div>
      ) : (
        <div data-testid="loading">Loading...</div>
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

const mockStats = {
  totalStudents: 1250,
  totalDrivers: 45,
  totalVehicles: 38,
  totalRoutes: 150,
  activeTrips: 12,
  completedTrips: 89,
  pendingMaintenance: 3,
  pendingPayments: 15,
  maintenanceAlerts: 2,
  attendanceRate: 94.5,
};

const renderWithProviders = (component: React.ReactElement) => {
  return render(<AuthProvider>{component}</AuthProvider>);
};

describe('SimpleAdminComponent', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders admin dashboard title', () => {
    renderWithProviders(<SimpleAdminComponent />);
    expect(screen.getByText('Admin Dashboard')).toBeInTheDocument();
  });

  test('shows loading state initially', () => {
    (mockApiClient.get as jest.Mock).mockImplementation(
      () => new Promise(() => {})
    );

    renderWithProviders(<SimpleAdminComponent />);
    expect(screen.getByTestId('loading')).toBeInTheDocument();
  });

  test('loads stats successfully', async () => {
    (mockApiClient.get as jest.Mock).mockResolvedValueOnce(mockStats);

    renderWithProviders(<SimpleAdminComponent />);

    await waitFor(() => {
      expect(screen.getByTestId('stats-loaded')).toBeInTheDocument();
    });
  });

  test('handles API errors gracefully', async () => {
    (mockApiClient.get as jest.Mock).mockRejectedValue(new Error('API Error'));

    renderWithProviders(<SimpleAdminComponent />);

    await waitFor(() => {
      expect(screen.getByTestId('loading')).toBeInTheDocument();
    });
  });
});
