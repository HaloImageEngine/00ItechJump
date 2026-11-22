import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VoicetrainCompoenent } from './voicetrain.component';

describe('VoicetrainCompoenent', () => {
  let component: VoicetrainCompoenent;
  let fixture: ComponentFixture<VoicetrainCompoenent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VoicetrainCompoenent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VoicetrainCompoenent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
