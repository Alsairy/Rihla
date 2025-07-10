# Rihla School Transportation System - Dependency Commands for Codex Environment Setup

## System Requirements

### Core Dependencies
- **Node.js**: Version 18.x (LTS)
- **.NET SDK**: Version 8.0.x
- **SQL Server**: 2019 or later (or SQL Server Express)
- **Git**: Latest version

## Installation Commands

### 1. .NET SDK Installation
```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Windows (PowerShell)
winget install Microsoft.DotNet.SDK.8

# macOS
brew install --cask dotnet-sdk

# Verify installation
dotnet --version
```

### 2. Node.js Installation
```bash
# Using Node Version Manager (recommended)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.0/install.sh | bash
source ~/.bashrc
nvm install 18
nvm use 18

# Ubuntu/Debian (alternative)
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Windows
winget install OpenJS.NodeJS.LTS

# macOS
brew install node@18

# Verify installation
node --version
npm --version
```

### 3. SQL Server Installation
```bash
# Ubuntu/Debian - SQL Server Express
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
sudo apt-get update
sudo apt-get install -y mssql-server
sudo /opt/mssql/bin/mssql-conf setup

# Windows - SQL Server Express (download installer)
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# macOS - Use Docker
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

## Project Setup Commands

### 1. Clone Repository
```bash
git clone https://github.com/Alsairy/Rihla.git
cd Rihla
```

### 2. Backend Setup (.NET Web API)
```bash
cd src/WebAPI/SchoolTransportationSystem.WebAPI

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run database migrations
dotnet ef database update

# Start the API server
dotnet run
```

### 3. Frontend Setup (React Web App)
```bash
cd src/Frontend/rihla-web

# Install dependencies
npm install --legacy-peer-deps

# Start development server
npm start

# Build for production
npm run build

# Run tests
npm test

# Run linting
npm run lint

# Format code
npm run format
```

### 4. Mobile App Setup (React Native with Expo)
```bash
cd src/Mobile/rihla-mobile

# Install Expo CLI globally
npm install -g @expo/cli

# Install dependencies
npm install --legacy-peer-deps

# Start Expo development server
npm start

# Run on Android
npm run android

# Run on iOS
npm run ios

# Run tests
npm test
```

## Development Tools Installation

### Essential Tools
```bash
# Git (if not already installed)
sudo apt-get install git  # Ubuntu/Debian
winget install Git.Git    # Windows
brew install git          # macOS

# Visual Studio Code
sudo snap install code --classic  # Ubuntu
winget install Microsoft.VisualStudioCode  # Windows
brew install --cask visual-studio-code     # macOS

# Postman (API testing)
sudo snap install postman  # Ubuntu
winget install Postman.Postman  # Windows
brew install --cask postman     # macOS
```

### VS Code Extensions (Recommended)
```bash
# Install via VS Code or command line
code --install-extension ms-dotnettools.csharp
code --install-extension ms-vscode.vscode-typescript-next
code --install-extension esbenp.prettier-vscode
code --install-extension ms-vscode.vscode-eslint
code --install-extension ms-mssql.mssql
code --install-extension expo.vscode-expo-tools
```

## Environment Configuration

### 1. Backend Environment Variables
Create `src/WebAPI/SchoolTransportationSystem.WebAPI/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RihlaDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here-minimum-32-characters",
    "Issuer": "RihlaAPI",
    "Audience": "RihlaClient",
    "ExpirationMinutes": 60
  }
}
```

### 2. Frontend Environment Variables
Create `src/Frontend/rihla-web/.env.local`:
```env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000/notificationHub
REACT_APP_ENVIRONMENT=development
```

### 3. Mobile Environment Variables
Create `src/Mobile/rihla-mobile/.env`:
```env
API_URL=http://localhost:5000
EXPO_PUBLIC_API_URL=http://localhost:5000
```

## Package Dependencies Summary

### Backend (.NET 8.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.AspNetCore.SignalR (1.2.0)
- Swashbuckle.AspNetCore (6.5.0)
- Serilog.AspNetCore (8.0.0)

### Frontend (React 19.1.0)
- @mui/material (7.2.0)
- @mui/icons-material (7.2.0)
- axios (1.10.0)
- react-router-dom (7.6.3)
- typescript (5.0.0)
- @microsoft/signalr (8.0.7)

### Mobile (React Native 0.72.10)
- expo (~49.0.0)
- @react-navigation/native (6.1.7)
- react-native-maps (1.7.1)
- axios (1.5.0)
- expo-location (~16.1.0)

## Verification Commands

### Check All Installations
```bash
# Verify .NET
dotnet --version

# Verify Node.js
node --version
npm --version

# Verify Git
git --version

# Test SQL Server connection (replace with your connection string)
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

### Test Project Builds
```bash
# Test backend build
cd src/WebAPI/SchoolTransportationSystem.WebAPI
dotnet build

# Test frontend build
cd src/Frontend/rihla-web
npm run build

# Test mobile project
cd src/Mobile/rihla-mobile
expo doctor
```

## Troubleshooting

### Common Issues
1. **Node.js version conflicts**: Use nvm to manage Node.js versions
2. **SQL Server connection issues**: Ensure SQL Server is running and connection string is correct
3. **Port conflicts**: Default ports are 5000 (API), 3000 (Frontend), 19000 (Expo)
4. **Package installation failures**: Use `--legacy-peer-deps` flag with npm

### Support Commands
```bash
# Clear npm cache
npm cache clean --force

# Reset node_modules
rm -rf node_modules package-lock.json
npm install --legacy-peer-deps

# Clear .NET cache
dotnet nuget locals all --clear

# Reset database
dotnet ef database drop
dotnet ef database update
```

## CI/CD Pipeline Requirements
The project uses GitHub Actions with the following requirements:
- Node.js 18.x
- .NET 8.0.x
- npm with --legacy-peer-deps flag
- SQL Server for integration tests

All commands above are tested and verified to work with the Rihla project structure.
