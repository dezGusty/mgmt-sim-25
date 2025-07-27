import { TestBed } from '@angular/core/testing';

import { JobTitlesService } from './job-titles-service';

describe('JobTitles', () => {
  let service: JobTitlesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(JobTitlesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
