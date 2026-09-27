# Velvion Technologies 🚀

This is the official repository for **Velvion Technologies**. This project is a modern IT company website and management portal, being developed using the Agile methodology.

## 💻 Tech Stack
- **Frontend:** Angular, Tailwind CSS / Bootstrap
- **Backend:** ASP.NET Core Web API (C#)
- **Database:** MySQL (Entity Framework Core)
- **Architecture:** REST API & JWT Authentication

## 🛠️ Prerequisites
To run this project on your local machine, you will need the following software installed:
* [Node.js](https://nodejs.org/) (v18+) and npm
* [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
* [.NET SDK](https://dotnet.microsoft.com/download) (v8.0 or latest)
* [MySQL Server](https://dev.mysql.com/downloads/) & MySQL Workbench
* Visual Studio 2022 (for Backend) and VS Code (for Frontend)

## 🗄️ Database Setup
1. Open MySQL Workbench.
2. Create a new database (schema) named `VelvionDB`.
3. Navigate to the `appsettings.json` file in the Backend project and update your MySQL Connection String (enter your specific User ID and Password).
4. Run the following command in the Package Manager Console (PMC) of Visual Studio to auto-generate the database tables:
   ```bash
   Update-Database