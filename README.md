# Football Scouting App

Football Scouting App is an ASP.NET Core MVC web application designed to support football scouting workflows. The system allows scouts to manage player profiles, record match reports, evaluate players by tactical position, and review scouting activity through a clean role-based interface.

## Live Demo

- Demo: [http://scoutingapp76658.azurewebsites.net](http://scoutingapp76658.azurewebsites.net)
- Source code: [https://github.com/alielhelisy/Football-Scouting-App-Web-Programming](https://github.com/alielhelisy/Football-Scouting-App-Web-Programming)

## Main Features

- Secure login and account creation
- Role-based access for admins and scouts
- Protected main admin account
- Player dashboard organized by tactical positions
- Player profile pages with personal, football, and report information
- Add, edit, view, and delete player records
- Create, edit, and delete scouting reports
- Report history for each player
- Reports page with search, position filter, rating filter, and sorting
- Player search by name, club, and position
- Admin account management
- Account page with activity and player summaries
- Change password page with password visibility controls

## User Roles

### Admin

Admins can manage accounts, view all players and reports, and access administrative pages. The main admin account is protected so other admins cannot change or delete it.

### Scout

Scouts can create and manage their own players and reports. They can search players, view player details, and maintain their own scouting activity.

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core
- SQL Server / SQL Server Express for local development
- SQLite support for lightweight hosted deployment
- Razor Views
- Cookie Authentication
- Azure App Service
- HTML, CSS, and JavaScript

## Project Structure

```text
Controllers/   MVC controllers for authentication, players, reports, search, and admin pages
Data/          Entity Framework database context
Helpers/       Shared helper logic such as password hashing
Models/        Application domain models
Views/         Razor views for the user interface
wwwroot/       Static assets
Program.cs     Application startup, database configuration, and routing
```

## Local Setup

### Requirements

- .NET 10 SDK
- SQL Server Express or another SQL Server instance

### Run Locally

1. Clone the repository.

   ```bash
   git clone https://github.com/alielhelisy/Football-Scouting-App-Web-Programming.git
   cd Football-Scouting-App-Web-Programming
   ```

2. Check the database connection string in `appsettings.json`.

   ```json
   "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ScoutingAppWeb;Trusted_Connection=True;TrustServerCertificate=True;"
   ```

3. Restore packages.

   ```bash
   dotnet restore
   ```

4. Run the application.

   ```bash
   dotnet run
   ```

5. Open the local URL shown in the terminal, for example:

   ```text
   http://localhost:5223
   ```

The application checks the database on startup and creates the default admin account if it does not already exist.

## Default Login

```text
Username: admin
Password: admin123
```

For security, the password should be changed after the first login.

## Database

The local version uses SQL Server through Entity Framework Core. The deployed demo is configured to use SQLite on Azure App Service for faster startup during project demonstration.

The application includes startup safeguards for hosted deployment, including retry support and optional SQLite maintenance switches used during deployment.

## Deployment

The demo is deployed on Azure App Service:

```text
http://scoutingapp76658.azurewebsites.net
```

For the course submission, the report should include:

- Project demo link
- Presentation/video explanation link
- GitHub source code link

## Course Context

This project was developed for the Web Programming course as a football scouting management system. It demonstrates server-side MVC development, database integration, authentication, role-based authorization, and a complete CRUD workflow for a real-world scouting scenario.
