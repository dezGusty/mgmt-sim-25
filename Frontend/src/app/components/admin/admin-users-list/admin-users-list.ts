import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../../models/entities/User';
import { UsersService } from '../../../services/users/users';
import { UserViewModel } from '../../../view-models/UserViewModel';

@Component({
  selector: 'app-admin-users-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users-list.html',
  styleUrl: './admin-users-list.css'
})
export class AdminUsersList implements OnInit {
  users: UserViewModel[] = [];
  filteredUsers: UserViewModel[] = [];
  searchTerm: string = '';
  selectedDepartment: string = '';

  // Pagination
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;

  constructor(private usersService: UsersService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
  this.usersService.getAllUsers().subscribe({
      next: (response) => {
        console.log('API response:', response);   

        const rawUsers: User[] = response;

        this.users = rawUsers.map(user => this.mapToUserViewModel(user));
        this.filteredUsers = [...this.users];
        this.totalItems = this.users.length;
      },
      error: (err) => {
        console.error('Failed to fetch users:', err);
      }
    });
  }


  private mapToUserViewModel(user: User): UserViewModel {
    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      jobTitle: user.jobTitleName || 'Unknown',
      department: user.departmentName || 'Unknown',          
      status: 'active',
      avatar: `https://ui-avatars.com/api/?name=${user.firstName}+${user.lastName}&background=20B486&color=fff`
    };
  }

  onSearchChange(): void {
    this.filterUsers();
  }

  onDepartmentChange(): void {
    this.filterUsers();
  }

  private filterUsers(): void {
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = !this.searchTerm || 
        user.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        user.jobTitle.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesDepartment = !this.selectedDepartment || 
        user.department.toLowerCase() === this.selectedDepartment.toLowerCase();

      return matchesSearch && matchesDepartment;
    });

    this.totalItems = this.filteredUsers.length;
    this.currentPage = 1; // Reset to first page
  }

  editUser(user: UserViewModel): void {
    console.log('Edit user:', user);
    // Implement edit functionality
  }

  viewUser(user: UserViewModel): void {
    console.log('View user:', user);
    // Implement view functionality
  }

  deleteUser(user: UserViewModel): void {
    if (confirm(`Are you sure you want to delete ${user.name}?`)) {
      this.users = this.users.filter(u => u.id !== user.id);
      this.filterUsers();
      console.log('User deleted:', user);
    }
  }

  // Pagination methods
  get paginatedUsers(): UserViewModel[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredUsers.slice(startIndex, endIndex);
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

}
