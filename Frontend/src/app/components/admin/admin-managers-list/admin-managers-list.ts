import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Manager {
  id: string;
  name: string;
  email: string;
  jobTitle: string;
  department: string;
  teamSize: number;
  status: 'active' | 'inactive';
  avatar?: string;
  phoneNumber?: string;
  startDate?: Date;
}

@Component({
  selector: 'app-admin-managers-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-managers-list.html',
  styleUrl: './admin-managers-list.css'
})
export class AdminManagersList implements OnInit {
  managers: Manager[] = [];
  filteredManagers: Manager[] = [];
  searchTerm: string = '';
  selectedDepartment: string = '';

  // Pagination
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;

  constructor() { }

  ngOnInit(): void {
    this.loadManagers();
  }

  loadManagers(): void {
    // Mock data - replace with actual service call
    this.managers = [
      {
        id: '1',
        name: 'John Smith',
        email: 'john.smith@company.com',
        jobTitle: 'Engineering Manager',
        department: 'Engineering',
        teamSize: 12,
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=John+Smith&background=20B486&color=fff'
      },
      {
        id: '2',
        name: 'Sarah Johnson',
        email: 'sarah.johnson@company.com',
        jobTitle: 'Marketing Manager',
        department: 'Marketing',
        teamSize: 8,
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Sarah+Johnson&background=20B486&color=fff'
      },
      {
        id: '3',
        name: 'Michael Brown',
        email: 'michael.brown@company.com',
        jobTitle: 'Sales Manager',
        department: 'Sales',
        teamSize: 15,
        status: 'active',
        avatar: 'https://ui-avatars.com/api/?name=Michael+Brown&background=20B486&color=fff'
      },
      {
        id: '4',
        name: 'Emily Davis',
        email: 'emily.davis@company.com',
        jobTitle: 'HR Manager',
        department: 'Human Resources',
        teamSize: 5,
        status: 'inactive',
        avatar: 'https://ui-avatars.com/api/?name=Emily+Davis&background=20B486&color=fff'
      }
    ];

    this.filteredManagers = [...this.managers];
    this.totalItems = this.managers.length;
  }

  onSearchChange(): void {
    this.filterManagers();
  }

  onDepartmentChange(): void {
    this.filterManagers();
  }

  private filterManagers(): void {
    this.filteredManagers = this.managers.filter(manager => {
      const matchesSearch = !this.searchTerm || 
        manager.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        manager.email.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        manager.jobTitle.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesDepartment = !this.selectedDepartment || 
        manager.department.toLowerCase() === this.selectedDepartment.toLowerCase();

      return matchesSearch && matchesDepartment;
    });

    this.totalItems = this.filteredManagers.length;
    this.currentPage = 1; // Reset to first page
  }

  editManager(manager: Manager): void {
    console.log('Edit manager:', manager);
    // Implement edit functionality
  }

  viewManager(manager: Manager): void {
    console.log('View manager:', manager);
    // Implement view functionality
  }

  deleteManager(manager: Manager): void {
    if (confirm(`Are you sure you want to delete ${manager.name}?`)) {
      this.managers = this.managers.filter(m => m.id !== manager.id);
      this.filterManagers();
      console.log('Manager deleted:', manager);
    }
  }

  // Pagination methods
  get paginatedManagers(): Manager[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredManagers.slice(startIndex, endIndex);
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
