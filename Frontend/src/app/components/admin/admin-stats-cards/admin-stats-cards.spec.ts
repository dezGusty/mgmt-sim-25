import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminStatsCards } from './admin-stats-cards';

describe('AdminStatsCards', () => {
  let component: AdminStatsCards;
  let fixture: ComponentFixture<AdminStatsCards>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminStatsCards]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminStatsCards);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
