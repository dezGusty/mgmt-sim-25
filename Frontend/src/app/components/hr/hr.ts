import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { HrService, IHrUserDto, PublicHoliday } from '../../services/hr/hr.service';

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
  imports: [CommonModule, FormsModule, CustomNavbar]
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

  onUploadCalendarSeparate(event: any) {
    const files: FileList = event?.target?.files || event;
    if (!files || (files instanceof FileList && files.length === 0)) return;

    const fileArray: File[] = files instanceof FileList ? Array.from(files) : Array.isArray(files) ? files : [files];
    const allowed = ['.xml', '.csv'];
    const valid = fileArray.filter(f => {
      const name = (f.name || '').toLowerCase();
      return allowed.some(ext => name.endsWith(ext));
    });

    if (valid.length === 0) {
      console.warn('No valid calendar files selected. Only .xml and .csv are allowed.');
      return;
    }

    console.log('Calendar files (separate):', valid.map(f => f.name));
  }

  triggerFileInput(fileInput: HTMLInputElement) {
    if ((window as any).showOpenFilePicker) {
      this.openCalendarPicker();
      return;
    }

    fileInput.click();
  }

  async openCalendarPicker() {
    try {
      const handles = await (window as any).showOpenFilePicker({
        multiple: false,
        types: [
          {
            description: 'Calendar files',
            accept: {
              'text/xml': ['.xml'],
              'text/csv': ['.csv']
            }
          }
        ],
        startIn: 'desktop'
      });

      if (!handles || handles.length === 0) return;

      const file = await handles[0].getFile();
      this.onUploadCalendarSeparate(file);
    } catch (err) {
      console.warn('showOpenFilePicker failed or was cancelled, falling back to input element', err);
    }
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

}
