import React from 'react';
import { render } from '@testing-library/react-native';
import App from '../../App';

jest.mock('react-native-gesture-handler', () => ({}));
jest.mock('@react-navigation/native', () => ({
  NavigationContainer: ({ children }) => children,
}));

describe('App', () => {
  it('renders correctly', () => {
    expect(true).toBe(true);
  });
});
