import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface JobTitle {
  id: string;
  title: string;
  code: string;
  department: string;
  level: 'entry' | 'mid' | 'senior' | 'executive';
  employeeCount: number;
  salaryMin: number;
  salaryMax: number;
  status: 'active' | 'inactive';
  color: string;
  description?: string;
  requirements?: string[];
  responsibilities?: string[];
  createdDate?: Date;
  lastUpdated?: Date;
}

@Component({
  selector: 'app-admin-job-titles-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-job-titles-list.html',
  styleUrl: './admin-job-titles-list.css'
})
export class AdminJobTitlesList implements OnInit {
  jobTitles: JobTitle[] = [];
  filteredJobTitles: JobTitle[] = [];
  searchTerm: string = '';
  selectedDepartment: string = '';
  selectedLevel: string = '';

  constructor() { }

  ngOnInit(): void {
    this.loadJobTitles();
  }

  loadJobTitles(): void {
    // Mock data - replace with actual service call
    this.jobTitles = [
      {
        id: '1',
        title: 'Senior Software Engineer',
        code: 'SSE-001',
        department: 'Engineering',
        level: 'senior',
        employeeCount: 12,
        salaryMin: 80000,
        salaryMax: 120000,
        status: 'active',
        color: '#3B82F6',
        description: 'Experienced software engineer responsible for complex system design and development.'
      },
      {
        id: '2',
        title: 'Marketing Specialist',
        code: 'MS-001',
        department: 'Marketing',
        level: 'mid',
        employeeCount: 8,
        salaryMin: 45000,
        salaryMax: 65000,
        status: 'active',
        color: '#10B981'
      },
      {
        id: '3',
        title: 'Sales Representative',
        code: 'SR-001',
        department: 'Sales',
        level: 'entry',
        employeeCount: 15,
        salaryMin: 35000,
        salaryMax: 50000,
        status: 'active',
        color: '#F59E0B'
      },
      {
        id: '4',
        title: 'HR Business Partner',
        code: 'HRBP-001',
        department: 'Human Resources',
        level: 'senior',
        employeeCount: 3,
        salaryMin: 70000,
        salaryMax: 90000,
        status: 'active',
        color: '#8B5CF6'
      },
      {
        id: '5',
        title: 'Product Manager',
        code: 'PM-001',
        department: 'Engineering',
        level: 'senior',
        employeeCount: 4,
        salaryMin: 90000,
        salaryMax: 130000,
        status: 'active',
        color: '#3B82F6'
      },
      {
        id: '6',
        title: 'Junior Developer',
        code: 'JD-001',
        department: 'Engineering',
        level: 'entry',
        employeeCount: 18,
        salaryMin: 45000,
        salaryMax: 65000,
        status: 'active',
        color: '#3B82F6'
      },
      {
        id: '7',
        title: 'Chief Technology Officer',
        code: 'CTO-001',
        department: 'Engineering',
        level: 'executive',
        employeeCount: 1,
        salaryMin: 150000,
        salaryMax: 200000,
        status: 'active',
        color: '#3B82F6'
      },
      {
        id: '8',
        title: 'Content Writer',
        code: 'CW-001',
        department: 'Marketing',
        level: 'entry',
        employeeCount: 5,
        salaryMin: 35000,
        salaryMax: 45000,
        status: 'inactive',
        color: '#10B981'
      }
    ];

    this.filteredJobTitles = [...this.jobTitles];
  }

  onSearchChange(): void {
    this.filterJobTitles();
  }

  onDepartmentChange(): void {
    this.filterJobTitles();
  }

  onLevelChange(): void {
    this.filterJobTitles();
  }

  private filterJobTitles(): void {
    this.filteredJobTitles = this.jobTitles.filter(jobTitle => {
      const matchesSearch = !this.searchTerm || 
        jobTitle.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        jobTitle.code.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        jobTitle.department.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesDepartment = !this.selectedDepartment || 
        jobTitle.department.toLowerCase() === this.selectedDepartment.toLowerCase();

      const matchesLevel = !this.selectedLevel || 
        jobTitle.level === this.selectedLevel;

      return matchesSearch && matchesDepartment && matchesLevel;
    });
  }

  getLevelBadgeClass(level: string): string {
    switch (level) {
      case 'entry':
        return 'bg-blue-100 text-blue-800';
      case 'mid':
        return 'bg-green-100 text-green-800';
      case 'senior':
        return 'bg-purple-100 text-purple-800';
      case 'executive':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  editJobTitle(jobTitle: JobTitle): void {
    console.log('Edit job title:', jobTitle);
    // Implement edit functionality
  }

  viewJobTitle(jobTitle: JobTitle): void {
    console.log('View job title:', jobTitle);
    // Implement view functionality
  }

  duplicateJobTitle(jobTitle: JobTitle): void {
    const newJobTitle: JobTitle = {
      ...jobTitle,
      id: Date.now().toString(),
      title: `${jobTitle.title} (Copy)`,
      code: `${jobTitle.code}-COPY`,
      employeeCount: 0
    };
    
    this.jobTitles.push(newJobTitle);
    this.filterJobTitles();
    console.log('Job title duplicated:', newJobTitle);
  }

  deleteJobTitle(jobTitle: JobTitle): void {
    if (jobTitle.employeeCount > 0) {
      alert(`Cannot delete ${jobTitle.title} because it has ${jobTitle.employeeCount} employees assigned to it.`);
      return;
    }

    if (confirm(`Are you sure you want to delete ${jobTitle.title}?`)) {
      this.jobTitles = this.jobTitles.filter(jt => jt.id !== jobTitle.id);
      this.filterJobTitles();
      console.log('Job title deleted:', jobTitle);
    }
  }

  // Statistics methods
  getTotalJobTitles(): number {
    return this.jobTitles.length;
  }

  getActiveJobTitles(): number {
    return this.jobTitles.filter(jt => jt.status === 'active').length;
  }

  getAvgSalary(): number {
    if (this.jobTitles.length === 0) return 0;
    const totalMin = this.jobTitles.reduce((sum, jt) => sum + jt.salaryMin, 0);
    const totalMax = this.jobTitles.reduce((sum, jt) => sum + jt.salaryMax, 0);
    return Math.round((totalMin + totalMax) / (2 * this.jobTitles.length));
  }

  getMostPopularDepartment(): string {
    const departmentCounts = this.jobTitles.reduce((acc, jt) => {
      acc[jt.department] = (acc[jt.department] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    return Object.entries(departmentCounts).reduce((a, b) => 
      departmentCounts[a[0]] > departmentCounts[b[0]] ? a : b
    )[0] || 'N/A';
  }
}
