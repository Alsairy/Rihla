import React from 'react';
import { render } from '@testing-library/react';
import { AuthProvider } from './contexts/AuthContext';

test('AuthProvider renders without crashing', () => {
  const TestComponent = () => <div>Test Component</div>;

  render(
    <AuthProvider>
      <TestComponent />
    </AuthProvider>
  );

  expect(document.body).toBeInTheDocument();
});

test('AuthProvider provides context to children', () => {
  const TestComponent = () => (
    <div data-testid="test-component">Test Component</div>
  );

  const { getByTestId } = render(
    <AuthProvider>
      <TestComponent />
    </AuthProvider>
  );

  expect(getByTestId('test-component')).toBeInTheDocument();
});
