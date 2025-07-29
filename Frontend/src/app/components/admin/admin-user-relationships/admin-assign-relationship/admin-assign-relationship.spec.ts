import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminAssignRelationship } from './admin-assign-relationship';

describe('AdminAssignRelationship', () => {
  let component: AdminAssignRelationship;
  let fixture: ComponentFixture<AdminAssignRelationship>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminAssignRelationship]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminAssignRelationship);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
