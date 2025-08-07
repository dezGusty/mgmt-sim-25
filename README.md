# mgmt-sim-25
Management Simulator 2025

## Front-end

Technologies used: Angular

### Admin Page

The admin page is dedicated to the users that have the "Admin" user role assigned. 


The main functionality of this page is to perform CRUD operations on the following entities: User, Department, JobTitle, LeaveRequestType, EmployeeManager, EmployeeRoleUser. It's route is 'http://localhost:4200/admin'.

#### 'admin-main-page'

This page contains the 'custom-navbar', 'add-admin-form' and 'admin-*-list' standalone components and can perform the following actions: 
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
All of the deleted entries in the database must be restored, if deleted, by searching in the 'admin-*-list' component the desired row of the table (entity) and pressing the restore button (blue, circular). If the admin tries to add an entry that already exists (it's unique is existent in the DB), an error message is shown that specifies the fact that the database already contains the entry.

#### 'admin-*-list' components

These components split the responsability of fetching paged, filtered (global search + relevant criteria) and sorted items provided by the BE. For each item of the desired list, besides 'admin-user-relationships-list'. All of the GET requests of this page send a 'IFiltered*' const interface to the API and all of the endpoints shall respond with an IApiResponse<*>. The response data is mapped to an I*ViewModel const interface that is used by the view : 

1. 'admin-department-list' displays active/inactive/all departments by sending a request to GET 'https://localhost:7275/departments/queried' endpoint, fetching queried data. IMPORTANT: a department can be deleted only if it is empty (no users are assigned to the desired to be deleted department). Other actions that may be performed are deleted and edit.

2. 'admin-job-titles-list' displays active/inactive/all job titles by sending a request to GET 'https://localhost:7275/jobTitles/queried' endpoint, fetching queried data. IMPORTANT: a job title can be deleted only if it is empty (no users are assigned to the desired to be deleted job title). Other actions that may be performed are deleted and edit.

3. 'admin-leave-request-types-list' displays active job titles by sending a request to GET 'https://localhost:7275/jobTitles/queried' endpoint, fetching queried data. The admin is able to delete existent tpyes.

4. 'admin-user-relationships' component encapsulates two components: 

    * 'admin-user-relationships-list' displays : all the admins, all the managers with their direct subordinates and all the unassigned users. These lists are paged and filtered and for each employee/manager relationships may be created/restored. The employees assigned to a team can be moved from one team to another or become unassigned by clicking the assign/reassign button.
    * 'admin-assign-relationship' exposes all the available managers. For a reassigning process all the already assigned managers are checked in the list, while for assigning it is empty. This uses the post/patch endpoints in the EmployeeManager controller.

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
