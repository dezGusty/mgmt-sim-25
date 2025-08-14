import { Component, signal, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AddRequests } from './components/add-requests/add-requests';
import { AddRequestForm } from './components/add-request-form/add-request-form';
import { Router } from '@angular/router';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { ILeaveRequest } from '../../models/leave-request';
import { Subject, BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-manager-main-page',
  imports: [
    CommonModule,
    FormsModule,
    AddRequests,
    AddRequestForm,
    CustomNavbar,
  ],
  templateUrl: './main-page.html',
  styleUrl: './main-page.css',
})
export class ManagerMainPage implements OnDestroy {
  @ViewChild('addRequestsRef') addRequestsComponent!: AddRequests;
  showAddRequestForm = false;
  currentFilter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'Pending';
  searchTerm: string = '';
  searchCriteria: 'all' | 'employee' | 'department' | 'type' = 'all';

  private searchTermSubject = new BehaviorSubject<string>('');
  debouncedSearchTerm$ = this.searchTermSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );

  constructor(private router: Router) {}

  viewMode: 'card' | 'table' | 'calendar' = 'table';

  getSearchPlaceholder(): string {
    switch (this.searchCriteria) {
      case 'employee':
        return 'Search by employee name...';
      case 'department':
        return 'Search by department...';
      case 'type':
        return 'Search by leave type...';
      default:
        return 'Search by all fields...';
    }
  }

  onRequestAdded(newRequest: ILeaveRequest) {
    this.addRequestsComponent.addRequest(newRequest);
  }

  goBack() {
    this.router.navigate(['/']);
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved' | 'Rejected') {
    this.currentFilter = filter;
  }

  setViewMode(mode: 'card' | 'table' | 'calendar') {
    this.viewMode = mode;
  }

  setSearchTerm(term: string) {
    this.searchTerm = term;
  }

  onSearchInput(event: any) {
    const value = event.target.value;
    this.searchTerm = value;
    this.searchTermSubject.next(value);
  }

  ngOnDestroy() {
    this.searchTermSubject.complete();
  }
}
