import React, { useEffect, useState } from 'react';
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Polyline,
} from 'react-leaflet';
import { Icon, LatLngExpression } from 'leaflet';
import { Box, Typography, Chip, CircularProgress, Alert } from '@mui/material';
import { apiClient } from '../services/apiClient';
import 'leaflet/dist/leaflet.css';

delete (Icon.Default.prototype as any)._getIconUrl;
Icon.Default.mergeOptions({
  iconRetinaUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const createVehicleIcon = (status: string) => {
  const color =
    status === 'Active'
      ? '#4CAF50'
      : status === 'InTransit'
        ? '#FF9800'
        : status === 'Maintenance'
          ? '#F44336'
          : '#9E9E9E';

  return new Icon({
    iconUrl: `data:image/svg+xml;base64,${btoa(`
      <svg width="25" height="25" viewBox="0 0 25 25" xmlns="http://www.w3.org/2000/svg">
        <circle cx="12.5" cy="12.5" r="10" fill="${color}" stroke="#fff" stroke-width="2"/>
        <rect x="8" y="10" width="9" height="6" fill="white" rx="1"/>
        <rect x="9" y="11" width="2" height="2" fill="${color}"/>
        <rect x="14" y="11" width="2" height="2" fill="${color}"/>
      </svg>
    `)}`,
    iconSize: [25, 25],
    iconAnchor: [12, 12],
    popupAnchor: [0, -12],
  });
};

const studentIcon = new Icon({
  iconUrl: `data:image/svg+xml;base64,${btoa(`
    <svg width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
      <circle cx="10" cy="10" r="8" fill="#2196F3" stroke="#fff" stroke-width="2"/>
      <circle cx="10" cy="8" r="2.5" fill="white"/>
      <path d="M6 14 Q10 12 14 14" stroke="white" stroke-width="2" fill="none"/>
    </svg>
  `)}`,
  iconSize: [20, 20],
  iconAnchor: [10, 10],
  popupAnchor: [0, -10],
});

interface Vehicle {
  id: string;
  plateNumber: string;
  model: string;
  capacity: number;
  status: string;
  currentLatitude?: number;
  currentLongitude?: number;
  driverId?: string;
  driverName?: string;
}

interface Route {
  id: string;
  name: string;
  description: string;
  waypoints: Array<{
    latitude: number;
    longitude: number;
    address: string;
    order: number;
  }>;
  vehicleId?: string;
}

interface Student {
  id: string;
  fullName: string;
  pickupLatitude?: number;
  pickupLongitude?: number;
  pickupAddress?: string;
  routeId?: string;
}

interface MapComponentProps {
  center?: LatLngExpression;
  zoom?: number;
  height?: string | number;
  showVehicles?: boolean;
  showRoutes?: boolean;
  showStudents?: boolean;
  selectedVehicleId?: string;
  selectedRouteId?: string;
}

const MapComponent: React.FC<MapComponentProps> = ({
  center = [24.7136, 46.6753], // Riyadh, Saudi Arabia
  zoom = 11,
  height = 400,
  showVehicles = true,
  showRoutes = true,
  showStudents = false,
  selectedVehicleId,
  selectedRouteId,
}) => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [routes, setRoutes] = useState<Route[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadMapData();
  }, []);

  const loadMapData = async () => {
    try {
      setLoading(true);
      setError(null);

      const promises: Promise<any>[] = [];

      if (showVehicles) {
        promises.push(apiClient.get<Vehicle[]>('/api/vehicles'));
      }

      if (showRoutes) {
        promises.push(apiClient.get<Route[]>('/api/routes'));
      }

      if (showStudents) {
        promises.push(apiClient.get<Student[]>('/api/students'));
      }

      const results = await Promise.all(promises);

      let resultIndex = 0;
      if (showVehicles) {
        setVehicles(results[resultIndex] || []);
        resultIndex++;
      }
      if (showRoutes) {
        setRoutes(results[resultIndex] || []);
        resultIndex++;
      }
      if (showStudents) {
        setStudents(results[resultIndex] || []);
      }
    } catch (err) {
      console.error('Failed to load map data:', err);
      setError('Failed to load map data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Active':
        return 'success';
      case 'InTransit':
        return 'warning';
      case 'Maintenance':
        return 'error';
      default:
        return 'default';
    }
  };

  const filteredVehicles = selectedVehicleId
    ? vehicles.filter(v => v.id === selectedVehicleId)
    : vehicles;

  const filteredRoutes = selectedRouteId
    ? routes.filter(r => r.id === selectedRouteId)
    : routes;

  if (loading) {
    return (
      <Box
        display="flex"
        justifyContent="center"
        alignItems="center"
        height={height}
        bgcolor="grey.100"
        borderRadius={1}
      >
        <CircularProgress />
        <Typography variant="body2" sx={{ ml: 2 }}>
          Loading map data...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ height: height }}>
        {error}
      </Alert>
    );
  }

  return (
    <Box sx={{ height, width: '100%', borderRadius: 1, overflow: 'hidden' }}>
      <MapContainer
        center={center}
        zoom={zoom}
        style={{ height: '100%', width: '100%' }}
        scrollWheelZoom={true}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {/* Vehicle Markers */}
        {showVehicles &&
          filteredVehicles.map(vehicle => {
            if (!vehicle.currentLatitude || !vehicle.currentLongitude)
              return null;

            return (
              <Marker
                key={vehicle.id}
                position={[vehicle.currentLatitude, vehicle.currentLongitude]}
                icon={createVehicleIcon(vehicle.status)}
              >
                <Popup>
                  <Box sx={{ minWidth: 200 }}>
                    <Typography variant="h6" gutterBottom>
                      🚌 {vehicle.plateNumber}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Model:</strong> {vehicle.model}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Capacity:</strong> {vehicle.capacity} students
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Driver:</strong>{' '}
                      {vehicle.driverName || 'Not assigned'}
                    </Typography>
                    <Box sx={{ mt: 1 }}>
                      <Chip
                        label={vehicle.status}
                        color={getStatusColor(vehicle.status) as any}
                        size="small"
                      />
                    </Box>
                  </Box>
                </Popup>
              </Marker>
            );
          })}

        {/* Route Polylines */}
        {showRoutes &&
          filteredRoutes.map(route => {
            if (!route.waypoints || route.waypoints.length < 2) return null;

            const positions: LatLngExpression[] = route.waypoints
              .sort((a, b) => a.order - b.order)
              .map(wp => [wp.latitude, wp.longitude]);

            return (
              <Polyline
                key={route.id}
                positions={positions}
                color="#2196F3"
                weight={4}
                opacity={0.7}
              >
                <Popup>
                  <Box sx={{ minWidth: 200 }}>
                    <Typography variant="h6" gutterBottom>
                      🛣️ {route.name}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {route.description}
                    </Typography>
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{ mt: 1 }}
                    >
                      <strong>Waypoints:</strong> {route.waypoints.length}
                    </Typography>
                  </Box>
                </Popup>
              </Polyline>
            );
          })}

        {/* Route Waypoint Markers */}
        {showRoutes &&
          filteredRoutes.map(route =>
            route.waypoints?.map((waypoint, index) => (
              <Marker
                key={`${route.id}-waypoint-${index}`}
                position={[waypoint.latitude, waypoint.longitude]}
                icon={
                  new Icon({
                    iconUrl: `data:image/svg+xml;base64,${btoa(`
                  <svg width="20" height="20" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
                    <circle cx="10" cy="10" r="8" fill="#FF5722" stroke="#fff" stroke-width="2"/>
                    <text x="10" y="14" text-anchor="middle" fill="white" font-size="8" font-family="Arial">${waypoint.order}</text>
                  </svg>
                `)}`,
                    iconSize: [20, 20],
                    iconAnchor: [10, 10],
                    popupAnchor: [0, -10],
                  })
                }
              >
                <Popup>
                  <Box sx={{ minWidth: 150 }}>
                    <Typography variant="subtitle2" gutterBottom>
                      📍 Stop #{waypoint.order}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {waypoint.address}
                    </Typography>
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{ mt: 1, display: 'block' }}
                    >
                      Route: {route.name}
                    </Typography>
                  </Box>
                </Popup>
              </Marker>
            ))
          )}

        {/* Student Pickup Locations */}
        {showStudents &&
          students.map(student => {
            if (!student.pickupLatitude || !student.pickupLongitude)
              return null;

            return (
              <Marker
                key={student.id}
                position={[student.pickupLatitude, student.pickupLongitude]}
                icon={studentIcon}
              >
                <Popup>
                  <Box sx={{ minWidth: 150 }}>
                    <Typography variant="h6" gutterBottom>
                      👤 {student.fullName}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Pickup:</strong>{' '}
                      {student.pickupAddress || 'Address not set'}
                    </Typography>
                    {student.routeId && (
                      <Typography variant="body2" color="text.secondary">
                        <strong>Route:</strong> {student.routeId}
                      </Typography>
                    )}
                  </Box>
                </Popup>
              </Marker>
            );
          })}
      </MapContainer>
    </Box>
  );
};

export default MapComponent;
