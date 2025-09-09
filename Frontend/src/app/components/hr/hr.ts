import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { HrService, IHrUserDto, PublicHoliday, ImportedPublicHoliday } from '../../services/hr/hr.service';
import { HolidayCalendarComponent } from './calendar/holiday-calendar.component';
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
  imports: [CommonModule, FormsModule, CustomNavbar, HolidayCalendarComponent]
})
export class Hr {
  records: HrRecord[] = [];

  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  editingIndex: number | null = null;
  editModel: Partial<HrRecord> | null = null;

  publicHolidays: PublicHoliday[] = [];
  editingHolidayIndex: number | null = null;
  editHolidayModel: Partial<PublicHoliday> | null = null;
  isAddingNewHoliday = false;
  
  importedHolidays: ImportedPublicHoliday[] = [];
  showImportPreview = false;
  
  currentView: 'table' | 'calendar' = 'table';

  get validImportedHolidaysCount(): number {
    return this.importedHolidays.filter(h => h.isValid).length;
  }

  constructor(private hrService: HrService) {}

  ngOnInit() {
    const year = new Date().getFullYear();
    this.loadPage(year, this.currentPage, this.pageSize);
    this.loadPublicHolidays(year);
  }

  loadPage(year: number, page: number, pageSize: number) {
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
      },
      error: (err: any) => {
        console.error('Failed to load HR users', err);
      }
    });
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
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPage(new Date().getFullYear(), this.currentPage, this.pageSize);
    }
  }

  nextPage() {
    this.currentPage++;
    this.loadPage(new Date().getFullYear(), this.currentPage, this.pageSize);
  }


  loadPublicHolidays(year: number) {
    this.hrService.getPublicHolidays(year).subscribe({
      next: (response: any) => {
        // Handle different response formats
        if (Array.isArray(response)) {
          this.publicHolidays = response;
        } else if (response?.data && Array.isArray(response.data)) {
          this.publicHolidays = response.data;
        } else if (response?.Data && Array.isArray(response.Data)) {
          this.publicHolidays = response.Data;
        } else {
          this.publicHolidays = [];
        }
      },
      error: (err: any) => {
        console.error('Failed to load public holidays', err);
        this.publicHolidays = [];
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
          // Handle different response formats
          let updatedHoliday: PublicHoliday;
          if (response?.data) {
            updatedHoliday = response.data;
          } else if (response?.Data) {
            updatedHoliday = response.Data;
          } else {
            updatedHoliday = response;
          }
          
          this.publicHolidays[index] = updatedHoliday;
          this.cancelHolidayEdit();
        },
        error: (err: any) => {
          console.error('Failed to update public holiday', err);
          alert('Failed to update public holiday. Please try again.');
        }
      });
    } else {
      this.hrService.createPublicHoliday(holiday).subscribe({
        next: (response: any) => {
          // Handle different response formats
          let newHoliday: PublicHoliday;
          if (response?.data) {
            newHoliday = response.data;
          } else if (response?.Data) {
            newHoliday = response.Data;
          } else {
            newHoliday = response;
          }
          
          if (this.isAddingNewHoliday) {
            this.publicHolidays.splice(index, 1);
            this.publicHolidays.push(newHoliday);
          } else {
            this.publicHolidays[index] = newHoliday;
          }
          this.cancelHolidayEdit();
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
    if (this.isAddingNewHoliday && this.editingHolidayIndex !== null) {
      this.publicHolidays.splice(this.editingHolidayIndex, 1);
    }
    this.editingHolidayIndex = null;
    this.editHolidayModel = null;
    this.isAddingNewHoliday = false;
  }

  addNewHoliday() {
    console.log('Adding new holiday...'); // Debug log
    console.log('Current publicHolidays:', this.publicHolidays); // Debug log

    // Ensure publicHolidays is an array
    if (!Array.isArray(this.publicHolidays)) {
      this.publicHolidays = [];
    }

    // Cancel any existing editing
    if (this.editingHolidayIndex !== null) {
      this.cancelHolidayEdit();
    }

    // Remove any existing unsaved holidays (those without IDs)
    this.publicHolidays = this.publicHolidays.filter(h => h.id);

    const newHoliday: PublicHoliday = {
      name: '',
      date: '',
      isRecurring: false
    } as PublicHoliday;
    
    this.publicHolidays.push(newHoliday);
    const newIndex = this.publicHolidays.length - 1;
    this.editingHolidayIndex = newIndex;
    this.editHolidayModel = {
      name: '',
      date: '',
      isRecurring: false
    };
    this.isAddingNewHoliday = true;

    console.log('New holiday added at index:', newIndex); // Debug log
    console.log('Updated publicHolidays:', this.publicHolidays); // Debug log
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
          this.publicHolidays.splice(index, 1);
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
          this.publicHolidays.push(...result.successful);
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
    // Cancel any existing editing first
    if (this.editingHolidayIndex !== null) {
      this.cancelHolidayEdit();
    }

    // Add new holiday for the clicked date
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

    // Switch to table view to show the editing interface
    this.currentView = 'table';
  }

  onCalendarHolidayClick(holiday: PublicHoliday) {
    // Find the holiday in the array and start editing it
    const index = this.publicHolidays.findIndex(h => 
      h.id === holiday.id || 
      (h.name === holiday.name && h.date === holiday.date)
    );
    
    if (index !== -1) {
      this.startEditHoliday(index);
      // Switch to table view to show the editing interface
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
