import { Component, OnInit, inject } from '@angular/core';
import { AuditService } from '../../core/services/audit.service';

@Component({
  selector: 'app-test-pc',
  imports: [],
  templateUrl: './test-pc.compoent.html',
  styleUrl: './test-pc.compoent.scss',
})
export class TestPcCompoent implements OnInit {
  private auditService = inject(AuditService);
  private userAlias: string = 'unknown';
  private userId: number = 0;

  ngOnInit(): void {
    this.loadUserInfo();
    this.logTestPcPageView();
  }

  private loadUserInfo(): void {
    try {
      const authCookie = this.getCookie('ljUserAuth');
      if (authCookie) {
        const authData = JSON.parse(authCookie);
        this.userAlias = authData.alias || 'unknown';
        this.userId = Number(authData.userId) || 0;
      }
    } catch {
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

  private logTestPcPageView(): void {
    this.auditService.createAuditLog(
      this.userAlias,
      this.userId,
      'PAGE_VIEW',
      `User ${this.userAlias} viewed test-pc page`,
      'ITechJump-mobile',
      'test-pc',
      'ItechJump'
    ).subscribe({
      next: (response) => {
        console.log('Audit log created with ID:', response.auditID);
      },
      error: (err) => {
        console.log('Failed to create audit log:', err);
      }
    });
  }
}
