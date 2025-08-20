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

  // paging
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
        // backend returns wrapper { message, data: { data: [...], totalCount, page, pageSize, totalPages } }
        const paged = res?.data || res; // try to handle both shapes
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
    // allow only editing vacation (totalVacationDays)
    this.editModel = { id: r.id, totalVacationDays: r.totalVacationDays };
  }

  saveEdit(index: number) {
    if (!this.editModel || this.editModel.id == null) return;
    const id = this.editModel.id;
    const days = this.editModel.totalVacationDays as number;

    this.hrService.adjustVacation(id, days).subscribe({
      next: (res: any) => {
        // on success, update local record
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
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    console.log('Calendar files (separate):', Array.from(files).map(f => f.name));
  }

  triggerFileInput(fileInput: HTMLInputElement) {
    fileInput.click();
  }

}
