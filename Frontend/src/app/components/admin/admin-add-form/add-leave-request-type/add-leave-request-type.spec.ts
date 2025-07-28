import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddLeaveRequestType } from './add-leave-request-type';

describe('AddLeaveRequestType', () => {
  let component: AddLeaveRequestType;
  let fixture: ComponentFixture<AddLeaveRequestType>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddLeaveRequestType]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddLeaveRequestType);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
