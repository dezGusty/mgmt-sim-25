import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddRequestForm } from './add-request-form';

describe('AddRequestForm', () => {
  let component: AddRequestForm;
  let fixture: ComponentFixture<AddRequestForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddRequestForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddRequestForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
