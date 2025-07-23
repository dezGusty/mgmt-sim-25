import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface User {
  id: string;
  name: string;
  email: string;
  role: 'admin' | 'manager' | 'employee';
  department: string;
  jobTitle: string;
  avatar?: string;
  managerId?: string;
  startDate?: Date;
  status: 'active' | 'inactive';
}

export interface UserRelationship {
  user: User;
  manager?: User;
  directReports: number;
  teamMembers?: User[];
}

export interface ManagerWithTeam extends User {
  teamMembers: User[];
}

@Component({
  selector: 'app-admin-user-relationships',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-user-relationships.html',
  styleUrl: './admin-user-relationships.css'
})
export class AdminUserRelationships implements OnInit {
  viewMode: 'hierarchy' | 'table' = 'hierarchy';
  searchTerm: string = '';
  selectedRole: string = '';
  selectedDepartment: string = '';

  // User data
  admins: User[] = [];
  managersWithTeams: ManagerWithTeam[] = [];
  unassignedUsers: User[] = [];
  userRelationships: UserRelationship[] = [];

  constructor() { }

  ngOnInit(): void {
    this.loadUserRelationships();
  }

  setViewMode(mode: 'hierarchy' | 'table'): void {
    this.viewMode = mode;
  }

  loadUserRelationships(): void {
    // Mock data - replace with actual service calls
    const allUsers: User[] = [
      // Admins
      {
        id: 'admin1',
        name: 'Alice Cooper',
        email: 'alice.cooper@company.com',
        role: 'admin',
        department: 'IT',
        jobTitle: 'System Administrator',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Alice+Cooper&background=ef4444&color=fff'
      },
      {
        id: 'admin2',
        name: 'Bob Wilson',
        email: 'bob.wilson@company.com',
        role: 'admin',
        department: 'IT',
        jobTitle: 'Database Administrator',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Bob+Wilson&background=ef4444&color=fff'
      },

      // Managers
      {
        id: 'mgr1',
        name: 'John Smith',
        email: 'john.smith@company.com',
        role: 'manager',
        department: 'Engineering',
        jobTitle: 'Engineering Manager',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=John+Smith&background=20B486&color=fff'
      },
      {
        id: 'mgr2',
        name: 'Sarah Johnson',
        email: 'sarah.johnson@company.com',
        role: 'manager',
        department: 'Marketing',
        jobTitle: 'Marketing Manager',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Sarah+Johnson&background=20B486&color=fff'
      },
      {
        id: 'mgr3',
        name: 'Michael Brown',
        email: 'michael.brown@company.com',
        role: 'manager',
        department: 'Sales',
        jobTitle: 'Sales Manager',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Michael+Brown&background=20B486&color=fff'
      },

      // Employees
      {
        id: 'emp1',
        name: 'Emma Davis',
        email: 'emma.davis@company.com',
        role: 'employee',
        department: 'Engineering',
        jobTitle: 'Senior Developer',
        managerId: 'mgr1',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Emma+Davis&background=6b7280&color=fff'
      },
      {
        id: 'emp2',
        name: 'James Wilson',
        email: 'james.wilson@company.com',
        role: 'employee',
        department: 'Engineering',
        jobTitle: 'Frontend Developer',
        managerId: 'mgr1',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=James+Wilson&background=6b7280&color=fff'
      },
      {
        id: 'emp3',
        name: 'Lisa Chen',
        email: 'lisa.chen@company.com',
        role: 'employee',
        department: 'Engineering',
        jobTitle: 'Backend Developer',
        managerId: 'mgr1',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Lisa+Chen&background=6b7280&color=fff'
      },
      {
        id: 'emp4',
        name: 'David Martinez',
        email: 'david.martinez@company.com',
        role: 'employee',
        department: 'Marketing',
        jobTitle: 'Content Specialist',
        managerId: 'mgr2',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=David+Martinez&background=6b7280&color=fff'
      },
      {
        id: 'emp5',
        name: 'Jessica Taylor',
        email: 'jessica.taylor@company.com',
        role: 'employee',
        department: 'Marketing',
        jobTitle: 'Digital Marketer',
        managerId: 'mgr2',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Jessica+Taylor&background=6b7280&color=fff'
      },
      {
        id: 'emp6',
        name: 'Robert Anderson',
        email: 'robert.anderson@company.com',
        role: 'employee',
        department: 'Sales',
        jobTitle: 'Sales Representative',
        managerId: 'mgr3',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Robert+Anderson&background=6b7280&color=fff'
      },
      {
        id: 'emp7',
        name: 'Amanda White',
        email: 'amanda.white@company.com',
        role: 'employee',
        department: 'Sales',
        jobTitle: 'Account Executive',
        managerId: 'mgr3',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Amanda+White&background=6b7280&color=fff'
      },

      // Unassigned users
      {
        id: 'unassigned1',
        name: 'Chris Garcia',
        email: 'chris.garcia@company.com',
        role: 'employee',
        department: 'Engineering',
        jobTitle: 'Junior Developer',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Chris+Garcia&background=f97316&color=fff'
      },
      {
        id: 'unassigned2',
        name: 'Nina Rodriguez',
        email: 'nina.rodriguez@company.com',
        role: 'employee',
        department: 'Marketing',
        jobTitle: 'Graphic Designer',
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Nina+Rodriguez&background=f97316&color=fff'
      }
    ];

    // Separate users by role
    this.admins = allUsers.filter(user => user.role === 'admin');
    
    const managers = allUsers.filter(user => user.role === 'manager');
    const employees = allUsers.filter(user => user.role === 'employee');

    // Create managers with their teams
    this.managersWithTeams = managers.map(manager => ({
      ...manager,
      teamMembers: employees.filter(emp => emp.managerId === manager.id)
    }));

    // Find unassigned users (employees without managers)
    this.unassignedUsers = employees.filter(emp => !emp.managerId);

    // Create user relationships for table view
    this.userRelationships = allUsers.map(user => {
      const manager = allUsers.find(u => u.id === user.managerId);
      const directReports = allUsers.filter(u => u.managerId === user.id).length;

      return {
        user,
        manager,
        directReports
      };
    });
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'admin':
        return 'bg-red-100 text-red-800';
      case 'manager':
        return 'bg-green-100 text-green-800';
      case 'employee':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  assignManager(user: User): void {
    console.log('Assign manager to user:', user);
    // Implement manager assignment functionality
    // This could open a modal to select a manager
  }

  editRelationship(relationship: UserRelationship): void {
    console.log('Edit relationship:', relationship);
    // Implement edit relationship functionality
  }

  viewRelationship(relationship: UserRelationship): void {
    console.log('View relationship:', relationship);
    // Implement view relationship functionality
  }

  // Filter methods for table view
  get filteredUserRelationships(): UserRelationship[] {
    return this.userRelationships.filter(relationship => {
      const user = relationship.user;
      
      const matchesSearch = !this.searchTerm || 
        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.jobTitle.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesRole = !this.selectedRole || user.role === this.selectedRole;
      
      const matchesDepartment = !this.selectedDepartment || 
        user.department.toLowerCase() === this.selectedDepartment.toLowerCase();

      return matchesSearch && matchesRole && matchesDepartment;
    });
  }

  // Statistics methods
  getTotalRelationships(): number {
    return this.userRelationships.length;
  }

  getAdminCount(): number {
    return this.admins.length;
  }

  getManagerCount(): number {
    return this.managersWithTeams.length;
  }

  getUnassignedCount(): number {
    return this.unassignedUsers.length;
  }

  // Bulk operations
  reassignTeam(fromManagerId: string, toManagerId: string): void {
    console.log(`Reassigning team from ${fromManagerId} to ${toManagerId}`);
    // Implement bulk team reassignment
  }

  promoteToManager(userId: string): void {
    console.log(`Promoting user ${userId} to manager`);
    // Implement user promotion
  }

  bulkAssignManager(userIds: string[], managerId: string): void {
    console.log(`Bulk assigning users ${userIds} to manager ${managerId}`);
    // Implement bulk manager assignment
  }
}
