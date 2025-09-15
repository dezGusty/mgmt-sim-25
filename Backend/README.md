# Backend - Management Simulator 2025

ASP.NET Core backend application implementing Clean Architecture/Onion Architecture for the Management Simulator 2025 project.

## 🛠️ Technologies Used

- **Framework**: ASP.NET Core (.NET 9.0)
- **ORM**: Entity Framework Core 9.0.7
- **Database**: SQLite (Development)
- **Authentication**: Cookie Authentication + JWT Bearer
- **Password Security**: BCrypt.Net-Next 4.0.3
- **Real-time Communication**: SignalR 1.2.0
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit with Moq
- **Caching**: In-Memory Caching

## 🏗️ Project Architecture

The backend follows **Clean Architecture/Onion Architecture** principles with clear separation of concerns:

```
Backend/ManagementSimulator/
├── ManagementSimulator.API/          # 🌐 Presentation Layer
├── ManagementSimulator.Core/         # 💼 Business Logic Layer  
├── ManagementSimulator.Database/     # 🗄️ Data Access Layer
├── ManagementSimulator.Infrastructure/ # ⚙️ Cross-cutting Concerns
└── ManagementSimulator.Tests/        # 🧪 Unit Tests
```

### 🌐 ManagementSimulator.API (Presentation Layer)

**Responsibilities**: HTTP endpoints, request/response handling, authentication, CORS

**Key Components**:
- **Controllers** (11 controllers):
  - `AuthController` - Authentication & authorization
  - `UsersController` - User management
  - `DepartmentsController` - Department CRUD operations
  - `JobTitleController` - Job title management
  - `LeaveRequestController` - Leave request management
  - `LeaveRequestTypeController` - Leave type configuration
  - `EmployeeManagerController` - Manager-employee relationships
  - `SecondManagerController` - Secondary manager assignments
  - `ProjectsController` - Project management
  - `HrController` - HR-specific operations
  - `EmployeeRoleController` - Role management

**Dependencies**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.7" />
<PackageReference Include="Microsoft.AspNetCore.JsonPatch" Version="9.0.7" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.7" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.3" />
```

### 💼 ManagementSimulator.Core (Business Logic Layer)

**Responsibilities**: Business rules, validation, DTOs, service interfaces

**Services** (14 services):
- `UserService` - User management and operations
- `LeaveRequestService` - Leave request processing
- `ProjectService` - Project management
- `EmailService` - Email notifications
- `DepartmentService` - Department operations
- `JobTitleService` - Job title management
- `EmployeeManagerService` - Team management
- `LeaveRequestTypeService` - Leave type configuration
- `AuthService` - Authentication logic
- `AuthorizationService` - Authorization checks
- `AvailabilityService` - Employee availability
- `SecondManagerService` - Secondary manager logic
- `EmployeeRoleService` - Role management
- `PublicHolidayService` - Holiday management

**Key Features**:
- **DTOs**: Structured request/response objects
- **Mapping Extensions**: Entity-to-DTO transformations
- **Business Validation**: Complex business rules
- **Service Interfaces**: Abstraction for testability

**Dependencies**:
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" Version="2.3.0" />
```

### 🗄️ ManagementSimulator.Database (Data Access Layer)

**Responsibilities**: Database context, entities, repositories, migrations

**Key Components**:
- **Entities** (13+ entities):
  - `User`, `Department`, `JobTitle`
  - `LeaveRequest`, `LeaveRequestType`
  - `Project`, `EmployeeManager`
  - `EmployeeRole`, `PublicHoliday`
  - `SecondManager`, `BaseEntity`

- **Repositories** (11 repositories):
  - `UserRepository` - User data operations
  - `DepartmentRepository` - Department data access
  - `JobTitleRepository` - Job title operations
  - `LeaveRequestRepository` - Leave request data
  - `ProjectRepository` - Project data management
  - `EmployeeManagerRepository` - Team relationships
  - `LeaveRequestTypeRepository` - Leave type data
  - `EmployeeRoleRepository` - Role data access
  - `SecondManagerRepository` - Secondary manager data
  - `PublicHolidayRepository` - Holiday data
  - `BaseRepository` - Generic repository operations

- **Database Context**: `MGMTSimulatorDbContext` with SQLite configuration
- **Migrations**: EF Core migrations for schema management
- **Seeds**: `SetupSeed` and `PopulateSeed` for initial data

**Dependencies**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.7" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.2" />
```

### ⚙️ ManagementSimulator.Infrastructure (Cross-cutting Concerns)

**Responsibilities**: Configuration, middleware, exceptions, utilities

**Key Components**:
- **Configuration**: `AppConfig`, `ConnectionStrings`
- **Middleware**: `ExceptionHandlingMiddleware`
- **Custom Exceptions**: Domain-specific exceptions
- **Utilities**: Cross-cutting helper functions

**Dependencies**:
```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.2.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.7" />
```

### 🧪 ManagementSimulator.Tests (Testing Layer)

**Responsibilities**: Unit tests, integration tests, test utilities

**Test Coverage**:
- **Repository Tests**: Data access layer testing
- **Service Tests**: Business logic testing
- **Integration Tests**: End-to-end scenarios

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQLite** (included with .NET)
- **Visual Studio 2022** or **VS Code** (recommended)

### 1. Clone and Setup

```bash
# Clone the repository
git clone https://github.com/dezGusty/mgmt-sim-25
cd Backend/ManagementSimulator

# Restore NuGet packages
dotnet restore
```

### 2. Database Setup

```bash
# Create/Update database with migrations
dotnet ef database update --project ManagementSimulator.Database --startup-project ManagementSimulator.API

# Optional: Seed database with test data
# Uncomment seed lines in Program.cs and run:
dotnet run --project ManagementSimulator.API
```

### 3. Run the Application

```bash
# Development mode
dotnet run --project ManagementSimulator.API

# Or with hot reload
dotnet watch run --project ManagementSimulator.API
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7275`
- **Swagger UI**: `https://localhost:7275/swagger`

### 4. Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test ManagementSimulator.Tests
```

## 📡 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/reset-password` - Password reset

### User Management
- `GET /api/users` - Get all users (paginated)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Soft delete user
- `POST /api/users/{id}/restore` - Restore deleted user

### Department Management
- `GET /api/departments` - Get all departments
- `POST /api/departments` - Create department
- `PUT /api/departments/{id}` - Update department
- `DELETE /api/departments/{id}` - Delete department

### Leave Request Management
- `GET /api/leaverequests` - Get leave requests (filtered)
- `POST /api/leaverequests` - Create leave request
- `PUT /api/leaverequests/{id}/review` - Approve/Reject request
- `DELETE /api/leaverequests/{id}` - Cancel request

### Project Management
- `GET /api/projects` - Get all projects
- `POST /api/projects` - Create project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### HR Operations
- `GET /api/hr/calendar` - Get HR calendar
- `GET /api/hr/availability` - Check employee availability
- `POST /api/hr/holidays` - Manage public holidays

*For complete API documentation, visit `/swagger` when running the application.*

## 🔐 Authentication & Authorization

### Cookie Authentication
- **Primary**: Cookie-based authentication for web sessions
- **Session Duration**: 12 hours with sliding expiration
- **Security**: HttpOnly, Secure, SameSite=None for CORS

### JWT Bearer (Optional)
- **Secondary**: JWT tokens for API access
- **Claims**: User ID, roles, permissions
- **Expiration**: Configurable token lifetime

### Role-Based Authorization
- **Admin**: Full system access
- **Manager**: Team and project management
- **HR**: Leave and calendar management  
- **User**: Personal data access only

### Security Features
- **Password Hashing**: BCrypt with salt
- **Rate Limiting**: Login attempt protection
- **CORS**: Configured for Angular frontend
- **Exception Handling**: Centralized error management

## 🗄️ Database Schema

### Core Entities
- **User**: Employee information, credentials, roles
- **Department**: Organizational units
- **JobTitle**: Position definitions
- **EmployeeManager**: Manager-employee relationships
- **LeaveRequest**: Time-off requests with approval workflow
- **Project**: Work assignments and tracking
- **PublicHoliday**: Company-wide holidays

### Entity Relationships
- User ↔ Department (Many-to-One)
- User ↔ JobTitle (Many-to-One)
- User ↔ EmployeeManager (Many-to-Many)
- User ↔ LeaveRequest (One-to-Many)
- User ↔ Project (Many-to-Many)

### Database Features
- **Soft Deletes**: Entities marked as deleted, not removed
- **Audit Trail**: CreatedAt, ModifiedAt, DeletedAt timestamps
- **Unique Constraints**: Email, department names, etc.
- **Indexes**: Optimized queries for common operations

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "MGMTSimulatorDb": "Data Source=MGMTSimulator.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `ASPNETCORE_URLS` - Binding URLs
- `ConnectionStrings__MGMTSimulatorDb` - Database connection

## 🧪 Testing Strategy

### Unit Tests
- **Service Layer**: Business logic validation
- **Repository Layer**: Data access verification
- **Controller Layer**: API endpoint testing

### Test Coverage Areas
- User management operations
- Leave request workflows
- Authentication scenarios
- Authorization checks
- Data validation rules

### Test Tools
- **xUnit**: Test framework
- **Moq**: Mocking framework
- **EF Core InMemory**: Database testing
- **FluentAssertions**: Assertion library

## 📊 Performance Features

### Caching
- **Memory Cache**: Frequently accessed data
- **Query Optimization**: EF Core query tuning
- **Pagination**: Large dataset handling

### Database Optimization
- **Indexes**: Strategic index placement
- **Lazy Loading**: On-demand data loading
- **Connection Pooling**: Efficient connection management

## 🔍 Monitoring & Logging

### Logging
- **Structured Logging**: JSON-formatted logs
- **Log Levels**: Information, Warning, Error
- **Request Logging**: HTTP request/response tracking

### Error Handling
- **Global Exception Handling**: Centralized error processing
- **Custom Exceptions**: Domain-specific error types
- **Error Responses**: Consistent error format

## 🚀 Deployment

### Development
```bash
dotnet run --project ManagementSimulator.API
```

### Production
```bash
dotnet publish -c Release -o ./publish
dotnet ManagementSimulator.API.dll
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY publish/ .
ENTRYPOINT ["dotnet", "ManagementSimulator.API.dll"]
```

## 🤝 Contributing

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation for public APIs
- Maintain test coverage above 80%

### Pull Request Process
1. Create feature branch from `main`
2. Implement changes with tests
3. Update documentation
4. Submit pull request
5. Code review and merge

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [xUnit Testing](https://xunit.net/docs/getting-started/netcore/cmdline) 