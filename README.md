# GameDB

GameDB is a Blazor-based web application that allows users to discover games using the IGDB (Internet Game Database) API. Users can search for titles, view detailed information, save their favorite games to a personal bookmark collection, and leave ratings/reviews.

## Features

- **IGDB API Integration**: Search thousands of titles with real-time data.
- **Local Caching**: Search results are cached in a local MySQL database for improved performance.
- **User Authentication**: Secure sign-up and login system using BCrypt password hashing.
- **Personal Bookmarks**: Save games to your account to view them later on your dashboard.
- **Game Reviews**: Rate games on a scale of 1-10 and leave comments.

## Prerequisites

Before you begin, ensure you have the following installed on your machine:

1. **.NET 8.0 SDK**: [Download .NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
   - Verify by running `dotnet --version` in your terminal.
2. **MySQL Server**: Download MySQL
   - Ensure the server is running and you have your root (or a specific user) credentials handy.
3. **IGDB API Account**: Register for IGDB API at [their site](https://www.igdb.com/api)
   - You will need a **Client ID** and **Client Secret** from the Twitch Developer portal.

## Setup Instructions

### 1. Database Configuration
Create a new database in your MySQL instance:
```sql
CREATE DATABASE gamedb;
```

### 2. Environment Variables
The application uses a `.env` file to manage sensitive credentials. Make sure you have your variables configured before running the project


### 3. Install EF Core Tools
If you haven't installed the Entity Framework Core tools globally, run:
```bash
dotnet tool install --global dotnet-ef
```

### 4. Apply Database Migrations
Navigate to the project directory and apply the migrations to set up your table structure:
```bash
dotnet ef database update
```
This can be skipped if you manually build the table using the accompanying .sql file

### 5. Run the Application
Start the development server:
```bash
dotnet run
```
You can connect to the server using the link in the output, or to whatever you have configured in `Properties/launchSettings.json`  

## Project Structure

- **Components/**: Contains Razor components, including pages (`Home`, `Search`, `GameDetail`) and UI elements (`NavMenu`, `LoginModal`).
- **Data/**: Contains the `GameDbContext` and Entity Framework configurations.
- **Models/**: Defines the data structures for Games, Users, Reviews, and Bookmarks.
- **Services/**: Application logic, including `GameService` for database operations and `IGDBService` for external API calls.
- **wwwroot/**: Static assets including the global `app.css`.
