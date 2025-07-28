import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddForm } from './admin-add-form';

describe('AddForm', () => {
  let component: AddForm;
  let fixture: ComponentFixture<AddForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
