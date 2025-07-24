import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminUserRelationships } from './admin-user-relationships';

describe('AdminUserRelationships', () => {
  let component: AdminUserRelationships;
  let fixture: ComponentFixture<AdminUserRelationships>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUserRelationships]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminUserRelationships);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
