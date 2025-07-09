# Environment Configuration Guide

## Overview
This guide explains how to configure environment variables for all components of the Rihla School Transportation System.

## Frontend Configuration

### Development Environment
Create `src/Frontend/rihla-web/.env`:
```env
REACT_APP_API_URL=http://localhost:5078
REACT_APP_ENVIRONMENT=development
REACT_APP_APP_NAME=Rihla School Transportation
REACT_APP_VERSION=1.0.0
```

### Production Environment
Create `src/Frontend/rihla-web/.env.production`:
```env
REACT_APP_API_URL=https://api.rihla.com
REACT_APP_ENVIRONMENT=production
REACT_APP_APP_NAME=Rihla School Transportation
REACT_APP_VERSION=1.0.0
REACT_APP_ENABLE_ANALYTICS=true
REACT_APP_SENTRY_DSN=your-sentry-dsn-here
```

## Backend Configuration

### Development Environment
The `appsettings.Development.json` file contains:
- SQLite database connection for local development
- JWT settings with development secret key
- Detailed logging configuration

### Production Environment
The `appsettings.Production.json` file uses environment variables:
- `${DB_PASSWORD}` - Database password
- `${JWT_SECRET_KEY}` - JWT secret key (minimum 32 characters)

### Required Environment Variables for Production
```bash
DB_PASSWORD=your-secure-database-password
JWT_SECRET_KEY=your-super-secure-jwt-secret-key-minimum-32-characters
```

## Mobile App Configuration

### Development Environment
Create `src/Mobile/rihla-mobile/.env`:
```env
API_BASE_URL=http://localhost:5078/api
APP_NAME=Rihla Mobile
APP_VERSION=1.0.0
ENVIRONMENT=development
```

### Production Environment
Create `src/Mobile/rihla-mobile/.env.production`:
```env
API_BASE_URL=https://api.rihla.com/api
APP_NAME=Rihla Mobile
APP_VERSION=1.0.0
ENVIRONMENT=production
ENABLE_ANALYTICS=true
SENTRY_DSN=your-sentry-dsn-here
```

## Security Best Practices

1. **Never commit sensitive data** to version control
2. **Use strong JWT secret keys** (minimum 32 characters)
3. **Rotate secrets regularly** in production
4. **Use HTTPS** for all production API endpoints
5. **Validate environment variables** on application startup

## Port Configuration

- **Backend API**: Port 5078 (configured in launchSettings.json)
- **Frontend Dev Server**: Port 3000 (default React)
- **Mobile Development**: Uses backend API on localhost:5078

## Database Configuration

### Development
- Uses SQLite database (`rihla.db`)
- No additional setup required

### Production
- Supports SQL Server, PostgreSQL, or other providers
- Update connection string in `appsettings.Production.json`
- Ensure database migrations are applied

## Troubleshooting

### Common Issues
1. **API connection failed**: Verify backend is running on correct port
2. **CORS errors**: Check AllowedHosts configuration
3. **JWT authentication failed**: Verify JWT secret key matches across environments
4. **Mobile app can't connect**: Ensure API_BASE_URL is correct

### Verification Commands
```bash
# Check backend health
curl http://localhost:5078/api/health

# Verify frontend environment
npm run build

# Test mobile API connection
# Check mobile app logs for API calls
```
