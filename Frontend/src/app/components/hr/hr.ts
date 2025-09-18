import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { HrService, IHrUserDto, PublicHoliday, ImportedPublicHoliday, IPagedResponse } from '../../services/hr/hr.service';
import { HolidayCalendarComponent } from './calendar/holiday-calendar.component';
import { WeekendManagementComponent } from './weekend-management/weekend-management.component';
import * as Papa from 'papaparse';

interface HrRecord {
  id: number;
  name: string;
  department: string;
  totalVacationDays: number; // vacation
  usedVacationDays: number;
  remainingVacationDays: number;
}

@Component({
  selector: 'app-hr',
  templateUrl: './hr.html',
  styleUrl: './hr.css',
  imports: [CommonModule, FormsModule, CustomNavbar, HolidayCalendarComponent, WeekendManagementComponent]
})
export class Hr {
  records: HrRecord[] = [];

  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  isLoading = false;

  editingIndex: number | null = null;
  editModel: Partial<HrRecord> | null = null;

  publicHolidays: PublicHoliday[] = [];
  editingHolidayIndex: number | null = null;
  editHolidayModel: Partial<PublicHoliday> | null = null;
  isAddingNewHoliday = false;
  
  // Pagination for public holidays
  holidaysCurrentPage = 1;
  holidaysPageSize = 10;
  holidaysTotalCount = 0;
  
  importedHolidays: ImportedPublicHoliday[] = [];
  showImportPreview = false;
  
  currentView: 'table' | 'calendar' = 'table';
  
  activeTab: 'vacation' | 'holidays' | 'weekends' = 'vacation';

  get validImportedHolidaysCount(): number {
    return this.importedHolidays.filter(h => h.isValid).length;
  }

  setActiveTab(tab: 'vacation' | 'holidays' | 'weekends') {
    this.activeTab = tab;
  }

  constructor(private hrService: HrService) {}

  ngOnInit() {
    const year = new Date().getFullYear();
    this.loadPage(year, this.currentPage, this.pageSize);
    this.loadPublicHolidays(year);
  }

  loadPage(year: number, page: number, pageSize: number) {
    this.isLoading = true;
    this.hrService.getUsers(year, page, pageSize).subscribe({
      next: (res: any) => {
        const paged = res?.data || res; 
        const items = paged?.data || paged?.Data || paged;
        this.records = (items || []).map((u: IHrUserDto) => ({
          id: u.id,
          name: u.fullName || `${u.firstName} ${u.lastName}`,
          department: u.departmentName,
          totalVacationDays: u.vacation ?? u.totalLeaveDays ?? 0,
          usedVacationDays: u.usedLeaveDays ?? 0,
          remainingVacationDays: u.remainingLeaveDays ?? 0
        }));

        this.totalCount = paged?.totalCount || paged?.TotalCount || this.records.length;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load HR users', err);
        this.isLoading = false;
      }
    });
  }

  createEmptyRecords() {
    this.records = Array(this.pageSize).fill(null).map((_, index) => ({
      id: -1,
      name: '...',
      department: '...',
      totalVacationDays: 0,
      usedVacationDays: 0,
      remainingVacationDays: 0
    }));
  }

  getDisplayId(index: number): number {
    return (this.currentPage - 1) * this.pageSize + index + 1;
  }

  trackByIndex(index: number, item: HrRecord) {
    return item.id || index;
  }

  startEdit(index: number) {
    this.editingIndex = index;
    const r = this.records[index];
    this.editModel = { id: r.id, totalVacationDays: r.totalVacationDays };
  }

  saveEdit(index: number) {
    if (!this.editModel || this.editModel.id == null) return;
    const id = this.editModel.id;
    const days = Number(this.editModel.totalVacationDays ?? 0);
    const current = this.records[index]?.totalVacationDays ?? 0;
    const delta = days - current; 

    if (delta === 0) {
      this.cancelEdit();
      return;
    }

    this.hrService.adjustVacation(id, delta).subscribe({
      next: (res: any) => {
        const newVacation = res?.Data?.NewVacation ?? res?.data?.NewVacation ?? res?.Data?.newVacation ?? res?.data?.newVacation ?? days;
        const r = this.records[index];
        r.totalVacationDays = typeof newVacation === 'number' ? newVacation : days;
        r.remainingVacationDays = r.totalVacationDays - r.usedVacationDays;
        this.cancelEdit();
      },
      error: (err: any) => {
        console.error('Failed to adjust vacation', err);
      }
    });
  }

  cancelEdit() {
    this.editingIndex = null;
    this.editModel = null;
  }

  prevPage() {
    if (this.currentPage > 1 && !this.isLoading) {
      this.currentPage--;
      this.createEmptyRecords();
      this.loadPage(new Date().getFullYear(), this.currentPage, this.pageSize);
    }
  }

  nextPage() {
    if (!this.isLoading) {
      this.currentPage++;
      this.createEmptyRecords(); // Show empty rows while loading new page
      this.loadPage(new Date().getFullYear(), this.currentPage, this.pageSize);
    }
  }


  prevHolidaysPage() {
    if (this.holidaysCurrentPage > 1) {
      this.holidaysCurrentPage--;
      this.loadPublicHolidaysPage(new Date().getFullYear(), this.holidaysCurrentPage, this.holidaysPageSize);
    }
  }

  nextHolidaysPage() {
    this.holidaysCurrentPage++;
    this.loadPublicHolidaysPage(new Date().getFullYear(), this.holidaysCurrentPage, this.holidaysPageSize);
  }

  getHolidaysDisplayId(index: number): number {
    return (this.holidaysCurrentPage - 1) * this.holidaysPageSize + index + 1;
  }

  getActualHolidayCount(): number {
    return this.publicHolidays.filter(h => h.id).length;
  }

  getDisplayedHolidays(): PublicHoliday[] {
    return this.publicHolidays.filter(h => h.id !== undefined);
  }

  loadPublicHolidays(year: number) {
    this.loadPublicHolidaysPage(year, this.holidaysCurrentPage, this.holidaysPageSize);
  }

  loadPublicHolidaysPage(year: number, page: number, pageSize: number) {
    this.hrService.getPublicHolidaysPaginated(year, page, pageSize).subscribe({
      next: (response: IPagedResponse<PublicHoliday>) => {
        this.publicHolidays = response.data || [];
        this.holidaysTotalCount = response.totalCount || 0;
        this.holidaysCurrentPage = response.page || page;
        

      },
      error: (err: any) => {
        console.error('Failed to load public holidays', err);
        this.publicHolidays = [];
        this.holidaysTotalCount = 0;
      }
    });
  }

  startEditHoliday(index: number) {
    this.editingHolidayIndex = index;
    const holiday = this.publicHolidays[index];
    this.editHolidayModel = {
      id: holiday.id,
      name: holiday.name,
      date: holiday.date,
      isRecurring: holiday.isRecurring
    };
    this.isAddingNewHoliday = false;
  }

  saveHoliday(index: number) {
    if (!this.editHolidayModel) return;

    if (!this.validateHoliday(this.editHolidayModel)) {
      return;
    }

    const holiday: PublicHoliday = {
      id: this.editHolidayModel.id,
      name: this.editHolidayModel.name!.trim(),
      date: this.editHolidayModel.date!,
      isRecurring: this.editHolidayModel.isRecurring || false
    };

    if (holiday.id) {
      this.hrService.updatePublicHoliday(holiday).subscribe({
        next: (response: any) => {
          this.cancelHolidayEdit();
          this.loadPublicHolidaysPage(new Date().getFullYear(), this.holidaysCurrentPage, this.holidaysPageSize);
        },
        error: (err: any) => {
          console.error('Failed to update public holiday', err);
          alert('Failed to update public holiday. Please try again.');
        }
      });
    } else {
      this.hrService.createPublicHoliday(holiday).subscribe({
        next: (response: any) => {
          this.cancelHolidayEdit();
          this.loadPublicHolidaysPage(new Date().getFullYear(), this.holidaysCurrentPage, this.holidaysPageSize);
        },
        error: (err: any) => {
          console.error('Failed to create public holiday', err);
          alert('Failed to create public holiday. Please try again.');
        }
      });
    }
  }

  validateHoliday(holiday: Partial<PublicHoliday>): boolean {
    if (!holiday.name || holiday.name.trim().length === 0) {
      alert('Holiday name is required.');
      return false;
    }

    if (!holiday.date || holiday.date.trim().length === 0) {
      alert('Holiday date is required.');
      return false;
    }

    const date = new Date(holiday.date);
    if (isNaN(date.getTime())) {
      alert('Please enter a valid date.');
      return false;
    }

    const isDuplicate = this.publicHolidays.some((h, idx) => {
      if (this.editingHolidayIndex === idx) return false;
      return h.name.toLowerCase() === holiday.name!.toLowerCase().trim() && 
             h.date === holiday.date;
    });

    if (isDuplicate) {
      alert('A holiday with this name and date already exists.');
      return false;
    }

    return true;
  }

  cancelHolidayEdit() {
    // Reset editing state
    this.editingHolidayIndex = null;
    this.editHolidayModel = null;
    this.isAddingNewHoliday = false;
  }

  addNewHoliday() {
    // Cancel any existing editing
    if (this.editingHolidayIndex !== null) {
      this.cancelHolidayEdit();
    }

    this.editingHolidayIndex = -1; 
    this.editHolidayModel = {
      name: '',
      date: '',
      isRecurring: false
    };
    this.isAddingNewHoliday = true;

  }

  saveNewHoliday() {
    if (!this.editHolidayModel || !this.validateHoliday(this.editHolidayModel)) {
      return;
    }

    const holiday: Omit<PublicHoliday, 'id'> = {
      name: this.editHolidayModel.name!.trim(),
      date: this.editHolidayModel.date!,
      isRecurring: this.editHolidayModel.isRecurring || false
    };

    this.hrService.createPublicHoliday(holiday).subscribe({
      next: (response: any) => {
        console.log('Holiday created successfully:', response);
        this.cancelHolidayEdit();
        const year = new Date().getFullYear();
        this.loadPublicHolidaysPage(year, this.holidaysCurrentPage, this.holidaysPageSize);
      },
      error: (err: any) => {
        console.error('Failed to create holiday', err);
      }
    });
  }

  deleteHoliday(index: number) {
    const holiday = this.publicHolidays[index];
    if (!holiday.id) {
      this.publicHolidays.splice(index, 1);
      return;
    }

    if (confirm(`Are you sure you want to delete "${holiday.name}"?`)) {
      this.hrService.deletePublicHoliday(holiday.id).subscribe({
        next: () => {
          this.loadPublicHolidaysPage(new Date().getFullYear(), this.holidaysCurrentPage, this.holidaysPageSize);
        },
        error: (err: any) => {
          console.error('Failed to delete public holiday', err);
        }
      });
    }
  }

  trackByHolidayIndex(index: number, item: PublicHoliday) {
    return item.id || index;
  }

  downloadTemplate() {
    const templateData = [
      { Name: 'New Year\'s Day', Date: '2024-01-01', Recurring: 'true' },
      { Name: 'Independence Day', Date: '2024-07-04', Recurring: 'true' },
      { Name: 'Christmas', Date: '2024-12-25', Recurring: 'true' }
    ];

    const csv = Papa.unparse(templateData, {
      header: true,
      delimiter: ','
    });

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'public-holidays-template.csv';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  triggerImportFile() {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.csv,.xlsx';
    fileInput.style.display = 'none';
    
    fileInput.addEventListener('change', (event: any) => {
      this.onImportFile(event);
    });
    
    document.body.appendChild(fileInput);
    fileInput.click();
    document.body.removeChild(fileInput);
  }

  onImportFile(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const fileName = file.name.toLowerCase();
    if (!fileName.endsWith('.csv') && !fileName.endsWith('.xlsx')) {
      alert('Please select a CSV or Excel file.');
      return;
    }

    if (fileName.endsWith('.csv')) {
      this.parseCSV(file);
    } else {
      alert('Excel files (.xlsx) are not yet supported. Please convert to CSV format.');
    }
  }

  parseCSV(file: File) {
    Papa.parse(file, {
      header: true,
      skipEmptyLines: true,
      complete: (results: any) => {
        const importedData: ImportedPublicHoliday[] = results.data.map((row: any, index: number) => {
          const holiday: ImportedPublicHoliday = {
            name: row.Name || row.name || '',
            date: row.Date || row.date || '',
            recurring: row.Recurring || row.recurring || false
          };

          const validation = this.hrService.validateImportedHoliday(holiday);
          holiday.isValid = validation.isValid;
          holiday.errors = validation.errors;

          return holiday;
        });

        this.importedHolidays = importedData;
        this.showImportPreview = true;
      },
      error: (error: any) => {
        console.error('Error parsing CSV:', error);
        alert('Error parsing CSV file. Please check the file format.');
      }
    });
  }

  importValidHolidays() {
    if (this.importedHolidays.length === 0) {
      return;
    }

    const currentYear = new Date().getFullYear();
    
    this.hrService.importValidHolidays(this.importedHolidays, currentYear).subscribe({
      next: (result) => {
        let message = '';
        
        if (result.successful.length > 0) {
          message += `Successfully imported ${result.successful.length} holiday(s). `;
          this.loadPublicHolidaysPage(currentYear, this.holidaysCurrentPage, this.holidaysPageSize);
        }
        
        if (result.duplicates.length > 0) {
          message += `Warning: ${result.duplicates.length} holiday(s) already exist and were skipped: ${result.duplicates.join(', ')}. `;
        }
        
        if (result.errors.length > 0) {
          message += `Failed to import ${result.errors.length} holiday(s) due to errors. `;
          console.error('Import errors:', result.errors);
        }
        
        if (message) {
          alert(message.trim());
        }
        
        this.showImportPreview = false;
        this.importedHolidays = [];
      },
      error: (error) => {
        console.error('Error during import:', error);
        alert('An error occurred during import. Please try again.');
      }
    });
  }

  cancelImport() {
    this.showImportPreview = false;
    this.importedHolidays = [];
  }

  importFromAPI() {
    const currentYear = new Date().getFullYear();
    
    this.hrService.fetchRomanianHolidays(currentYear).subscribe({
      next: (apiHolidays) => {
        console.log('Raw API response:', apiHolidays);
        if (apiHolidays && apiHolidays.length > 0) {
          console.log('First holiday structure:', apiHolidays[0]);
          console.log('First holiday name:', apiHolidays[0].name);
        }
        const convertedHolidays = this.hrService.convertAPIResponseToImported(apiHolidays);
        this.importedHolidays = convertedHolidays;
        this.showImportPreview = true;
      },
      error: (err) => {
        console.error('Failed to fetch Romanian holidays from API:', err);
        alert('Failed to fetch holidays from API. Please check your internet connection and try again.');
      }
    });
  }

  switchToTableView() {
    this.currentView = 'table';
  }

  switchToCalendarView() {
    this.currentView = 'calendar';
  }

  onCalendarDateClick(date: Date) {
    if (this.editingHolidayIndex !== null) {
      this.cancelHolidayEdit();
    }

    const newHoliday: PublicHoliday = {
      name: '',
      date: this.formatDateForInput(date),
      isRecurring: false
    } as PublicHoliday;
    
    this.publicHolidays.push(newHoliday);
    const newIndex = this.publicHolidays.length - 1;
    this.editingHolidayIndex = newIndex;
    this.editHolidayModel = {
      name: '',
      date: this.formatDateForInput(date),
      isRecurring: false
    };
    this.isAddingNewHoliday = true;

    this.currentView = 'table';
  }

  onCalendarHolidayClick(holiday: PublicHoliday) {
    const index = this.publicHolidays.findIndex(h => 
      h.id === holiday.id || 
      (h.name === holiday.name && h.date === holiday.date)
    );
    
    if (index !== -1) {
      this.startEditHoliday(index);
      this.currentView = 'table';
    }
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  trackByDate(index: number, day: any): string {
    return day.date.toISOString();
  }

}
