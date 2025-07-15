import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  LinearProgress,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Tooltip,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Button,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  Speed as SpeedIcon,
  LocalGasStation as FuelIcon,
  AccessTime as TimeIcon,
  Route as RouteIcon,
  Refresh as RefreshIcon,
  Assessment as AssessmentIcon,
  Timeline as TimelineIcon,
} from '@mui/icons-material';
import { apiClient } from '../services/apiClient';

interface RouteEfficiencyMetrics {
  routeId: number;
  routeName: string;
  totalDistance: number;
  estimatedDuration: string;
  numberOfStops: number;
  studentCapacity: number;
  averageActualDuration: string;
  onTimePerformance: number;
  fuelEfficiency: number;
  costPerStudent: number;
  optimizationScore: number;
  lastCalculated: string;
}

interface EfficiencyTrend {
  date: string;
  onTimePerformance: number;
  fuelEfficiency: number;
  optimizationScore: number;
}

interface RouteEfficiencyAnalyticsProps {
  selectedRouteId?: number;
  onRouteSelect?: (routeId: number) => void;
}

export const RouteEfficiencyAnalytics: React.FC<
  RouteEfficiencyAnalyticsProps
> = ({ selectedRouteId, onRouteSelect }) => {
  const [metrics, setMetrics] = useState<RouteEfficiencyMetrics[]>([]);
  const [trends, setTrends] = useState<EfficiencyTrend[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedPeriod, setSelectedPeriod] = useState<string>('7days');
  const [selectedMetric, setSelectedMetric] =
    useState<string>('onTimePerformance');

  useEffect(() => {
    loadEfficiencyMetrics();
    loadEfficiencyTrends();
  }, [selectedPeriod]);

  const loadEfficiencyMetrics = async () => {
    try {
      setLoading(true);
      const response = await apiClient.get('/api/routes/efficiency-metrics');
      setMetrics(response as RouteEfficiencyMetrics[]);
    } catch (err: any) {
      setError(err.message || 'Failed to load efficiency metrics');
    } finally {
      setLoading(false);
    }
  };

  const loadEfficiencyTrends = async () => {
    try {
      const response = await apiClient.get(
        `/api/routes/efficiency-trends?period=${selectedPeriod}`
      );
      setTrends(response as EfficiencyTrend[]);
    } catch (err: any) {
      console.error('Failed to load efficiency trends:', err);
    }
  };

  const getPerformanceColor = (
    score: number
  ): 'success' | 'warning' | 'error' => {
    if (score >= 80) return 'success';
    if (score >= 60) return 'warning';
    return 'error';
  };

  const getPerformanceIcon = (current: number, previous: number) => {
    if (current > previous) {
      return <TrendingUpIcon color="success" />;
    } else if (current < previous) {
      return <TrendingDownIcon color="error" />;
    }
    return null;
  };

  const formatDuration = (duration: string): string => {
    return duration.replace('PT', '').replace('H', 'h ').replace('M', 'm');
  };

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: 400,
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ m: 2 }}>
        {error}
      </Alert>
    );
  }

  const selectedRoute = selectedRouteId
    ? metrics.find(m => m.routeId === selectedRouteId)
    : null;

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" component="h1">
          <AssessmentIcon sx={{ mr: 2, verticalAlign: 'middle' }} />
          Route Efficiency Analytics
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Period</InputLabel>
            <Select
              value={selectedPeriod}
              label="Period"
              onChange={e => setSelectedPeriod(e.target.value)}
            >
              <MenuItem value="7days">Last 7 Days</MenuItem>
              <MenuItem value="30days">Last 30 Days</MenuItem>
              <MenuItem value="90days">Last 90 Days</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={loadEfficiencyMetrics}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {/* Overall Performance Summary */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <RouteIcon color="primary" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">{metrics.length}</Typography>
              <Typography variant="body2" color="textSecondary">
                Active Routes
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <TimeIcon color="success" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">
                {metrics.length > 0
                  ? Math.round(
                      metrics.reduce((sum, m) => sum + m.onTimePerformance, 0) /
                        metrics.length
                    )
                  : 0}
                %
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Avg On-Time Performance
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <FuelIcon color="warning" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">
                {metrics.length > 0
                  ? (
                      metrics.reduce((sum, m) => sum + m.fuelEfficiency, 0) /
                      metrics.length
                    ).toFixed(1)
                  : 0}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Avg Fuel Efficiency (MPG)
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <SpeedIcon color="info" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="h4">
                {metrics.length > 0
                  ? Math.round(
                      metrics.reduce((sum, m) => sum + m.optimizationScore, 0) /
                        metrics.length
                    )
                  : 0}
                %
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Avg Optimization Score
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Selected Route Details */}
      {selectedRoute && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Route Details: {selectedRoute.routeName}
            </Typography>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 4 }}>
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="textSecondary">
                    On-Time Performance
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <LinearProgress
                      variant="determinate"
                      value={selectedRoute.onTimePerformance}
                      color={getPerformanceColor(
                        selectedRoute.onTimePerformance
                      )}
                      sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
                    />
                    <Typography variant="body2">
                      {selectedRoute.onTimePerformance}%
                    </Typography>
                  </Box>
                </Box>
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="textSecondary">
                    Optimization Score
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <LinearProgress
                      variant="determinate"
                      value={selectedRoute.optimizationScore}
                      color={getPerformanceColor(
                        selectedRoute.optimizationScore
                      )}
                      sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
                    />
                    <Typography variant="body2">
                      {selectedRoute.optimizationScore}%
                    </Typography>
                  </Box>
                </Box>
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <Typography variant="body2" color="textSecondary">
                  Distance
                </Typography>
                <Typography variant="h6">
                  {selectedRoute.totalDistance} km
                </Typography>
                <Typography
                  variant="body2"
                  color="textSecondary"
                  sx={{ mt: 1 }}
                >
                  Duration
                </Typography>
                <Typography variant="h6">
                  {formatDuration(selectedRoute.estimatedDuration)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <Typography variant="body2" color="textSecondary">
                  Cost per Student
                </Typography>
                <Typography variant="h6">
                  {formatCurrency(selectedRoute.costPerStudent)}
                </Typography>
                <Typography
                  variant="body2"
                  color="textSecondary"
                  sx={{ mt: 1 }}
                >
                  Fuel Efficiency
                </Typography>
                <Typography variant="h6">
                  {selectedRoute.fuelEfficiency} MPG
                </Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}

      {/* Routes Performance Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Route Performance Metrics
          </Typography>
          <TableContainer component={Paper} variant="outlined">
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Route</TableCell>
                  <TableCell align="center">Stops</TableCell>
                  <TableCell align="center">Distance</TableCell>
                  <TableCell align="center">On-Time %</TableCell>
                  <TableCell align="center">Fuel Efficiency</TableCell>
                  <TableCell align="center">Cost/Student</TableCell>
                  <TableCell align="center">Optimization Score</TableCell>
                  <TableCell align="center">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {metrics.map(metric => (
                  <TableRow
                    key={metric.routeId}
                    selected={selectedRouteId === metric.routeId}
                    hover
                    onClick={() => onRouteSelect?.(metric.routeId)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {metric.routeName}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        Capacity: {metric.studentCapacity}
                      </Typography>
                    </TableCell>
                    <TableCell align="center">{metric.numberOfStops}</TableCell>
                    <TableCell align="center">
                      {metric.totalDistance} km
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={`${metric.onTimePerformance}%`}
                        color={getPerformanceColor(metric.onTimePerformance)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      {metric.fuelEfficiency} MPG
                    </TableCell>
                    <TableCell align="center">
                      {formatCurrency(metric.costPerStudent)}
                    </TableCell>
                    <TableCell align="center">
                      <Box
                        sx={{
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          gap: 1,
                        }}
                      >
                        <Chip
                          label={`${metric.optimizationScore}%`}
                          color={getPerformanceColor(metric.optimizationScore)}
                          size="small"
                        />
                      </Box>
                    </TableCell>
                    <TableCell align="center">
                      <Tooltip title="View Trends">
                        <IconButton size="small">
                          <TimelineIcon />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  );
};

export default RouteEfficiencyAnalytics;
