import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminDepartmentsList } from './admin-departments-list';

describe('AdminDepartmentsList', () => {
  let component: AdminDepartmentsList;
  let fixture: ComponentFixture<AdminDepartmentsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminDepartmentsList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminDepartmentsList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
