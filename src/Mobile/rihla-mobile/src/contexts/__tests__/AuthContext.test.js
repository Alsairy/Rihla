import React from 'react';
import { render, waitFor } from '@testing-library/react-native';
import { AuthProvider, useAuth } from '../AuthContext';
import { Text } from 'react-native';

const TestComponent = () => {
  const { user, isAuthenticated, isLoading } = useAuth();
  
  return (
    <>
      <Text testID="isAuthenticated">{isAuthenticated ? 'true' : 'false'}</Text>
      <Text testID="isLoading">{isLoading ? 'true' : 'false'}</Text>
      <Text testID="userEmail">{user?.email || 'no-user'}</Text>
    </>
  );
};

describe('AuthContext', () => {
  it('should provide initial authentication state', async () => {
    const { getByTestId } = render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(getByTestId('isAuthenticated').children[0]).toBe('false');
      expect(getByTestId('isLoading').children[0]).toBe('false');
      expect(getByTestId('userEmail').children[0]).toBe('no-user');
    });
  });

  it('should handle authentication state changes', async () => {
    const { getByTestId } = render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(getByTestId('isAuthenticated')).toBeTruthy();
    });
  });
});
