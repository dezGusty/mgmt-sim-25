import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddJobTitle } from './add-job-title';

describe('AddJobTitle', () => {
  let component: AddJobTitle;
  let fixture: ComponentFixture<AddJobTitle>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddJobTitle]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddJobTitle);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
