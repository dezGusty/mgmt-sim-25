import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AddRequests } from './add-requests';

describe('AddRequests', () => {
  let component: AddRequests;
  let fixture: ComponentFixture<AddRequests>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddRequests],
    }).compileComponents();

    fixture = TestBed.createComponent(AddRequests);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
