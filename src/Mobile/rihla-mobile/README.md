# Rihla Mobile App

A React Native mobile application for the Rihla School Transportation Management System.

## Features

- **Authentication**: Secure login with role-based access
- **Dashboard**: Real-time overview of transportation metrics
- **Student Management**: View and manage student information
- **Trip Tracking**: Monitor active trips and schedules
- **Profile Management**: User settings and preferences
- **Offline Support**: Basic functionality when offline
- **Push Notifications**: Real-time alerts and updates

## Tech Stack

- **React Native**: Cross-platform mobile development
- **Expo**: Development platform and tools
- **React Navigation**: Navigation and routing
- **Axios**: HTTP client for API calls
- **AsyncStorage**: Local data persistence
- **Expo Vector Icons**: Icon library

## Getting Started

### Prerequisites

- Node.js (v16 or higher)
- npm or yarn
- Expo CLI
- iOS Simulator (for iOS development)
- Android Studio (for Android development)

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm start
```

3. Run on specific platform:
```bash
# iOS
npm run ios

# Android
npm run android

# Web
npm run web
```

## Project Structure

```
src/
├── screens/          # Screen components
│   ├── LoginScreen.js
│   ├── DashboardScreen.js
│   ├── StudentsScreen.js
│   ├── TripsScreen.js
│   └── ProfileScreen.js
├── contexts/         # React contexts
│   └── AuthContext.js
├── services/         # API services
│   ├── apiClient.js
│   └── authService.js
├── components/       # Reusable components
└── utils/           # Utility functions
```

## API Integration

The app connects to the Rihla backend API running on `http://localhost:5000/api`. 

### Available Endpoints

- `GET /students` - Fetch students list
- `POST /auth/login` - User authentication
- `GET /trips` - Fetch trips data
- `GET /dashboard` - Dashboard statistics

## Features by Screen

### Login Screen
- Email/password authentication
- Demo credentials display
- Form validation
- Loading states

### Dashboard Screen
- Key metrics overview
- Quick action buttons
- Recent alerts
- Pull-to-refresh

### Students Screen
- Students list with search
- Student details modal
- Contact actions
- Filter and sort options

### Trips Screen
- Trip status filtering
- Real-time trip tracking
- Driver contact
- Route information

### Profile Screen
- User information
- App preferences
- Settings toggles
- Logout functionality

## Development

### Adding New Screens

1. Create screen component in `src/screens/`
2. Add navigation route in `App.js`
3. Update tab navigator if needed

### API Integration

1. Add service methods in `src/services/`
2. Use in screen components with proper error handling
3. Implement loading states

### Styling

- Uses React Native StyleSheet
- Consistent color scheme
- Responsive design principles
- iOS and Android platform differences

## Building for Production

### iOS

```bash
expo build:ios
```

### Android

```bash
expo build:android
```

## Environment Variables

Create `.env` file:

```
API_BASE_URL=https://api.rihla.sa
EXPO_PUBLIC_API_URL=https://api.rihla.sa
```

## Testing

```bash
npm test
```

## Contributing

1. Fork the repository
2. Create feature branch
3. Make changes
4. Test thoroughly
5. Submit pull request

## License

MIT License - see LICENSE file for details.

