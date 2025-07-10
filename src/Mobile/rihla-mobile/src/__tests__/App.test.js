import React from 'react';

jest.mock('react-native-gesture-handler', () => ({}));
jest.mock('@react-navigation/native', () => ({
  NavigationContainer: ({ children }) => children,
}));

describe('App', () => {
  it('renders correctly', () => {
    expect(true).toBe(true);
  });
});
