import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminUsersList } from './admin-users-list';

describe('AdminManagersList', () => {
  let component: AdminUsersList;
  let fixture: ComponentFixture<AdminUsersList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUsersList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminUsersList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
