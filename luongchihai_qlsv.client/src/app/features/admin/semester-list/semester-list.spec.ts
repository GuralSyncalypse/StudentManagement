import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SemesterList } from './semester-list';

describe('SemesterList', () => {
  let component: SemesterList;
  let fixture: ComponentFixture<SemesterList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SemesterList],
    }).compileComponents();

    fixture = TestBed.createComponent(SemesterList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
