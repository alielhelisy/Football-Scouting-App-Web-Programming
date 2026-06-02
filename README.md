# Football Scouting App

An ASP.NET Core MVC web application for managing football scouting work. The app lets scouts create player profiles, organize players by position, write scouting reports, search players, and manage user accounts with admin/scout roles.

## Features

- User authentication with admin and scout roles
- Main admin protection
- Player dashboard grouped by tactical position
- Add, edit, view, and delete players
- Search players by name, club, and position
- Create, edit, and delete scouting reports
- Report history on each player profile
- Account page with activity and player summaries
- Change password page
- Admin account management

## Tech Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / SQL Server Express
- Razor views
- Cookie authentication

## Requirements

- .NET 10 SDK
- SQL Server Express or another SQL Server instance

## Setup

1. Update the connection string in `appsettings.json` if your SQL Server name is different.

2. Restore packages:

   ```bash
   dotnet restore
   ```

3. Run the app:

   ```bash
   dotnet run
   ```

4. Open the local URL shown in the terminal, for example:

   ```text
   http://localhost:5223
   ```

The app applies migrations on startup and creates the default admin account if it does not already exist.

## Default Admin

```text
Username: admin
Password: admin123
```

Change the password after the first login.

