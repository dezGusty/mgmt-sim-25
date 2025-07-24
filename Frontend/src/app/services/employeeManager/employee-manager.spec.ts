import { TestBed } from '@angular/core/testing';

import { EmployeeManagerService } from './employee-manager';

describe('EmployeeManager', () => {
  let service: EmployeeManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmployeeManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
