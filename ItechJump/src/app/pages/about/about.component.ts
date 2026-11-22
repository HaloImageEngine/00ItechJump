import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandLogoComponent } from '../../shared/brand-logo/brand-logo.component';
import { AuditService } from '../../core/services/audit.service';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule, BrandLogoComponent],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {
  private auditService = inject(AuditService);
  private userAlias: string = 'unknown';
  private userId: number = 0;

  ngOnInit(): void {
    this.loadUserInfo();
    this.logPageView();
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

  private logPageView(): void {
    this.auditService.createAuditLog(
      this.userAlias,
      this.userId,
      'PAGE_VIEW',
      `User ${this.userAlias} viewed about page`,
      'ITechJump-mobile',
      'about',
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
