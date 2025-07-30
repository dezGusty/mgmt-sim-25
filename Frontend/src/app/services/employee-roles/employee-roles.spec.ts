import { TestBed } from '@angular/core/testing';

import { EmployeeRolesService } from './employee-roles';

describe('EmployeeRoles', () => {
  let service: EmployeeRolesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmployeeRolesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
