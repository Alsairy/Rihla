# Rihla School Transportation Management System

A comprehensive school transportation management platform built with .NET Core backend, React frontend, and React Native mobile app.

## 🚌 System Overview

Rihla is a production-ready school transportation management system that provides:
- **Admin Dashboard**: Complete system management and oversight
- **Parent Portal**: Track children's transportation and trip history
- **Driver Interface**: Manage assigned vehicles and daily trips
- **Mobile App**: Cross-platform mobile access for all user types

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js (v16 or higher)
- npm or yarn package manager

### Environment Setup

#### 1. Environment Variables Configuration

**Backend Environment Variables:**
```bash
# Copy the environment template
cp src/WebAPI/SchoolTransportationSystem.WebAPI/.env.example src/WebAPI/SchoolTransportationSystem.WebAPI/.env

# Edit the .env file with your configuration
# REQUIRED: Set JWT_SECRET_KEY (minimum 32 characters)
JWT_SECRET_KEY=your-very-secure-secret-key-at-least-32-characters-long
CONNECTION_STRING=Data Source=rihla.db
```

**Frontend Environment Variables:**
```bash
# Copy the environment template
cp src/Frontend/rihla-web/.env.example src/Frontend/rihla-web/.env.local

# Edit .env.local with your backend URL
REACT_APP_API_URL=http://localhost:5079
REACT_APP_ENVIRONMENT=development
```

#### 2. Database Setup
```bash
# Navigate to backend directory
cd src/WebAPI/SchoolTransportationSystem.WebAPI

# Apply database migrations
dotnet ef database update

# Seed database with initial data (optional)
dotnet run --seed-data
```

### Local Development

**IMPORTANT**: Both frontend and backend servers must be running for the authentication system to work properly.

#### Option 1: Automated Startup (Recommended)
```bash
# Run the automated startup script
./start_servers.sh
```

#### Option 2: Manual Startup
```bash
# Terminal 1: Start Backend Server (.NET API)
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet run --urls "http://localhost:5079"

# Terminal 2: Start Frontend Server (React)
cd src/Frontend/rihla-web
npm install  # Only needed first time
npm start

# Terminal 3: Start Mobile App (Optional)
cd src/Mobile/rihla-mobile
npm install  # Only needed first time
npx expo start
```

### Access URLs
- **Frontend Web App**: http://localhost:3000
- **Backend API**: http://localhost:5079
- **API Documentation**: http://localhost:5079/swagger
- **Mobile App**: Expo DevTools at http://localhost:8081

## 👥 Test Credentials

The system comes with pre-configured test users for each persona:

| Role | Email | Password | Dashboard URL |
|------|-------|----------|---------------|
| Admin | admin@rihla.com | password123 | http://localhost:3000/admin |
| Parent | parent@rihla.com | password123 | http://localhost:3000/parent |
| Driver | driver@rihla.com | password123 | http://localhost:3000/driver |

## 🏗️ System Architecture

### Backend (.NET Core)
- **Clean Architecture**: Domain, Application, Infrastructure, and WebAPI layers
- **Entity Framework Core**: Database ORM with SQLite for development
- **JWT Authentication**: Secure token-based authentication
- **SignalR**: Real-time notifications and updates
- **Swagger/OpenAPI**: Comprehensive API documentation

### Frontend (React + TypeScript)
- **Material-UI**: Modern, responsive user interface
- **React Router**: Client-side routing for SPA experience
- **Axios**: HTTP client for API communication
- **Context API**: State management for authentication

### Mobile App (React Native)
- **Cross-platform**: iOS and Android support
- **Expo**: Development and deployment platform
- **Native Base**: UI component library
- **Offline Support**: Local data caching and synchronization

## 🔧 Development

### Backend Development
```bash
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet watch run --urls "http://localhost:5079"
```

### Frontend Development
```bash
cd src/Frontend/rihla-web
npm start
```

### Mobile Development
```bash
cd src/Mobile/rihla-mobile
npm install
npx expo start
```

## 🚀 Deployment

### Staging Deployment

#### Prerequisites
- .NET 8.0 Runtime
- Node.js 18+ for frontend build
- PostgreSQL or SQL Server for production database
- SSL certificates for HTTPS

#### 1. Backend Staging Deployment
```bash
# Build the application
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet publish -c Release -o ./publish

# Set staging environment variables
export ASPNETCORE_ENVIRONMENT=Staging
export JWT_SECRET_KEY="your-staging-secret-key-minimum-32-characters"
export CONNECTION_STRING="your-staging-database-connection-string"

# Run database migrations
dotnet ef database update --connection "your-staging-connection-string"

# Start the application
cd publish
dotnet SchoolTransportationSystem.WebAPI.dll --urls "https://staging-api.yourdomain.com"
```

#### 2. Frontend Staging Deployment
```bash
# Build for staging
cd src/Frontend/rihla-web
npm install
REACT_APP_API_URL=https://staging-api.yourdomain.com npm run build

# Deploy build folder to your web server
# Example: Copy build/ contents to your web server's document root
```

### Production Deployment

#### 1. Backend Production Deployment

**Using Docker (Recommended):**
```bash
# Create Dockerfile in WebAPI directory
cd src/WebAPI/SchoolTransportationSystem.WebAPI

# Build Docker image
docker build -t rihla-backend:latest .

# Run with environment variables
docker run -d \
  --name rihla-backend \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_SECRET_KEY="your-production-secret-key-minimum-32-characters" \
  -e CONNECTION_STRING="your-production-database-connection-string" \
  rihla-backend:latest
```

**Manual Production Deployment:**
```bash
# Build for production
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet publish -c Release -o ./publish

# Set production environment variables
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET_KEY="your-production-secret-key-minimum-32-characters"
export CONNECTION_STRING="your-production-database-connection-string"

# Configure reverse proxy (nginx/Apache)
# Run database migrations
dotnet ef database update --connection "your-production-connection-string"

# Start with systemd service or process manager
```

#### 2. Frontend Production Deployment
```bash
# Build for production
cd src/Frontend/rihla-web
npm install
REACT_APP_API_URL=https://api.yourdomain.com npm run build

# Deploy to CDN or web server
# Example: Upload build/ contents to AWS S3, Netlify, or your web server
```

#### 3. Mobile App Production Deployment
```bash
# Build for production
cd src/Mobile/rihla-mobile

# For iOS
npx expo build:ios

# For Android
npx expo build:android

# Or use EAS Build (recommended)
npx eas build --platform all
```

### Database Migration Guide

#### Development to Staging Migration
```bash
# Create migration for new changes
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet ef migrations add YourMigrationName

# Apply to staging database
dotnet ef database update --connection "your-staging-connection-string"
```

#### Staging to Production Migration
```bash
# Backup production database first
# Apply migrations to production
dotnet ef database update --connection "your-production-connection-string"

# Seed production data if needed
dotnet run --seed-data --connection "your-production-connection-string"
```

### Environment-Specific Configuration

#### Development (.env)
```bash
JWT_SECRET_KEY=development-secret-key-minimum-32-characters
CONNECTION_STRING=Data Source=rihla.db
ASPNETCORE_ENVIRONMENT=Development
```

#### Staging (.env.staging)
```bash
JWT_SECRET_KEY=staging-secret-key-minimum-32-characters
CONNECTION_STRING=Server=staging-db;Database=RihlaStaging;Trusted_Connection=true;
ASPNETCORE_ENVIRONMENT=Staging
```

#### Production (.env.production)
```bash
JWT_SECRET_KEY=production-secret-key-minimum-32-characters
CONNECTION_STRING=Server=prod-db;Database=RihlaProd;Trusted_Connection=true;
ASPNETCORE_ENVIRONMENT=Production
```

## 📊 Features

### Admin Dashboard
- System statistics and analytics
- User management (Students, Parents, Drivers)
- Vehicle and route management
- Trip scheduling and monitoring
- Payment tracking
- Maintenance records
- Comprehensive reporting

### Parent Portal
- Child registration and profile management
- Real-time trip tracking
- Trip history and attendance records
- Payment management
- Driver contact information
- Notification preferences

### Driver Interface
- Assigned vehicle information
- Daily trip schedules
- Student pickup/dropoff management
- Trip status updates
- Maintenance reporting
- Route navigation

## 🔒 Security Features

- JWT-based authentication with refresh tokens
- Role-based authorization (Admin, Parent, Driver)
- CORS protection
- Input validation and sanitization
- Secure password hashing
- Session management

## 📱 Mobile Features

- Biometric authentication
- Offline data synchronization
- Push notifications
- GPS tracking
- Multi-language support (English/Arabic)
- Dark/Light theme support

## 🌐 Internationalization

The system supports multiple languages:
- English (default)
- Arabic (RTL support)

## 🚨 Troubleshooting

### Login Issues
If you encounter "Login failed" errors:
1. Ensure both backend and frontend servers are running
2. Check that backend is accessible at http://localhost:5079
3. Verify frontend is running at http://localhost:3000
4. Clear browser localStorage and try again
5. Verify JWT_SECRET_KEY environment variable is set

### Server Startup Issues
```bash
# Check if ports are in use
lsof -i :3000  # Frontend port
lsof -i :5079  # Backend port
lsof -i :8081  # Mobile Expo port

# Kill existing processes if needed
pkill -f "npm start"
pkill -f "dotnet run"
pkill -f "expo start"

# Restart using the startup script
./start_servers.sh
```

### Database Issues
```bash
# Reset database (development only)
cd src/WebAPI/SchoolTransportationSystem.WebAPI
rm -f rihla.db
dotnet ef database update  # Recreate database
dotnet run --seed-data     # Add seed data
```

### Environment Variable Issues
```bash
# Verify environment variables are loaded
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet user-secrets list

# Check frontend environment variables
cd src/Frontend/rihla-web
cat .env.local
```

### Production Deployment Issues

#### SSL Certificate Issues
```bash
# Verify SSL certificate
openssl x509 -in certificate.crt -text -noout

# Test HTTPS endpoint
curl -I https://api.yourdomain.com/health
```

#### Database Connection Issues
```bash
# Test database connection
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet ef database update --dry-run --connection "your-connection-string"
```

#### Performance Issues
```bash
# Monitor application performance
dotnet-counters monitor --process-id <pid>

# Check memory usage
dotnet-dump collect --process-id <pid>
```

## 📝 Logging and Monitoring

### Development Logs
When using the startup script, logs are available at:
- Backend: `/tmp/rihla_backend.log`
- Frontend: `/tmp/rihla_frontend.log`

### Production Logging
```bash
# Configure structured logging in appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "/var/log/rihla/app.log",
      "MinLevel": "Information"
    }
  }
}
```

### Health Checks
```bash
# Backend health check
curl http://localhost:5079/health

# Frontend health check
curl http://localhost:3000

# Database health check
curl http://localhost:5079/health/database
```

## 🛑 Stopping Servers

### Development
```bash
# Stop all servers
pkill -f "dotnet run" && pkill -f "npm start" && pkill -f "expo start"

# Or use Ctrl+C in each terminal window
```

### Production
```bash
# Stop Docker containers
docker stop rihla-backend rihla-frontend

# Stop systemd services
sudo systemctl stop rihla-backend
sudo systemctl stop rihla-frontend
```

## 🔒 Security Considerations

### Production Security Checklist
- [ ] Use strong JWT secret keys (minimum 32 characters)
- [ ] Enable HTTPS with valid SSL certificates
- [ ] Configure CORS for production domains only
- [ ] Use secure database connection strings
- [ ] Enable rate limiting and request throttling
- [ ] Configure proper firewall rules
- [ ] Regular security updates and patches
- [ ] Monitor application logs for security events
- [ ] Use environment variables for all sensitive data
- [ ] Enable database encryption at rest

### Environment Variables Security
```bash
# Never commit these to version control:
JWT_SECRET_KEY=
CONNECTION_STRING=
SMTP_PASSWORD=
SMS_AUTH_TOKEN=
AZURE_AD_CLIENT_SECRET=
```

## 📚 API Documentation

Once the backend server is running, visit http://localhost:5000/swagger for comprehensive API documentation with interactive testing capabilities.

## 🤝 Contributing

1. Ensure both servers start successfully using `./start_servers.sh`
2. Test all three user personas (Admin, Parent, Driver)
3. Verify authentication flows work correctly
4. Run tests before submitting changes

## 📄 License

This project is licensed under the MIT License.

---

**Note**: This system requires both frontend and backend servers to be running simultaneously for proper authentication functionality. Always use the startup script or ensure both servers are started manually before testing login functionality.
