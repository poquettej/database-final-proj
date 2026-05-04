# Database and IGDB Setup Guide

## Prerequisites

1. **MySQL Server** - Download from https://dev.mysql.com/downloads/mysql/
2. **IGDB API Access** - Register at https://api.igdb.com/

## Step 1: Configure IGDB API

1. Sign up at the [IGDB API](https://api.igdb.com/) website
2. Get your **Client ID** and **Access Token** from the IGDB dashboard
3. Update `appsettings.json` with your credentials:

```json
"IGDBSettings": {
  "BaseUrl": "https://api.igdb.com/v4",
  "ClientId": "YOUR_CLIENT_ID_HERE",
  "AccessToken": "YOUR_ACCESS_TOKEN_HERE"
}
```

## Step 2: Setup MySQL Database

1. Start your MySQL server
2. Create a new database (or the migration will create it):

```sql
CREATE DATABASE IF NOT EXISTS gamedb;
```

3. Update the connection string in `appsettings.json` if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=gamedb;Uid=root;Pwd=password;"
}
```

Replace `password` with your MySQL root password.

## Step 3: Run Database Migrations

Install EF Core CLI (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

Create and apply the initial migration:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## How It Works

1. **Search Flow**:
   - User searches for a game
   - System checks local MySQL database cache first
   - If found, returns cached results immediately
   - If not found, queries IGDB API, caches results, then returns them

2. **Caching**:
   - All games fetched from IGDB are automatically cached in MySQL
   - Subsequent searches for the same game are served from cache (faster)
   - Database stays updated with latest game data from IGDB

## Packages Used

- `Pomelo.EntityFrameworkCore.MySql` - MySQL database provider for EF Core
- Built-in `System.Text.Json` for JSON parsing (no additional package needed)
- Built-in `HttpClient` for HTTPS requests

## Troubleshooting

- **Connection refused**: Ensure MySQL server is running
- **IGDB API 401**: Check that ClientId and AccessToken are correct
- **Database migration fails**: Verify MySQL credentials and database name
