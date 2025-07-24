import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/entities/User';
import { UserViewModel } from '../../../view-models/UserViewModel';
import { UsersService } from '../../../services/users/users';
import { UserFilterPipe } from '../../../pipes/filterPipe/filter-pipe';
import { UnassignedUsersPipe } from '../../../pipes/unassignedUsersPipe/unassigned-users-pipe';

@Component({
  selector: 'app-admin-user-relationships',
  imports: [CommonModule, FormsModule, UserFilterPipe, UnassignedUsersPipe],
  templateUrl: './admin-user-relationships.html',
  styleUrl: './admin-user-relationships.css'
})
export class AdminUserRelationships implements OnInit {
  managersIds: Set<number> = new Set<number>();
  adminsIds: Set<number> = new Set<number>();
  users: UserViewModel[] = [];

  constructor(private userService: UsersService) {

  }

  ngOnInit(): void {
    this.loadUserRelationships();
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
    user.managersIds?.forEach(element => {
      this.managersIds.add(element);
    });

    if(user.roles.includes("Admin"))
    { 
      this.adminsIds.add(user.id);
    }

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
      managersIds: user.managersIds || [],
      subordinatesEmails: user.subordinatesEmails || [],
    };
  }

  isManager(userId: number): boolean {
    return this.managersIds.has(userId);
  }

  isAdmin(user: number): boolean {
    return this.adminsIds.has(user);
  }

  areUnassignedUsers(): boolean {
      return this.users.some(u => u.managersIds && u.managersIds.length > 0);
  }

  getUnassignedUsersCount(): number {
    return this.users.filter(u => u.managersIds && u.managersIds.length === 0).length;
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
