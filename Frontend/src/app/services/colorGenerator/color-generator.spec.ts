import { TestBed } from '@angular/core/testing';

import { ColorGenerator } from './color-generator';

describe('ColorGenerator', () => {
  let service: ColorGenerator;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ColorGenerator);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
