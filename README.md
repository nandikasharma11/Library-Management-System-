# Library Management System (LMSystem)

LMSystem is a full-stack, enterprise-grade Library Management System built using ASP.NET Core MVC, Entity Framework Core, SQL Server, and Bootstrap 5.

## Technology Stack
- **Backend Framework:** ASP.NET Core 8.0 MVC
- **ORM / Data Access:** Entity Framework Core Code-First, ADO.NET (`Microsoft.Data.SqlClient`)
- **Database Engine:** SQL Server / LocalDB
- **Frontend Framework:** Bootstrap 5, jQuery, Bootstrap Icons
- **Authentication:** ASP.NET Core Identity
- **Unit Testing:** xUnit with FluentAssertions and EF Core In-Memory

## Features
- **Books CRUD:** Comprehensive catalog management for books with availability tracking and input checks.
- **Book Borrowing & Returning:** Atomic transactions mapping checkout and check-in workflows.
- **Identity & Access Control:** Seeding routines for core roles (`Administrator`, `Librarian`, `Member`) and default credentials.
- **Dashboards:** Metrics panels pulling direct counts of students, books, librarians, and publications.
- **Students & Librarians CRUD:** Enterprise raw SQL / ADO.NET CRUD controllers and parameterized commands.
- **Publications Module:** Newspapers and Magazines management utilizing single-table inheritance discriminator.
- **Static Pages:** About and Contact information hubs.
- **Tests:** Full in-memory database test suite verifying filtering, search queries, and page layouts.

---

## Setup and Installation

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Docker/Remote instance)

### Installation Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/nandikasharma11/Library-Management-System-.git
   cd Library-Management-System-
   ```

2. **Configure Database Connection:**
   Update the connection string in `LMSystem/appsettings.json` to point to your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=LMS;Integrated Security=True;Encrypt=False"
   }
   ```

3. **Generate and Apply EF Core Migrations:**
   ```bash
   cd LMSystem
   dotnet ef database update
   ```

4. **Seed Default Users and Roles:**
   The application will automatically attempt to run migrations and seed default credentials on startup:
   - **Administrator:** `admin@example.com` (Password: `Password123`)
   - **Librarian:** `librarian@example.com` (Password: `Password123`)
   - **Member:** `member@example.com` (Password: `Password123`)

---

## Running the Application

To run the web app locally:
```bash
cd LMSystem
dotnet run
```
The application will start hosting at `http://localhost:5200` (or similar configured port).

---

## Running the Test Suite

To run all unit tests in the solution:
```bash
dotnet test
```