import React, { Component, ReactNode } from 'react';
import { Box, Typography, Button } from '@mui/material';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: any) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <Box display="flex" flexDirection="column" alignItems="center" p={4}>
          <Typography variant="h5" gutterBottom>
            Something went wrong
          </Typography>
          <Button onClick={() => window.location.reload()}>Reload Page</Button>
        </Box>
      );
    }
    return this.props.children;
  }
}

export default ErrorBoundary;
