import { TestBed } from '@angular/core/testing';

import { LeaveRequestTypeService } from './leave-request-type';

describe('LeaveRequestType', () => {
  let service: LeaveRequestTypeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeaveRequestTypeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
