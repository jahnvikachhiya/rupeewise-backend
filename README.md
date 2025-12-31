# Expense Management API

RESTful API for managing expenses, budgets, notifications, and users. Built with **ASP.NET Core 7**, **JWT Authentication**, and SQL Server backend.

---

## Features

- User management (registration, login, profile, roles)
- JWT authentication with role-based access
- Expense, category, and budget management
- Admin account seeding
- PDF report generation
- Notifications and email (Brevo)
- Health check and Swagger documentation

---

## Requirements

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- SQL Server (Express or full)
- Optional: Postman or Swagger for testing

---

## Setup

1. Clone the repository

git clone <your-repo-url>
cd ExpenseManagementAPI


2. Install dependencies



dotnet restore


3. Create `.env.development` or `.env.production` with the following:


Database

DB_SERVER=YourServerName\SQLEXPRESS
DB_NAME=ExpenseManagementDB

JWT

JWT_SECRET_KEY=YourBase64EncodedSecretKey
JWT_ISSUER=rupeewise-api
JWT_AUDIENCE=rupeewise-client

Admin account

ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@example.com

ADMIN_PASSWORD=admin123

Brevo (email service)

BREVO_API_KEY=YourBrevoApiKey
BREVO_SENDER_EMAIL=sender@example.com

BREVO_SENDER_NAME=YourApp

Application URL

APP_URL=http://localhost:5173

CORS

CORS_ALLOWED_ORIGINS=http://localhost:5173


4. Run the application



dotnet run


Swagger UI will be available at: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

---

## Folder Structure



/Controllers - API endpoints
/Repositories - Data access layer
/Services - Business logic
/Data - Database helper and migrations
/Middleware - Custom middlewares
/DTOs - Data Transfer Objects
/Models - Entity models
/Helpers - Utilities (e.g., PasswordHasher)
/Seeders - Admin seeding


---

## API Endpoints

- `POST /api/users/register` - Register a new user
- `POST /api/users/login` - Authenticate and get JWT
- `GET /api/users/profile/{id}` - Get user profile
- `PUT /api/users/profile/{id}` - Update profile
- `PUT /api/users/change-password/{id}` - Change password
- `GET /health` - Health check

> Full Swagger documentation available at `/swagger`

---

## Notes

- Ensure your JWT secret is **Base64 encoded**.
- Do **not commit** `.env` files containing secrets.
- Use CORS allowed origins properly in frontend integration.