# 🚀 RIHLA SYSTEM - COMPLETE INSTALLATION GUIDE

## Prerequisites Installation

### 1. Install Node.js (Required for Frontend & Mobile)
```bash
# Download and install Node.js 18+ from https://nodejs.org/
# Verify installation
node --version  # Should be 18.0 or higher
npm --version   # Should be 9.0 or higher
```

### 2. Install .NET SDK (Required for Backend)
```bash
# Download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download
# Verify installation
dotnet --version  # Should be 8.0 or higher
```

### 3. Install Git (Optional but Recommended)
```bash
# Download from https://git-scm.com/downloads
# Verify installation
git --version
```

## 📦 System Installation

### Step 1: Extract the Complete System
```bash
# Extract the RihlaCompleteSystem package to your desired location
# Example: C:\Projects\RihlaCompleteSystem (Windows) or ~/Projects/RihlaCompleteSystem (Mac/Linux)
```

### Step 2: Backend Installation & Setup
```bash
# Navigate to backend directory
cd src/WebAPI/SchoolTransportationSystem.WebAPI

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run database migrations (if needed)
dotnet ef database update

# Start the backend server
dotnet run

# Backend will be available at: http://localhost:5078
# API Documentation: http://localhost:5078/swagger
```

### Step 3: Frontend Installation & Setup
```bash
# Open a new terminal/command prompt
# Navigate to frontend directory
cd src/Frontend/school-transport-frontend

# Install npm dependencies
npm install

# Start the development server
npm run dev

# Frontend will be available at: http://localhost:5173
```

### Step 4: Mobile App Installation & Setup (Optional)
```bash
# Open a new terminal/command prompt
# Navigate to mobile directory
cd src/Mobile/rihla-mobile

# Install dependencies
npm install

# Install Expo CLI globally (if not already installed)
npm install -g @expo/cli

# Start the Expo development server
npx expo start

# Follow the Expo instructions to run on device or simulator
```

## 🔧 Configuration

### Backend Configuration
Edit `src/WebAPI/SchoolTransportationSystem.WebAPI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=rihla.db"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "RihlaSystem",
    "Audience": "RihlaUsers",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Frontend Configuration
Edit `src/Frontend/school-transport-frontend/.env`:
```env
VITE_API_BASE_URL=http://localhost:5078/api
VITE_APP_NAME=Rihla School Transportation
VITE_APP_VERSION=1.0.0
```

### Mobile App Configuration
Edit `src/Mobile/rihla-mobile/.env`:
```env
API_BASE_URL=http://localhost:5078/api
APP_NAME=Rihla Mobile
```

## 🗄️ Database Setup

### SQLite Database (Default - Included)
- **File**: `src/WebAPI/SchoolTransportationSystem.WebAPI/rihla.db`
- **Status**: Pre-configured with sample data
- **Action**: No additional setup required

### SQL Server Setup (Production Alternative)
```bash
# 1. Install SQL Server or SQL Server Express
# 2. Create a new database named 'RihlaDB'
# 3. Update connection string in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RihlaDB;Trusted_Connection=true;TrustServerCertificate=true;"
}

# 4. Run migrations
dotnet ef database update
```

### PostgreSQL Setup (Production Alternative)
```bash
# 1. Install PostgreSQL
# 2. Create a new database named 'rihladb'
# 3. Update connection string in appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=rihladb;Username=postgres;Password=yourpassword"
}

# 4. Install PostgreSQL provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# 5. Run migrations
dotnet ef database update
```

## 🔐 Security Configuration

### JWT Token Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-very-secure-secret-key-at-least-32-characters-long",
    "Issuer": "RihlaSystem",
    "Audience": "RihlaUsers",
    "ExpirationMinutes": 60
  }
}
```

### CORS Configuration (if needed)
In `Program.cs`, CORS is already configured for development. For production:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## 🌐 Production Deployment

### Frontend Production Build
```bash
cd src/Frontend/school-transport-frontend

# Create production build
npm run build

# The build folder will contain the production-ready files
# Deploy the 'dist' folder to your web server
```

### Backend Production Deployment
```bash
cd src/WebAPI/SchoolTransportationSystem.WebAPI

# Create production build
dotnet publish -c Release -o ./publish

# Deploy the 'publish' folder to your server
# Configure IIS, Apache, or Nginx to serve the application
```

### Mobile App Production Build
```bash
cd src/Mobile/rihla-mobile

# For Android
npx expo build:android

# For iOS
npx expo build:ios

# Follow Expo documentation for app store deployment
```

## 🔧 Troubleshooting

### Common Issues & Solutions

#### Backend Issues
**Issue**: "Unable to connect to database"
```bash
# Solution: Check database file permissions and path
ls -la rihla.db
# Ensure the file exists and is readable
```

**Issue**: "Port 5078 already in use"
```bash
# Solution: Change port in launchSettings.json or kill existing process
netstat -ano | findstr :5078  # Windows
lsof -i :5078                 # Mac/Linux
```

#### Frontend Issues
**Issue**: "Module not found" errors
```bash
# Solution: Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

**Issue**: "API connection failed"
```bash
# Solution: Verify backend is running and check .env file
curl http://localhost:5078/api/health
```

#### Mobile App Issues
**Issue**: "Expo CLI not found"
```bash
# Solution: Install Expo CLI globally
npm install -g @expo/cli
```

**Issue**: "Metro bundler issues"
```bash
# Solution: Clear Metro cache
npx expo start --clear
```

## 📊 Verification Steps

### 1. Backend Verification
```bash
# Check if backend is running
curl http://localhost:5078/api/health

# Check API documentation
# Open browser: http://localhost:5078/swagger
```

### 2. Frontend Verification
```bash
# Check if frontend is accessible
# Open browser: http://localhost:5173

# Login with demo credentials:
# Email: admin@rihla.sa
# Password: admin123
```

### 3. Database Verification
```bash
# Check if database file exists
ls -la src/WebAPI/SchoolTransportationSystem.WebAPI/rihla.db

# Check database tables (using SQLite browser or command line)
sqlite3 rihla.db ".tables"
```

## 🔄 Updates & Maintenance

### Updating Dependencies
```bash
# Backend dependencies
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet list package --outdated
dotnet add package [PackageName] --version [Version]

# Frontend dependencies
cd src/Frontend/school-transport-frontend
npm outdated
npm update

# Mobile dependencies
cd src/Mobile/rihla-mobile
npm outdated
npm update
```

### Database Migrations
```bash
# Create new migration
dotnet ef migrations add [MigrationName]

# Apply migrations
dotnet ef database update

# Rollback migration
dotnet ef database update [PreviousMigrationName]
```

## 📞 Support & Resources

### Documentation
- **System Overview**: SYSTEM_SUMMARY.md
- **API Documentation**: Available at /swagger when backend is running
- **Design System**: design_system/ folder
- **Deployment Guide**: DEPLOYMENT_GUIDE.md

### Demo Credentials
- **Admin**: admin@rihla.sa / admin123
- **Driver**: driver@rihla.sa / driver123
- **Parent**: parent@rihla.sa / parent123

### Useful Commands
```bash
# Check system status
dotnet --info                    # .NET information
node --version && npm --version  # Node.js information
git --version                    # Git information

# Development servers
dotnet run                       # Start backend
npm run dev                      # Start frontend
npx expo start                   # Start mobile app

# Production builds
dotnet publish -c Release        # Backend production build
npm run build                    # Frontend production build
npx expo build:android          # Android production build
```

---

## ✅ Installation Checklist

- [ ] Node.js 18+ installed
- [ ] .NET 8.0 SDK installed
- [ ] System package extracted
- [ ] Backend dependencies restored (`dotnet restore`)
- [ ] Frontend dependencies installed (`npm install`)
- [ ] Backend server running (http://localhost:5078)
- [ ] Frontend server running (http://localhost:5173)
- [ ] Database accessible (rihla.db exists)
- [ ] Login successful with demo credentials
- [ ] All modules accessible through navigation
- [ ] API documentation accessible (/swagger)

**Once all items are checked, your Rihla system is ready for use!**

