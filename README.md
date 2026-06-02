# Football Scouting App

An ASP.NET Core MVC web application for managing football scouting work. The app lets scouts create player profiles, organize players by position, write scouting reports, search players, and manage user accounts with admin/scout roles.

## Demo

Live project demo:

```text
http://scoutingapp76658.azurewebsites.net
```

GitHub source code:

```text
https://github.com/alielhelisy/Football-Scouting-App-Web-Programming
```

## Features

- User authentication with admin and scout roles
- Main admin protection
- Player dashboard grouped by tactical position
- Add, edit, view, and delete players
- Admin view of all players and reports
- Search players by name, club, and position
- Create, edit, and delete scouting reports
- Reports page with search, position filter, rating filter, and sorting
- Report history on each player profile
- Account page with activity and player summaries
- Change password page
- Admin account management

## Tech Stack

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / SQL Server Express
- Azure App Service
- Azure SQL Database
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

## Deployment

The deployed demo is hosted on Azure App Service and uses Azure SQL Database for the online database.

For course submission, include the live demo link, the GitHub source code link, and a separate presentation/video link in the project report.
