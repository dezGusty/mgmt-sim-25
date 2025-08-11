# mgmt-sim-25

Management Simulator 2025

## Front-end

Technologies used: Angular

### Login & Authentication

The authentication system handles user login, password reset functionality, and session management for the Management Simulator application.

#### 'login' component

The login page allows users to authenticate and access the application. It's route is 'http://localhost:4200/login'.

**Main functionality:**

- User authentication with email and password
- Form validation (both FE and BE)
- Automatic redirection based on user roles
- Remember me functionality
- Integration with password reset flow

**API Integration:**

- POST 'https://localhost:7275/auth/login' - Authenticates user credentials and returns JWT token
- The component sends an `ILoginRequest` interface containing:
  ```typescript
  {
    email: string;
    password: string;
  }
  ```

**Features:**

1. **Email Validation** - Validates email format and required field
2. **Password Validation** - Ensures password is provided and meets basic requirements
3. **Error Handling** - Displays appropriate error messages for:
   - Invalid credentials
   - Account locked/disabled
   - Server connectivity issues
   - Validation errors

**User Flow:**

1. User enters email and password
2. Frontend validates input fields
3. Sends authentication request to backend
4. On success:
   - JWT token is stored (localStorage or sessionStorage based on 'Remember Me')
   - User is redirected to appropriate dashboard based on role:
     - Admin users → `/admin`
     - Manager users → `/manager`
     - Regular users → `/user`
     - More than one role → `/role-selector`
5. On failure:
   - Error message is displayed
   - Form remains accessible for retry

#### 'reset-password' component

The password reset system provides a secure way for users to regain access to their accounts. It consists of two main flows:

##### **Request Password Reset Flow**

**Route:** 'http://localhost:4200/reset-password'

**Features:**

1. **Email Input & Validation** - User provides registered email address
2. **Code Generation & Delivery** - System generates 6-character alphanumeric code and sends via email
3. **Rate Limiting** - Prevents abuse by limiting reset requests per email
4. **Email Template** - Professional email with:
   - Verification code
   - Expiration time (15 minutes)
   - Security notice
   - Instructions for next steps

**User Flow:**

1. User enters registered email address
2. System validates email format and existence
3. Verification code is generated and sent to email
4. User receives email with 6-digit code
5. System displays code input form
6. User proceeds to password reset confirmation

##### **Password Reset Confirmation Flow**

**Features:**

1. **Code Verification** - Validates 6-character verification code
2. **Password Requirements** - Enforces strong password policies:
   - Minimum 8 characters
   - At least one uppercase letter
   - At least one lowercase letter
   - At least one number
   - At least one special character
3. **Password Confirmation** - Ensures both password fields match
4. **Code Expiration** - 15-minute timeout for security
5. **One-time Use** - Codes are invalidated after successful use

**User Flow:**

1. User enters verification code from email
2. User creates new password meeting requirements
3. User confirms password by re-entering
4. System validates code and password strength
5. On success:
   - Password is updated in database
   - User is redirected to login page
   - Success confirmation is displayed
6. On failure:
   - Appropriate error message is shown
   - User can request new code if expired

#### Security Features

**JWT Token Management:**

- Secure token storage with configurable persistence
- Automatic token refresh mechanism
- Role-based access control integration
- Secure logout with token invalidation

**Password Security:**

- BCrypt hashing for password storage
- Salt-based encryption
- Force password change on first login
- Password strength validation

**Reset Security:**

- Time-limited verification codes (15 minutes)
- Single-use verification codes
- Rate limiting on reset requests
- Secure code generation (cryptographically random)
- Email verification before password change

#### Error Handling & Validation

**Frontend Validations:**

- Real-time email format validation
- Password strength indicator
- Required field validation
- Form state management

**Backend Validations:**

- Email existence verification
- Account status checking
- Password policy enforcement
- Security breach detection

**Error Messages:**

- User-friendly error descriptions
- Security-conscious messaging (no information leakage)
- Consistent error formatting

#### Integration with Other Components

**Navigation:**

- Seamless integration with routing system
- Automatic redirection based on authentication state
- Protected route handling

**State Management:**

- User session persistence
- Authentication state across components
- Role-based UI adaptations

**Notifications:**

- Success/error toast notifications
- Email delivery confirmations
- Security alerts for suspicious activity

#### IMPORTANT Security Notes:

1. **Code Expiration:** Verification codes expire after 15 minutes for security
2. **Rate Limiting:** Only one reset request per email every 5 minutes
3. **Audit Trail:** All authentication attempts are logged for security monitoring
4. **Token Security:** JWT tokens include user roles and expiration claims
5. **Password Policy:** New passwords must meet minimum security requirements
6. **Email Verification:** Reset codes are only sent to registered email addresses

#### Email Templates

**Welcome Email (New User):**

- Instructions for first login
- Password change requirement notice

**Password Reset Email:**

- 6-digit verification code
- Clear expiration time
- Security best practices reminder
- Contact information for support

Both email templates are professionally designed and include company branding for consistency with the overall application experience.

### Admin Page

The admin page is dedicated to the users that have the "Admin" user role assigned.

The main functionality of this page is to perform CRUD operations on the following entities: User, Department, JobTitle, LeaveRequestType, EmployeeManager, EmployeeRoleUser. It's route is 'http://localhost:4200/admin'.

#### 'admin-main-page'

This page contains the 'custom-navbar', 'add-admin-form' and 'admin-\*-list' standalone components and can perform the following actions:

- Logout
- Open/hide 'add-admin-form' component by clicking the 'Add new item' button
- Choose the pertinent list of displayed items by clicking one of the existing options of the navigation tab

#### 'admin-add-form' component

The 'admin-add-form' component has the responsability of creating the previously specified entities and display the following 4 form sub-components :

1. 'add-department' - POST 'https://localhost:7275/departments' in BE (departments must have a unique name)

2. 'add-job-title' - POST 'https://localhost:7275/jobTitles' in BE (job titles must have a unique title)

3. 'add-leave-request-type' - POST 'https://localhost:7275/leaveRequestType' in BE (leave types must have a unique title). Also, a good to know is the fact that the leave type is unique by their type and isPaid status combined (e.g. study leave paid != study leave unpaid)

4. 'add-user' - POST 'https://localhost:7275/users' in BE, also uses GET 'https://localhost:7275/departments/queried' and GET 'https://localhost:7275/jobTitles/queried' in order to fetch the necessary FK required to be able to create a new user, implemented using lazy loading. An user must have a unique email address. After succesfully adding a user in the database an email is sent to it's address, containing a confirmation message and 3 indication steps, helpful in the 'reset password' process.

All of the mentioned forms contain error messages (FE validations) but also BE validations in case of duplicate unique columns in the database or FK not found message.

### IMPORTANT:

All of the deleted entries in the database must be restored, if deleted, by searching in the 'admin-\*-list' component the desired row of the table (entity) and pressing the restore button (blue, circular). If the admin tries to add an entry that already exists (it's unique is existent in the DB), an error message is shown that specifies the fact that the database already contains the entry.

#### 'admin-\*-list' components

These components split the responsability of fetching paged, filtered (global search + relevant criteria) and sorted items provided by the BE. For each item of the desired list, besides 'admin-user-relationships-list'. All of the GET requests of this page send a 'IFiltered*' const interface to the API and all of the endpoints shall respond with an IApiResponse<*>. The response data is mapped to an I\*ViewModel const interface that is used by the view :

1. 'admin-department-list' displays active/inactive/all departments by sending a request to GET 'https://localhost:7275/departments/queried' endpoint, fetching queried data. IMPORTANT: a department can be deleted only if it is empty (no users are assigned to the desired to be deleted department). Other actions that may be performed are deleted and edit.

2. 'admin-job-titles-list' displays active/inactive/all job titles by sending a request to GET 'https://localhost:7275/jobTitles/queried' endpoint, fetching queried data. IMPORTANT: a job title can be deleted only if it is empty (no users are assigned to the desired to be deleted job title). Other actions that may be performed are deleted and edit.

3. 'admin-leave-request-types-list' displays active job titles by sending a request to GET 'https://localhost:7275/jobTitles/queried' endpoint, fetching queried data. The admin is able to delete existent tpyes.

4. 'admin-user-relationships' component encapsulates two components:

   - 'admin-user-relationships-list' displays : all the admins, all the managers with their direct subordinates and all the unassigned users. These lists are paged and filtered and for each employee/manager relationships may be created/restored. The employees assigned to a team can be moved from one team to another or become unassigned by clicking the assign/reassign button.
   - 'admin-assign-relationship' exposes all the available managers. For a reassigning process all the already assigned managers are checked in the list, while for assigning it is empty. This uses the post/patch endpoints in the EmployeeManager controller.

5. 'admin-users-list' displays active/inactive/all users by sending a request to GET 'https://localhost:7275/users/queried' endpoint, fetching queried data. Other actions that may be performed are restore, deleted and edit.

## Back-end

Technologies used: ASP.NET Core, EF core, SQL server

The ASP.NET project is structured in a web-API application (ManagementSimulator.API), and 3 class libraries : ManagementSimulator.Core(services), ManagementSimulator.Database(repositories), ManagementSimulator.Infrastructure(app config, middleware), following the onion/repositories design pattern.

### ManagementSimulator.API

### ManagementSimulator.Core

### ManagementSimulator.Database

### ManagementSimulator.Infrastructure

### Database

Schema: (https://www.mermaidchart.com/app/projects/5ee93bd9-c7b6-44b3-88b1-9ce276d93eb4/diagrams/dafd62f6-2389-4164-a13e-1d9c5c653559/share/invite/eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkb2N1bWVudElEIjoiZGFmZDYyZjYtMjM4OS00MTY0LWExM2UtMWQ5YzVjNjUzNTU5IiwiYWNjZXNzIjoiVmlldyIsImlhdCI6MTc1NDU3NzY5OX0.t2MmExZOX017MDUMEvpZC1U6Jrw1cL9LCUPN7a7n6Qg)

![Screenshot](./Screenshots/dbSchema.png)
