import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { YourQuestionComponent } from './your-question.component';

describe('YourQuestionComponent', () => {
  let component: YourQuestionComponent;
  let fixture: ComponentFixture<YourQuestionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YourQuestionComponent, HttpClientTestingModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(YourQuestionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
