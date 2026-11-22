import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';
import { BrandLogoComponent } from '../../shared/brand-logo/brand-logo.component';

@Component({
  selector: 'app-logoff',
  imports: [RouterModule, BrandLogoComponent],
  templateUrl: './logoff.component.html',
  styleUrl: './logoff.component.scss',
})
export class LogoffComponent implements OnInit {
  private router = inject(Router);
  private auth = inject(AuthService);

  ngOnInit(): void {
    this.auth.clearSession();
    try {
      if (typeof document !== 'undefined') {
        const expire = 'Thu, 01 Jan 1970 00:00:00 GMT';
        const names = ['ljUserAuth', 'ljUserDisplay', 'useralias', 'useremail'];
        for (const name of names) {
          document.cookie = `${name}=; Expires=${expire}; Path=/; SameSite=Lax`;
        }
        localStorage.removeItem('techjumpUserDisplay');
      }
    } catch {
      /* ignore */
    }
    void this.router.navigate(['/main-page']);
  }
}