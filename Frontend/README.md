# Frontend - Management Simulator 2025

Management Simulator frontend application developed with Angular 20.1.0.

## 🛠️ Technologies Used

- **Framework**: Angular 20.1.0
- **Styling**: TailwindCSS 4.1.11 
- **Real-time Communication**: Microsoft SignalR 8.0.7
- **CSV File Processing**: PapaParse 5.5.3
- **Language**: TypeScript 5.8.2

## 🚀 Development Commands

### Starting Development Server

To start the local development server:

```bash
npm start
# or
ng serve
```

The application will be available at `http://localhost:4200/`. The application will automatically reload when you modify source files.

### Installing Dependencies

```bash
npm install
```

### Production Build

To compile the project for production:

```bash
ng build
```

The compiled files will be stored in the `dist/` directory. The production build optimizes the application for performance and speed.

## 📁 Project Structure

```
src/app/
├── components/           # UI components organized by roles
│   ├── admin/           # Administration panel
│   │   ├── admin-add-form/          # Forms for adding entities
│   │   ├── admin-departments-list/   # Departments list
│   │   ├── admin-job-titles-list/    # Job titles list
│   │   ├── admin-users-list/         # Users list
│   │   └── admin-user-relationships/ # Manager-employee relationship management
│   ├── manager/         # Manager panel
│   │   ├── leave-management-view/    # Leave request management
│   │   ├── project-management-view/  # Project management
│   │   └── project-details/          # Project details
│   ├── hr/              # HR panel
│   │   ├── calendar/                 # Calendar with leaves
│   │   └── leave-days-overview/      # Leave overview
│   ├── user/            # User panel
│   ├── shared/          # Reusable components
│   ├── login/           # Login page
│   ├── forgot-password/ # Password reset
│   └── reset-password/  # Password reset confirmation
├── services/            # Angular services for API
│   ├── authService/     # Authentication service
│   ├── departments/     # Departments service
│   ├── users/           # Users service
│   ├── leave-requests/  # Leave requests service
│   └── projects/        # Projects service
├── models/              # TypeScript interfaces
│   ├── entities/        # Entity models
│   ├── requests/        # API request models
│   └── responses/       # API response models
├── guards/              # Route guards for authentication
├── interceptors/        # HTTP interceptors
├── pipes/               # Custom pipes
└── utils/               # Utility functions
```

## 🔐 Authentication System

### Authentication Components

#### Login Component (`/login`)
- Authentication with email and password
- Frontend and backend validation
- Automatic redirection based on user roles
- "Remember Me" functionality
- Integration with password reset flow

#### Reset Password Component (`/reset-password`)
- Password reset request via email
- 6-character verification code
- Strong password policy validation
- Code expiration after 15 minutes
- Rate limiting for security

### Authentication Flows

1. **Standard Login**:
   - User → Email/Password → Validation → JWT Token → Role-based redirection

2. **Password Reset**:
   - Email → Verification code → New password → Confirmation → Login

### Roles and Redirections

- **Admin** → `/admin` - Full access to all functionalities
- **Manager** → `/manager` - Team and project management  
- **HR** → `/hr` - Leave management and calendar
- **User** → `/user` - View profile and own requests
- **Multiple roles** → `/role-selector` - Active role selection

## 🎨 Styling and UI

### TailwindCSS
The project uses TailwindCSS 4.1.11 for styling:
- Configuration in `tailwind.config.js`
- Utility classes for responsive design
- Custom components for UI consistency

### Shared Components
- Custom navbar with role-based navigation
- Reusable forms with validation
- Tables with pagination, sorting, and filtering
- Modals for confirmations and details

## 📡 API Integration

### HTTP Services
All services extend a BaseService and use:
- Interceptors for automatic authentication
- Centralized error handling
- Type-safe API calls with TypeScript interfaces

### SignalR Integration
- Real-time notifications for:
  - Leave request approvals/rejections
  - Project updates
  - Administrative notifications

## 🔍 Advanced Features

### Filtering and Search
- Global search in all lists
- Multi-criteria filtering
- Column sorting
- Server-side pagination

### Export/Import
- Data export to CSV format
- User import from CSV
- Data validation on import

### Responsive Design
- Optimized for desktop, tablet, and mobile
- Adaptive navigation on small screens
- Touch-friendly on mobile devices

## 🛠️ Development and Debugging

### Code Scaffolding

To generate new components:

```bash
# New component
ng generate component component-name

# New service
ng generate service service-name

# New guard
ng generate guard guard-name

# New pipe
ng generate pipe pipe-name
```

For complete list of available schematics:

```bash
ng generate --help
```

### Environment Configuration

Configure API endpoints in:
- `src/environments/environment.ts` (development)
- `src/environments/environment.prod.ts` (production)

### Debugging
- Use Chrome DevTools for debugging
- Angular DevTools extension for component inspection
- Console logs for data flow tracking

## 📚 Additional Resources

- [Angular Documentation](https://angular.dev/)
- [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli)
- [TailwindCSS Documentation](https://tailwindcss.com/docs)
- [SignalR for JavaScript](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)

## 🐛 Troubleshooting

### Common Issues

1. **Port occupied**: Change port with `ng serve --port 4201`
2. **Corrupted node modules**: Delete `node_modules` and run `npm install`
3. **TypeScript errors**: Check versions in `package.json`
4. **API connection failed**: Verify backend is running on correct port
