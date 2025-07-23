import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../../services/departments/department';
import { Department } from '../../../models/entities/Department';

@Component({
  selector: 'app-admin-departments-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-departments-list.html',
  styleUrl: './admin-departments-list.css'
})
export class AdminDepartmentsList implements OnInit {
  departments: Department[] = [];
  filteredDepartments: Department[] = [];
  searchTerm: string = '';
  isLoading: boolean = true;
  error: string = '';


  constructor(private departmentService: DepartmentService) { }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.error = '';
    this.departmentService.getAllDepartments().subscribe({
      next: (departments) => {
        this.departments = departments;
        this.filteredDepartments = [...this.departments];
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Failed to load departments. Please try again later.';
        console.error('Error loading departments:', err);
        this.departments = [];
        this.filteredDepartments = [];
      }
    });


  }

  onSearchChange(): void {
    this.filterDepartments();
  }

  private filterDepartments(): void {
    this.filteredDepartments = this.departments.filter(department => {
      const matchesSearch = !this.searchTerm ||
        department.name?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        department.description?.toLowerCase().includes(this.searchTerm.toLowerCase());

      return matchesSearch;
    });
  }

  editDepartment(department: Department): void {
    console.log('Edit department:', department);
  }

  viewDepartment(department: Department): void {
    console.log('View department:', department);
  }

  deleteDepartment(department: Department): void {
    if (confirm(`Are you sure you want to delete ${department.name} department?`)) {
      this.departmentService.deleteDepartment(department.id).subscribe({
        next: () => {
          this.departments = this.departments.filter(d => d.id !== department.id);
          this.filterDepartments();
          console.log('Department deleted:', department);
        },
        error: (error) => {
          console.error('Error deleting department:', error);
          alert('Failed to delete department. Please try again.');
        }
      });
    }
  }

  addDepartment(): void {
    console.log('Add new department');
  }

  getTotalDepartments(): number {
    return this.departments.length;
  }

}