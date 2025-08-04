import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';
import { LeaveRequest } from '../../../models/entities/iLeave-request';
import { RequestStatus } from '../../../models/enums/RequestStatus';

interface LeaveCategory {
  title: string;
  maxDays: number;
  used: number;
  color: string;
  note?: string;
}


interface LeaveTypeCache {
  typeId: number;
  typeName: string;
  staticMaxDays: number;
  remainingDaysData: any | null;
  isLoading: boolean;
  error: string;
  lastUpdated: number | null;
}

@Component({
  selector: 'app-user-leave-balance',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-leave-balance.html',
  styleUrl: './user-leave-balance.css',
})
export class UserLeaveBalance implements OnInit {
  @Output() close = new EventEmitter<void>();

  // Configurare de bază
  selectedYear = new Date().getFullYear();
  availableYears = [2023, 2024, 2025, 2026];

  // Date principale
  leaveRequestTypes: ILeaveRequestType[] = [];
  selectedLeaveRequestTypeId: number = 0;
  leaveTypesCache: LeaveTypeCache[] = [];

  // Stări de încărcare și erori
  isLoadingTypes = true;
  errorMessage = '';

  RequestStatus = RequestStatus;
  isLoadingUpcomingLeaves = true;
  upcomingLeaves: LeaveRequest[] = [];


  // Date legacy (păstrate pentru compatibilitate)
  leaveCategories: LeaveCategory[] = [];

  constructor(
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) { }

  ngOnInit() {
    this.loadLeaveRequestTypes();
    this.loadUpcomingLeaves();
  }

  // =====================
  // ÎNCĂRCARE INIȚIALĂ
  // =====================

  loadLeaveRequestTypes() {
    this.isLoadingTypes = true;
    this.errorMessage = '';

    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        this.leaveRequestTypes = types.data;
        this.initializeCache(types.data);
        this.isLoadingTypes = false;

        if (types.data.length > 0) {
          this.selectedLeaveRequestTypeId = types.data[0].id;
          this.loadDataForSelectedType();
        }
      },
      error: (err) => {
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
        console.error('Error loading leave types:', err);
      }
    });
  }

  loadUpcomingLeaves() {
    this.isLoadingUpcomingLeaves = true;
    this.leaveRequestService.getCurrentUserLeaveRequests().subscribe({
      next: (response) => {
        const currentDate = new Date();
        const currentMonth = currentDate.getMonth();
        const currentYear = currentDate.getFullYear();
        
        this.upcomingLeaves = response.data.filter(req => {
          if (req.requestStatus !== RequestStatus.APPROVED) return false;

          const startDate = new Date(req.startDate);
          const endDate = new Date(req.endDate);

         const isInCurrentMonth = (
          (startDate.getMonth() === currentMonth && startDate.getFullYear() === currentYear) ||
          (endDate.getMonth() === currentMonth && endDate.getFullYear() === currentYear) ||
          (startDate <= new Date(currentYear, currentMonth, 1) && 
           endDate >= new Date(currentYear, currentMonth + 1, 0))
        );

          return isInCurrentMonth;
        });

        this.isLoadingUpcomingLeaves = false;
      },
      error: (err) => {
        this.isLoadingUpcomingLeaves = false;
        this.errorMessage = 'Failed to load upcoming leaves.';
        console.error('Error loading upcoming leaves:', err);
      }
    });
  }

  private initializeCache(types: ILeaveRequestType[]) {
    this.leaveTypesCache = types.map(type => ({
      typeId: type.id,
      typeName: type.title,
      staticMaxDays: type.maxDays,
      remainingDaysData: null,
      isLoading: false,
      error: '',
      lastUpdated: null
    }));
  }

  // =====================
  // GESTIONARE EVENIMENTE
  // =====================

  onLeaveTypeChange() {
    // Asigură-te că e număr, nu string
    this.selectedLeaveRequestTypeId = Number(this.selectedLeaveRequestTypeId);

    // Forțează actualizarea template-ului
    setTimeout(() => {
      this.loadDataForSelectedType();
    }, 0);
  }

  onYearChange() {
    this.clearYearCache();
    this.loadDataForSelectedType();
  }

  // =====================
  // ÎNCĂRCARE DATE API
  // =====================

  private loadDataForSelectedType() {
    const cacheEntry = this.getCurrentTypeCache();

    if (!cacheEntry) {
      return;
    }

    // Verifică dacă avem deja datele în cache pentru anul curent
    if (this.hasValidCacheData(cacheEntry)) {
      return;
    }

    this.fetchRemainingDaysData(cacheEntry);
  }

  private fetchRemainingDaysData(cacheEntry: LeaveTypeCache) {
    if (!this.selectedLeaveRequestTypeId || this.selectedLeaveRequestTypeId === 0) {
      cacheEntry.remainingDaysData = null;
      return;
    }


    cacheEntry.isLoading = true;
    cacheEntry.error = '';

    this.leaveRequestService.getCurrentUserRemainingLeaveDays(
      this.selectedLeaveRequestTypeId,
      this.selectedYear
    ).subscribe({
      next: (response) => {
        cacheEntry.remainingDaysData = response.data;
        cacheEntry.isLoading = false;
        cacheEntry.error = '';
        cacheEntry.lastUpdated = Date.now();
      },
      error: (err) => {
        console.error('API error:', err);
        cacheEntry.isLoading = false;
        cacheEntry.remainingDaysData = null;
        cacheEntry.error = 'Failed to load remaining days information.';
      }
    });
  }

  // =====================
  // CACHE MANAGEMENT
  // =====================

  private getCurrentTypeCache(): LeaveTypeCache | undefined {
    return this.leaveTypesCache.find(cache => cache.typeId === this.selectedLeaveRequestTypeId);
  }

  private hasValidCacheData(cacheEntry: LeaveTypeCache): boolean {
    const isValid = cacheEntry.remainingDaysData !== null &&
      cacheEntry.error === '' &&
      !cacheEntry.isLoading;

    return isValid;
  }

  private clearYearCache() {
    this.leaveTypesCache.forEach(cache => {
      cache.remainingDaysData = null;
      cache.isLoading = false;
      cache.error = '';
      cache.lastUpdated = null;
    });
  }

  // =====================
  // GETTERS FOR TEMPLATE
  // =====================

  get isLoadingRemainingDays(): boolean {
    const cache = this.getCurrentTypeCache();
    return cache?.isLoading || false;
  }

  get remainingDaysError(): string {
    const cache = this.getCurrentTypeCache();
    return cache?.error || '';
  }

  get remainingDaysInfo(): any | null {
    const cache = this.getCurrentTypeCache();
    const result = cache?.remainingDaysData || null;
    return result;
  }

  getSelectedLeaveTypeMaxDays(): number {
    const cache = this.getCurrentTypeCache();
    if (!cache) return 0;

    if (cache.remainingDaysData?.totalDays) {
      return cache.remainingDaysData.totalDays;
    }

    return cache.staticMaxDays;
  }

  getSelectedLeaveTypeName(): string {
    const cache = this.getCurrentTypeCache();
    const name = cache?.typeName || 'Select leave type';

    return name;
  }

  getSelectedTypeRemainingDays(): number {
    const cache = this.getCurrentTypeCache();
    if (!cache) return 0;

    if (cache.remainingDaysData?.remainingDays !== undefined) {
      return cache.remainingDaysData.remainingDays;
    }

    return cache.staticMaxDays;
  }

  getSelectedTypeUsedDays(): number {
    const totalDays = this.getSelectedLeaveTypeMaxDays();
    const remainingDays = this.getSelectedTypeRemainingDays();
    const usedDays = totalDays - remainingDays;

    return Math.max(0, usedDays);
  }

  getSelectedTypeUsagePercentage(): number {
    const totalDays = this.getSelectedLeaveTypeMaxDays();
    if (totalDays === 0) return 0;

    const usedDays = this.getSelectedTypeUsedDays();
    return (usedDays / totalDays) * 100;
  }

  // =====================
  // LEGACY METHODS (kept for compatibility)
  // =====================

  getTotalDays(): number {
    return this.getSelectedLeaveTypeMaxDays();
  }

  getUsagePercentage(): number {
    return (this.getUsedDays() / this.getTotalDays()) * 100;
  }

  getUsedDays(): number {
    return this.leaveCategories.reduce((sum, category) => sum + category.used, 0);
  }
}