# 🚀 Rihla School Transportation System - Complete Beginner's Installation Guide

## 📖 What is this guide for?

This guide will help you install and run the Rihla School Transportation System on your computer, even if you have no technical experience. We'll explain everything step by step, including what each tool does and why you need it.

## 🎯 What you'll have when we're done

After following this guide, you'll have:
- A **backend server** (the brain of the system that handles data)
- A **frontend website** (the user interface you see in your browser)
- A **mobile app** (optional - for phones and tablets)
- A **database** (where all the information is stored)

---

## 📋 Step 1: Understanding What We Need to Install

Before we start, let's understand what each tool does:

### 🟢 Node.js (Required)
- **What it is**: A program that lets your computer run JavaScript code
- **Why we need it**: The frontend website and mobile app are built with JavaScript
- **Think of it as**: The engine that powers our website

### 🔵 .NET SDK (Required)
- **What it is**: Microsoft's programming toolkit
- **Why we need it**: The backend server is built with .NET
- **Think of it as**: The engine that powers our server

### 🟡 Git (Optional but recommended)
- **What it is**: A tool for downloading and managing code
- **Why we need it**: To download the Rihla code to your computer
- **Think of it as**: A way to copy files from the internet to your computer

---

## 📥 Step 2: Installing the Prerequisites

### 🟢 Installing Node.js

#### For Windows:
1. **Go to the Node.js website**: Open your web browser and visit https://nodejs.org/
2. **Download Node.js**: Click the big green button that says "Download Node.js" (it should show version 18 or higher)
3. **Run the installer**: 
   - Find the downloaded file (usually in your Downloads folder)
   - Double-click the file (it will be named something like `node-v18.x.x-x64.msi`)
   - Click "Next" through all the steps
   - Accept the license agreement
   - Keep all the default settings
   - Click "Install"
4. **Verify it worked**:
   - Press `Windows key + R`
   - Type `cmd` and press Enter
   - In the black window that opens, type: `node --version`
   - You should see something like `v18.17.0`

#### For Mac:
1. **Go to the Node.js website**: Visit https://nodejs.org/
2. **Download Node.js**: Click the button for macOS
3. **Run the installer**: 
   - Find the downloaded `.pkg` file
   - Double-click it and follow the installation steps
4. **Verify it worked**:
   - Press `Cmd + Space` and type "Terminal"
   - In the terminal window, type: `node --version`
   - You should see something like `v18.17.0`

### 🔵 Installing .NET SDK

#### For Windows:
1. **Go to Microsoft's website**: Visit https://dotnet.microsoft.com/download
2. **Download .NET 8.0 SDK**: Look for ".NET 8.0" and click "Download .NET SDK"
3. **Run the installer**:
   - Find the downloaded file (something like `dotnet-sdk-8.0.x-win-x64.exe`)
   - Double-click it
   - Follow the installation steps (just click "Next" and "Install")
4. **Verify it worked**:
   - Open Command Prompt (Windows key + R, type `cmd`)
   - Type: `dotnet --version`
   - You should see something like `8.0.100`

#### For Mac:
1. **Go to Microsoft's website**: Visit https://dotnet.microsoft.com/download
2. **Download .NET 8.0 SDK**: Click the macOS download button
3. **Run the installer**: Double-click the downloaded `.pkg` file
4. **Verify it worked**:
   - Open Terminal
   - Type: `dotnet --version`
   - You should see something like `8.0.100`

---

## 📁 Step 3: Getting the Rihla Code

### Option A: If you have the code already
If someone gave you a folder with the Rihla code, skip to Step 4.

### Option B: Download from GitHub
1. **Go to the Rihla repository**: Visit https://github.com/Alsairy/Rihla
2. **Download the code**:
   - Click the green "Code" button
   - Click "Download ZIP"
   - Save it to your Desktop or Documents folder
3. **Extract the files**:
   - Find the downloaded ZIP file
   - Right-click it and choose "Extract All" (Windows) or double-click it (Mac)
   - Remember where you extracted it!

---

## 🚀 Step 4: Running the Rihla System

Now we'll start the three parts of the system. You'll need to open multiple command windows.

### 🔵 Starting the Backend (The Server)

1. **Open a command window**:
   - **Windows**: Press Windows key + R, type `cmd`, press Enter
   - **Mac**: Press Cmd + Space, type "Terminal", press Enter

2. **Navigate to the backend folder**:
   ```bash
   cd path/to/your/Rihla/src/WebAPI/SchoolTransportationSystem.WebAPI
   ```
   **Replace "path/to/your/Rihla" with the actual location where you extracted the files**
   
   **Example for Windows**: `cd C:\Users\YourName\Desktop\Rihla\src\WebAPI\SchoolTransportationSystem.WebAPI`
   
   **Example for Mac**: `cd /Users/YourName/Desktop/Rihla/src/WebAPI/SchoolTransportationSystem.WebAPI`

3. **Install the backend dependencies**:
   ```bash
   dotnet restore
   ```
   **What this does**: Downloads all the code libraries the backend needs

4. **Build the backend**:
   ```bash
   dotnet build
   ```
   **What this does**: Prepares the backend code to run

5. **Start the backend server**:
   ```bash
   dotnet run
   ```
   **What this does**: Starts the server that handles all the data
   
   **You'll see**: Messages saying the server is running
   **Success sign**: You'll see "Now listening on: http://localhost:5078"

**⚠️ Important**: Keep this command window open! If you close it, the backend will stop working.

### 🟢 Starting the Frontend (The Website)

1. **Open a NEW command window** (keep the first one open):
   - **Windows**: Press Windows key + R, type `cmd`, press Enter
   - **Mac**: Press Cmd + Space, type "Terminal", press Enter

2. **Navigate to the frontend folder**:
   ```bash
   cd path/to/your/Rihla/src/Frontend/rihla-web
   ```
   **Replace "path/to/your/Rihla" with the actual location**

3. **Install the frontend dependencies**:
   ```bash
   npm install --legacy-peer-deps
   ```
   **What this does**: Downloads all the code libraries the website needs
   **This might take a few minutes**: You'll see lots of text scrolling by

4. **Start the website**:
   ```bash
   npm start
   ```
   **What this does**: Starts the website
   
   **You'll see**: Your web browser should automatically open to http://localhost:3000
   **Success sign**: You'll see the Rihla login page

**⚠️ Important**: Keep this command window open too!

### 📱 Starting the Mobile App (Optional)

If you want to run the mobile app:

1. **Open a THIRD command window**:

2. **Navigate to the mobile folder**:
   ```bash
   cd path/to/your/Rihla/src/Mobile/rihla-mobile
   ```

3. **Install mobile dependencies**:
   ```bash
   npm install --legacy-peer-deps
   ```

4. **Install Expo CLI** (needed for mobile apps):
   ```bash
   npm install -g @expo/cli
   ```

5. **Start the mobile app**:
   ```bash
   npx expo start
   ```

---

## 🔐 Step 5: Testing the System

### 🌐 Testing the Website
1. **Open your web browser** and go to: http://localhost:3000
2. **You should see**: A login page for Rihla
3. **Try logging in** with these test accounts:
   - **Admin**: Email: `admin@rihla.sa`, Password: `admin123`
   - **Driver**: Email: `driver@rihla.sa`, Password: `driver123`
   - **Parent**: Email: `parent@rihla.sa`, Password: `parent123`

### 🔧 Testing the Backend
1. **Open your web browser** and go to: http://localhost:5078/swagger
2. **You should see**: A page showing all the available API functions
3. **This means**: The backend is working correctly

---

## ❌ Troubleshooting Common Problems

### Problem: "Command not found" or "not recognized"
**Solution**: The tool isn't installed correctly
- Go back to Step 2 and reinstall Node.js or .NET SDK
- Make sure to restart your command window after installing

### Problem: "Port already in use"
**Solution**: Something else is using the same port
- Try closing other programs
- Or restart your computer and try again

### Problem: "npm install" fails
**Solution**: Try these commands one by one:
```bash
npm cache clean --force
npm install --legacy-peer-deps
```

### Problem: Website doesn't load
**Solution**: 
- Make sure the backend is running (you should see "listening on port 5078")
- Make sure the frontend is running (you should see "compiled successfully")
- Try refreshing your browser

### Problem: "dotnet: command not found"
**Solution**: .NET SDK isn't installed correctly
- Go back to Step 2 and reinstall .NET SDK
- Restart your command window

---

## 🎉 Success! What You Should See

When everything is working:

1. **Backend**: Command window shows "Now listening on: http://localhost:5078"
2. **Frontend**: Browser shows the Rihla login page at http://localhost:3000
3. **You can log in**: Using the test accounts provided above
4. **You can see data**: Student lists, driver information, vehicle details, etc.

---

## 🛑 How to Stop the System

When you're done testing:

1. **Stop the frontend**: In the frontend command window, press `Ctrl + C` (Windows) or `Cmd + C` (Mac)
2. **Stop the backend**: In the backend command window, press `Ctrl + C` (Windows) or `Cmd + C` (Mac)
3. **Close command windows**: You can now close all the command windows

---

## 📞 Getting Help

If you're still having trouble:

1. **Check the error messages**: Look for red text in your command windows
2. **Make sure all prerequisites are installed**: Go back to Step 2
3. **Try restarting**: Close everything and start over from Step 4
4. **Check your internet connection**: Some steps require downloading files

---

## 🎯 What's Next?

Once you have the system running:

- **Explore the admin dashboard**: See all the features for managing students, drivers, and vehicles
- **Try the parent portal**: See what parents can view about their children
- **Test the driver interface**: See the tools available for drivers
- **Look at the mobile app**: If you set it up, try it on your phone

**Congratulations!** You now have a complete school transportation management system running on your computer! 🎉
