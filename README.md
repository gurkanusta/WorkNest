# WorkNest

WorkNest is a **multi-user project & task management backend API**
built with **ASP.NET Core Web API** following **Clean Architecture** principles.

## Features
- JWT Authentication (Register / Login)
- Multi-user project system
- Project membership & authorization
- Task management (CRUD)
- Task status workflow (Todo / InProgress / Done)
- Pagination, filtering & sorting
- FluentValidation for request validation
- Global exception handling
- Serilog logging (console & file)
- Entity Framework Core + SQL Server
- Clean Architecture (Api / Domain / Infrastructure)

##  Architecture
WorkNest
├── WorkNest.Api
│ ├── Controllers
│ ├── Middlewares
│ ├── Validators
│ └── Program.cs
├── WorkNest.Domain
│ └── Entities
└── WorkNest.Infrastructure
├── Identity
├── Persistence
└── Migrations


##  Authentication
- JWT Bearer Token
- Secure project-based authorization


##  Tech Stack
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- FluentValidation
- Serilog
- JWT Authentication

