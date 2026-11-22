import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { API_ORIGIN } from '../../core/config/api-origin';
import { AuditService } from '../../core/services/audit.service';

// Top-level log to verify file is loaded
console.log('🔹 voicetrain.component.ts file loaded');
console.log('🔹 Environment at file load:', environment);

interface AudioQandA {
  Qid: number;
  Question: string;
  Description: string;
  AudioLoc: string;
  DocLoc: string;
  Active: boolean;
}

@Component({
  selector: 'app-voicetrain',
  imports: [],
  templateUrl: './voicetrain.component.html',
  styleUrl: './voicetrain.component.scss',
})
export class VoicetrainCompoenent implements OnInit {
  @ViewChild('audioPlayer') audioPlayer!: ElementRef<HTMLAudioElement>;

  docUrl!: SafeResourceUrl;
  mp3File: string = '';
  docxFile: string = '';

  private readonly httpMain = `${API_ORIGIN}/QandA`;
  private readonly apiUrl = `${API_ORIGIN}/api/ITechJump/Tech/GetAudioQandAByQid`;

  private userAlias: string = 'unknown';
  private userId: number = 0;
  private currentQid: number = 0;
  private currentAudioLoc: string = '';

  constructor(
    private sanitizer: DomSanitizer,
    private http: HttpClient,
    private auditService: AuditService
  ) {
    // Log immediately in constructor
    console.log('=== VoiceTrain Constructor Called ===');
    this.loadUserInfo();
  }

  ngOnInit(): void {
    // Log again in ngOnInit to ensure it runs
    console.log('=== VoiceTrain ngOnInit Called ===');
    this.loadAudioQandA();
  }

  private loadUserInfo(): void {
    try {
      // Try to get user info from ljUserAuth cookie
      const authCookie = this.getCookie('ljUserAuth');
      if (authCookie) {
        const authData = JSON.parse(authCookie);
        this.userAlias = authData.alias || 'unknown';
        this.userId = Number(authData.userId) || 0;
      }
    } catch {
      // Fallback to default values
      this.userAlias = 'unknown';
      this.userId = 0;
    }
  }

  private getCookie(name: string): string | null {
    try {
      if (typeof document === 'undefined') return null;
      const match = document.cookie
        .split(';')
        .map((c) => c.trim())
        .find((c) => c.startsWith(name + '='));
      if (!match) return null;
      return decodeURIComponent(match.substring(name.length + 1));
    } catch {
      return null;
    }
  }

  private getRandomQid(): number {
    // Generate random Qid between 1-8
    return Math.floor(Math.random() * 8) + 1;
  }

  private loadAudioQandA(): void {
    const qid = this.getRandomQid();
    this.currentQid = qid;
    console.log('Loading Audio Q&A for Qid:', qid);

    this.http.get<AudioQandA[]>(`${this.apiUrl}?Qid=${qid}`).subscribe({
      next: (response) => {
        if (response && response.length > 0) {
          const data = response[0];

          // Store the audio location
          this.currentAudioLoc = data.AudioLoc;

          // Populate mp3File and docxFile from API response
          this.mp3File = this.httpMain + data.AudioLoc;
          this.docxFile = this.httpMain + data.DocLoc;

          // Update the sanitized URL for iframe
          this.docUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.docxFile);

          console.log('✓ Audio Q&A Loaded Successfully');
          console.log('Question:', data.Question);
          console.log('MP3 File:', this.mp3File);
          console.log('Document File:', this.docxFile);

          // Log the voice training activity to audit log
          this.auditService.createAuditLog(
            this.userAlias,
            this.userId,
            'VOICE_TRAIN_START',
            `User ${this.userAlias} started voice training with Qid ${qid}`,
            `ITechJump-mobile|${this.currentAudioLoc}`,
            'voicetrain',
            'ItechJump'
          ).subscribe({
            next: (auditResponse) => {
              console.log('Audit log created with ID:', auditResponse.auditID);
            },
            error: (err) => {
              console.log('Failed to create audit log:', err);
            }
          });

          // Auto-play audio after a brief delay to ensure audio element is ready
          setTimeout(() => {
            this.playAudio();
          }, 100);
        }

        this.debugLog();
      },
      error: (error) => {
        console.error('✗ Error loading Audio Q&A:', error);
        this.debugLog();
      }
    });
  }

  private playAudio(): void {
    if (this.audioPlayer && this.audioPlayer.nativeElement) {
      const audio = this.audioPlayer.nativeElement;

      // Set up monitoring for when audio ends
      this.monitorAudioPlayback(audio);

      audio.load();
      audio.play().catch((error) => {
        console.log('Auto-play was prevented:', error);
        console.log('User interaction may be required to play audio');
      });
    }
  }

  private monitorAudioPlayback(audio: HTMLAudioElement): void {
    // Remove any existing event listener to avoid duplicates
    audio.removeEventListener('ended', this.handleAudioEnded);

    // Add event listener for when audio ends
    audio.addEventListener('ended', this.handleAudioEnded);

    console.log('🎵 Audio monitoring started');
  }

  private handleAudioEnded = (): void => {
    console.log('🎵 Audio playback completed - Cleaning up resources...');

    // Log the completion activity to audit log
    this.auditService.createAuditLog(
      this.userAlias,
      this.userId,
      'VOICE_TRAIN_COMPLETE',
      `User ${this.userAlias} completed voice training audio`,
      `ITechJump-mobile|${this.currentAudioLoc}`,
      'voicetrain',
      'ItechJump'
    ).subscribe({
      next: (auditResponse) => {
        console.log('Audit log created with ID:', auditResponse.auditID);
      },
      error: (err) => {
        console.log('Failed to create audit log:', err);
      }
    });

    // Clean up resources before refresh
    this.cleanupResources();

    // Refresh the page to load new content
    setTimeout(() => {
      window.location.reload();
    }, 500);
  }

  private cleanupResources(): void {
    console.log('🧹 Starting resource cleanup...');

    // Clean up audio element
    if (this.audioPlayer && this.audioPlayer.nativeElement) {
      const audio = this.audioPlayer.nativeElement;

      // Pause and reset audio
      audio.pause();
      audio.currentTime = 0;
      audio.src = '';
      audio.load();

      // Remove event listener
      audio.removeEventListener('ended', this.handleAudioEnded);

      console.log('✓ Audio resources cleaned');
    }

    // Clear MP3 and document references
    this.mp3File = '';
    this.docxFile = '';

    // Clear sanitized URL - this helps release iframe resources
    if (this.docUrl) {
      this.docUrl = this.sanitizer.bypassSecurityTrustResourceUrl('about:blank');
    }

    console.log('✓ All resources cleaned up');
  }

  private debugLog(): void {
    console.log('Debug Mode Flag:', environment.debugMode);
    console.log('Environment:', environment);

    if (environment.debugMode) {
      console.log('✓ Debug mode is ON');
      console.log('MP3 File:', this.mp3File);
      console.log('DOCX File:', this.docxFile);
    } else {
      console.log('✗ Debug mode is OFF');
    }

    console.log('========================================');
  }
}
