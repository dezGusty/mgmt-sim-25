import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminJobTitlesList } from './admin-job-titles-list';

describe('AdminJobTitlesList', () => {
  let component: AdminJobTitlesList;
  let fixture: ComponentFixture<AdminJobTitlesList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminJobTitlesList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminJobTitlesList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
