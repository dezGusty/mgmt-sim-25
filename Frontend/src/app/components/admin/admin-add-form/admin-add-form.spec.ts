import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminAddAdminForm } from './admin-add-form';

describe('AdminAddAdminForm', () => {
  let component: AdminAddAdminForm;
  let fixture: ComponentFixture<AdminAddAdminForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminAddAdminForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminAddAdminForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
