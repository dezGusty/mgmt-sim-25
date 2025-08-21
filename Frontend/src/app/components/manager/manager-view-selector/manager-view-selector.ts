import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CustomNavbar } from '../../shared/custom-navbar/custom-navbar';

interface ManagerView {
  name: string;
  displayName: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  bgGradient: string;
}

@Component({
  selector: 'app-manager-view-selector',
  imports: [CommonModule, CustomNavbar],
  templateUrl: './manager-view-selector.html',
  styleUrls: ['./manager-view-selector.css'],
})
export class ManagerViewSelector implements OnInit {
  managerViews: ManagerView[] = [];
  userName = 'Manager';
  isLoading = false;

  constructor(private router: Router) {}

  ngOnInit() {
    this.loadManagerViews();
  }

  loadManagerViews() {
    this.managerViews = [
      {
        name: 'leave-management',
        displayName: 'Leave Management',
        description: 'Manage team leave requests, approvals, and review leave history',
        icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
        route: '/manager/leave',
        color: 'text-green-600',
        bgGradient: 'from-green-400 to-green-600'
      },
      {
        name: 'project-management',
        displayName: 'Project Management',
        description: 'Oversee project assignments, resource allocation, and team workloads',
        icon: 'M9 5H7a2 2 0 00-2 2v6a2 2 0 002 2h2m4-4h2a2 2 0 002-2V9a2 2 0 00-2-2h-2m0 0V5a2 2 0 00-2-2H9a2 2 0 00-2 2v2m0 0v6a2 2 0 002 2h2',
        route: '/manager/projects',
        color: 'text-blue-600',
        bgGradient: 'from-blue-400 to-blue-600'
      }
    ];
  }

  selectView(view: ManagerView) {
    console.log(`Manager selected view: ${view.name}, navigating to: ${view.route}`);
    this.router.navigate([view.route]);
  }

  goBack() {
    this.router.navigate(['/']);
  }
}