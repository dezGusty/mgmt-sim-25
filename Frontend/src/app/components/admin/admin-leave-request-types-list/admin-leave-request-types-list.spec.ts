import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminLeaveRequestTypesList } from './admin-leave-request-types-list';

describe('AdminLeaveRequestTypesList', () => {
  let component: AdminLeaveRequestTypesList;
  let fixture: ComponentFixture<AdminLeaveRequestTypesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminLeaveRequestTypesList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminLeaveRequestTypesList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
