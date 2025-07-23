import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminLeaveTypesList } from './admin-leave-request-types-list';

describe('AdminLeaveRequestTypesList', () => {
  let component: AdminLeaveTypesList;
  let fixture: ComponentFixture<AdminLeaveTypesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminLeaveTypesList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminLeaveTypesList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
