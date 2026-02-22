# InsureX

## Overview
InsureX is a modular insurance platform built with .NET 8. It includes a web frontend (MVC), a Web API, and a layered architecture for maintainability and scalability. This project is designed to handle policies, customers, and insurance operations efficiently.

---

## Project Structure

```

dev/srv/
├── InsureX.sln
├── .gitignore
├── README.md
└── src/
├── InsureX.Web/                 # MVC Frontend
│   ├── Program.cs
│   ├── Controllers/
│   └── Views/
├── InsureX.Api/                 # Web API
│   ├── Program.cs
│   └── Controllers/
├── InsureX.Application/         # Business logic / Services
├── InsureX.Domain/              # Entities / Interfaces
│   └── Entities/
└── InsureX.Infrastructure/      # EF Core / Database context
└── Data/

````

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server or SQL Server Express
- Optional: Visual Studio 2022/2023 or VS Code

---

### Build & Run

1. Clone the repository:
```bash
git clone https://github.com/luigi043/dev.git
cd dev
````

2. Restore dependencies:

```bash
dotnet restore
```

3. Build the solution:

```bash
dotnet build
```

4. Run the Web project (MVC frontend):

```bash
dotnet run --project src/InsureX.Web
```

5. Run the API project:

```bash
dotnet run --project src/InsureX.Api
```

6. Open your browser to `https://localhost:5001` to see the web frontend.

---

## Database Setup

The project uses Entity Framework Core for data access.

1. Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=InsureX;Trusted_Connection=True;"
}
```

2. Create the database and initial schema:

```bash
dotnet ef migrations add InitialCreate --project src/InsureX.Infrastructure
dotnet ef database update --project src/InsureX.Infrastructure
```

---

## Sample API Endpoint

`GET /api/sample` returns a sample insurance policy in JSON format:

```json
{
  "id": 1,
  "policyNumber": "POL123456",
  "holderName": "John Doe",
  "startDate": "2026-02-22T00:00:00",
  "endDate": "2027-02-22T00:00:00"
}
```

---

## Notes

* This is a **starter template**. You should migrate actual business code into the `Domain`, `Application`, and `Infrastructure` projects.
* MVC controllers and API controllers are included as examples to verify the setup is working.
* Make sure the database server is running before applying EF migrations.

---

## Contributing

* Clone the repo
* Create a feature branch
* Commit changes
* Push to your fork
* Create a Pull Request

---

## License

This project is under the MIT License.

```

---

This README now:  

- Matches your **current skeleton structure**  
- Explains how to **build, run, and test**  
- Includes **database setup** and a **sample API endpoint**  
- Provides guidance for **future contributions**  

I can also **add badges and a more visual “Getting Started” section** with commands highlighted in GitHub style so it looks professional.  

Do you want me to do that next?
```
