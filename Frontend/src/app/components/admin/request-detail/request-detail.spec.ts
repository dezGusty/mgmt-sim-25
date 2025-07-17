import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestDetail } from './request-detail';

describe('RequestDetail', () => {
  let component: RequestDetail;
  let fixture: ComponentFixture<RequestDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
