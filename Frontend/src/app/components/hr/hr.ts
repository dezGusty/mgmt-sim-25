import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { HrService, IHrUserDto } from '../../services/hr/hr.service';

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

  constructor(private hrService: HrService) {}

  ngOnInit() {
    const year = new Date().getFullYear();
    this.loadPage(year, this.currentPage, this.pageSize);
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
    const days = this.editModel.totalVacationDays as number;

    this.hrService.adjustVacation(id, days).subscribe({
      next: (res: any) => {
        const r = this.records[index];
        r.totalVacationDays = days;
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

}
