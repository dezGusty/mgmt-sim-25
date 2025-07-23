import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/entities/User';
import { UserViewModel } from '../../../view-models/UserViewModel';
import { UsersService } from '../../../services/users/users';
import { UserFilterPipe } from '../../../pipes/filterPipe/filter-pipe';

@Component({
  selector: 'app-admin-user-relationships',
  imports: [CommonModule, FormsModule, UserFilterPipe],
  templateUrl: './admin-user-relationships.html',
  styleUrl: './admin-user-relationships.css'
})
export class AdminUserRelationships implements OnInit {
  unassignedUsers: any[] = []; // ← Adaugă această proprietate
  userRelationships: any[] = []; // ← Adaugă această proprietate
  searchTerm: string = '';
  selectedRole: string = '';
  selectedDepartment: string = '';
  viewMode:string = 'hierarchy';

  users: UserViewModel[] = [];

  constructor(private userService: UsersService) {

  }

  ngOnInit(): void {
    this.loadUserRelationships();
  }

  setViewMode(mode: 'hierarchy' | 'table'): void {
    this.viewMode = mode;
  }

  loadUserRelationships(): void {
    this.userService.getAllUsersIncludeRelationships().subscribe({
      next: (response) => {
        console.log('API response:', response);
        const rawUsers: User[] = response;

        this.users = rawUsers.map(user => this.mapToUserViewModel(user));
      },
      error: (err) => {
        console.error('Failed to fetch users:', err);
      }
    })
  }

  mapToUserViewModel(user: User): UserViewModel {
    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      jobTitle: user.jobTitleName || 'Unknown',
      jobTitleId: user.jobTitleId || 0,
      department: user.departmentName || 'Unknown',
      departmentId: user.departmentId || 0,
      subordinatesIds: user.subordinatesIds || [],
      subordinatesNames: user.subordinatesNames || [],
      roles : user.roles || [],
      subordinatesJobTitleIds: user.subordinatesJobTitleIds || [],
      subordinatesJobTitleNames: user.subordinatesJobTitles  || [],
    };
  }

  areUnassignedUsers(): boolean {
    return this.unassignedUsers.length > 0;
  }
  
  getSubordinateCount(manager: any): number {
    return manager.subordinatesIds?.length || 0;
  }

  getAdminCount(): number {
    return this.users.filter(user => user.roles?.includes("Admin")).length;
  }

  getManagerCount(): number {
    return this.users.filter(user => user.roles?.includes("Manager")).length;
  }

  assignManager(user: any): void {
    console.log('Assign manager to:', user);
  }

  editRelationship(relationship: any): void {
    console.log('Edit relationship:', relationship);
  }

  viewRelationship(relationship: any): void {
    console.log('View relationship:', relationship);
    // Logica pentru vizualizarea relației
  }

  getTotalRelationships(): number {
    return this.userRelationships.length;
  }

  getUnassignedCount(): number {
    return this.unassignedUsers.length;
  }

  getRoleBadgeClass(role: string): string {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'bg-red-100 text-red-800';
      case 'manager':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }
}
