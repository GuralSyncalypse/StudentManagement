import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CourseSectionListComponent } from './course-section-list.component';

describe('CourseSectionListComponent', () => {
  let component: CourseSectionListComponent;
  let fixture: ComponentFixture<CourseSectionListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CourseSectionListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CourseSectionListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
