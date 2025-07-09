#!/bin/bash


set -e  # Exit on any error

echo "🚌 Starting Rihla School Transportation System Servers..."
echo "=================================================="

check_port() {
    local port=$1
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

wait_for_server() {
    local url=$1
    local name=$2
    local max_attempts=30
    
    echo "⏳ Waiting for $name to start..."
    for i in $(seq 1 $max_attempts); do
        if curl -s "$url" > /dev/null 2>&1; then
            echo "✅ $name started successfully!"
            return 0
        fi
        if [ $i -eq $max_attempts ]; then
            echo "❌ Error: $name failed to start within $max_attempts seconds."
            return 1
        fi
        sleep 1
    done
}

if check_port 5000; then
    echo "ℹ️  Backend server already running on port 5000"
    BACKEND_RUNNING=true
else
    BACKEND_RUNNING=false
fi

if check_port 3000; then
    echo "ℹ️  Frontend server already running on port 3000"
    FRONTEND_RUNNING=true
else
    FRONTEND_RUNNING=false
fi

if [ "$BACKEND_RUNNING" = false ]; then
    echo "🔧 Starting backend server (.NET API)..."
    cd /home/ubuntu/repos/Rihla/src/WebAPI/SchoolTransportationSystem.WebAPI
    
    if ! command -v dotnet &> /dev/null; then
        echo "❌ Error: .NET SDK is not installed. Please install .NET 8.0 SDK."
        exit 1
    fi
    
    nohup dotnet run --urls "http://localhost:5000" > /tmp/rihla_backend.log 2>&1 &
    BACKEND_PID=$!
    echo "🔧 Backend server started with PID: $BACKEND_PID"
    
    if ! wait_for_server "http://localhost:5000/api/auth/login" "Backend API"; then
        echo "❌ Backend server startup failed. Check logs at /tmp/rihla_backend.log"
        kill $BACKEND_PID 2>/dev/null || true
        exit 1
    fi
else
    echo "✅ Backend server already running"
fi

if [ "$FRONTEND_RUNNING" = false ]; then
    echo "🎨 Starting frontend server (React)..."
    cd /home/ubuntu/repos/Rihla/src/Frontend/rihla-web
    
    if ! command -v npm &> /dev/null; then
        echo "❌ Error: Node.js/npm is not installed. Please install Node.js."
        exit 1
    fi
    
    if [ ! -d "node_modules" ]; then
        echo "📦 Installing frontend dependencies..."
        npm install
    fi
    
    nohup npm start > /tmp/rihla_frontend.log 2>&1 &
    FRONTEND_PID=$!
    echo "🎨 Frontend server started with PID: $FRONTEND_PID"
    
    if ! wait_for_server "http://localhost:3000" "Frontend React App"; then
        echo "❌ Frontend server startup failed. Check logs at /tmp/rihla_frontend.log"
        kill $FRONTEND_PID 2>/dev/null || true
        [ "$BACKEND_RUNNING" = false ] && kill $BACKEND_PID 2>/dev/null || true
        exit 1
    fi
else
    echo "✅ Frontend server already running"
fi

echo ""
echo "🎉 Rihla School Transportation System is now running!"
echo "=================================================="
echo "🔧 Backend API:     http://localhost:5000"
echo "🎨 Frontend Web:    http://localhost:3000"
echo ""
echo "📋 Test Credentials:"
echo "   Admin:  admin@rihla.com / password123"
echo "   Parent: parent@rihla.com / password123"
echo "   Driver: driver@rihla.com / password123"
echo ""
echo "📝 Server Logs:"
echo "   Backend: /tmp/rihla_backend.log"
echo "   Frontend: /tmp/rihla_frontend.log"
echo ""
echo "🛑 To stop servers: pkill -f 'dotnet run' && pkill -f 'npm start'"
echo "=================================================="
