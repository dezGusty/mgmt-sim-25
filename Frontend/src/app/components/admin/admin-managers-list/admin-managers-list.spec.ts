import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminManagersList } from './admin-managers-list';

describe('AdminManagersList', () => {
  let component: AdminManagersList;
  let fixture: ComponentFixture<AdminManagersList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminManagersList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminManagersList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
