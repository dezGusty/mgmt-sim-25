import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';

interface HrRecord {
  id?: number;
  name: string;
  department: string;
  totalVacationDays: number;
  usedVacationDays: number;
}

@Component({
  selector: 'app-hr',
  templateUrl: './hr.html',
  styleUrl: './hr.css',
  imports: [CommonModule, FormsModule, CustomNavbar]
})
export class Hr {
  // Hardcoded demo data - will be replaced with backend data later
  records: HrRecord[] = [
    { name: 'Alice Popescu', department: 'Finance', totalVacationDays: 25, usedVacationDays: 8 },
    { name: 'Bogdan Ionescu', department: 'Engineering', totalVacationDays: 20, usedVacationDays: 12 },
    { name: 'Carmen Dinu', department: 'Marketing', totalVacationDays: 22, usedVacationDays: 5 },
  ];

  // Keeps track of which row is in edit mode (index) or null for none
  editingIndex: number | null = null;

  // Temporary model for editing
  editModel: HrRecord | null = null;

  constructor() {}

  ngOnInit() {
    // placeholder for future backend calls
  }

  getDisplayId(index: number): number {
    // UI id: 1-based index (auto-incremented)
    return index + 1;
  }

  trackByIndex(index: number, item: HrRecord) {
    return index;
  }

  startEdit(index: number) {
    this.editingIndex = index;
    const r = this.records[index];
    this.editModel = { ...r };
  }

  saveEdit(index: number) {
    if (!this.editModel) return;
    this.records[index] = { ...this.editModel };
    this.cancelEdit();
  }

  cancelEdit() {
    this.editingIndex = null;
    this.editModel = null;
  }

  // Placeholder upload handler - currently just logs filename(s)
  onUploadCalendar(event: any) {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;
    const names = Array.from(files).map(f => f.name).join(', ');
    console.log('Uploaded calendar files:', names);
  }

  triggerFileInput(fileInput: HTMLInputElement) {
    fileInput.click();
  }

}
