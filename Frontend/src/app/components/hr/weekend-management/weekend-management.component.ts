import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WeekendConfigurationService, WeekendConfiguration, UpdateWeekendConfigurationRequest } from '../../../services/weekend-configuration/weekend-configuration.service';

@Component({
  selector: 'app-weekend-management',
  templateUrl: './weekend-management.component.html',
  styleUrl: './weekend-management.component.css',
  imports: [CommonModule, FormsModule],
  standalone: true
})
export class WeekendManagementComponent implements OnInit {
  weekendConfiguration: WeekendConfiguration | null = null;
  availableDays: string[] = [];
  selectedDays: { [key: string]: boolean } = {};
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';
  validationErrors: string[] = [];

  constructor(private weekendService: WeekendConfigurationService) {}

  ngOnInit() {
    this.availableDays = this.weekendService.getAvailableDays();
    this.initializeSelectedDays();
    this.loadWeekendConfiguration();
  }

  private initializeSelectedDays() {
    this.availableDays.forEach(day => {
      this.selectedDays[day] = false;
    });
  }

  loadWeekendConfiguration() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.weekendService.getWeekendConfiguration().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.weekendConfiguration = response.data;
          this.updateSelectedDays();
        } else {
          this.errorMessage = response.message || 'Failed to load weekend configuration';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading weekend configuration:', error);
        this.errorMessage = 'Failed to load weekend configuration. Please try again.';
        this.isLoading = false;
      }
    });
  }

  private updateSelectedDays() {
    if (!this.weekendConfiguration) return;
    
    // Reset all selections
    Object.keys(this.selectedDays).forEach(day => {
      this.selectedDays[day] = false;
    });
    
    // Set current weekend days as selected
    this.weekendConfiguration.weekendDays.forEach(day => {
      this.selectedDays[day] = true;
    });
  }

  onDaySelectionChange() {
    this.clearMessages();
    this.validateCurrentSelection();
  }

  private validateCurrentSelection() {
    const selectedDaysList = this.getSelectedDaysList();
    const validation = this.weekendService.validateConfiguration(selectedDaysList, selectedDaysList.length);
    this.validationErrors = validation.errors;
  }

  getSelectedDaysList(): string[] {
    return Object.keys(this.selectedDays).filter(day => this.selectedDays[day]);
  }

  private clearMessages() {
    this.errorMessage = '';
    this.successMessage = '';
    this.validationErrors = [];
  }

  saveConfiguration() {
    this.clearMessages();
    const selectedDaysList = this.getSelectedDaysList();
    
    // Validate before saving
    const validation = this.weekendService.validateConfiguration(selectedDaysList, selectedDaysList.length);
    if (!validation.isValid) {
      this.validationErrors = validation.errors;
      return;
    }

    this.isSaving = true;
    
    const updateRequest: UpdateWeekendConfigurationRequest = {
      weekendDays: selectedDaysList,
      weekendDaysCount: selectedDaysList.length
    };

    this.weekendService.updateWeekendConfiguration(updateRequest).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.weekendConfiguration = response.data;
          this.successMessage = 'Weekend configuration updated successfully!';
          setTimeout(() => this.successMessage = '', 5000); // Clear success message after 5 seconds
        } else {
          this.errorMessage = response.message || 'Failed to update weekend configuration';
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error updating weekend configuration:', error);
        this.errorMessage = 'Failed to update weekend configuration. Please try again.';
        this.isSaving = false;
      }
    });
  }

  resetToDefault() {
    this.clearMessages();
    
    // Set default weekend (Saturday, Sunday)
    Object.keys(this.selectedDays).forEach(day => {
      this.selectedDays[day] = day === 'Saturday' || day === 'Sunday';
    });
    
    this.validateCurrentSelection();
  }

  resetToCurrent() {
    this.clearMessages();
    this.updateSelectedDays();
    this.validateCurrentSelection();
  }

  getDayDisplayName(day: string): string {
    const dayMapping: { [key: string]: string } = {
      'Monday': 'Mon',
      'Tuesday': 'Tue', 
      'Wednesday': 'Wed',
      'Thursday': 'Thu',
      'Friday': 'Fri',
      'Saturday': 'Sat',
      'Sunday': 'Sun'
    };
    return dayMapping[day] || day;
  }

  getSelectedCount(): number {
    return this.getSelectedDaysList().length;
  }

  canSave(): boolean {
    return !this.isSaving && this.validationErrors.length === 0 && this.getSelectedCount() > 0;
  }
}
