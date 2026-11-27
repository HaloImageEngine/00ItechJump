import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { NavComponent } from '../nav/nav.component';
import { BrandLogoComponent } from '../brand-logo/brand-logo.component';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/services/auth.service';
import { DeviceService } from '../../core/services/device.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NavComponent, BrandLogoComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private authService = inject(AuthService);
  protected device = inject(DeviceService);

  // Auth state
  isLoggedIn = false;
  userAlias: string | null = null; // e.g., 'WDBeaty' when available
  userId: string | null = null; // e.g., '1003' when available
  authLabel: 'Register' | 'Login' | 'LogOff' = 'Register';

  // Version modal
  showVersion = signal(false);
  versionId = environment.versionId;
  appVersion = environment.appVersion;

  // Keep a reference so we can remove the listener on destroy
  private userUpdatedHandler = () => {
    // LoginComponent fires this after successful login
    this.isLoggedIn = true;
    this.refreshAuthStateFromCookies();
  };

  ngOnInit(): void {
    // Initial cookie read
    this.refreshAuthStateFromCookies();

    // Debug check with flag
    this.debugUserInfo();

    // Listen for login component updates
    if (typeof window !== 'undefined') {
      window.addEventListener('lj:user-updated', this.userUpdatedHandler);
    }
  }

  ngOnDestroy(): void {
    if (typeof window !== 'undefined') {
      window.removeEventListener('lj:user-updated', this.userUpdatedHandler);
    }
  }

  // ----------------- Helpers -----------------

  /**
   * Read a cookie by name from document.cookie.
   */
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

  /**
   * Refresh alias & button label from cookies and local isLoggedIn flag.
   *
   * Rules:
   *  1. If user is logged in AND cookie available => "LogOff"
   *  2. If user is NOT logged in AND NO cookie   => "Register"
   *  3. If cookie available but user is logged off => "Login"
   */
  private refreshAuthStateFromCookies(): void {
    const aliasCookie = this.getCookie('useralias');
    this.userAlias = aliasCookie && aliasCookie.trim().length > 0
      ? aliasCookie.trim()
      : null;

    // Debug: Log all storage and cookie data
    console.log('=== HEADER COMPONENT DEBUG ===');
    console.log('localStorage itechjumpUserDisplay:', localStorage.getItem('itechjumpUserDisplay'));
    console.log('localStorage techjumpUserDisplay:', localStorage.getItem('techjumpUserDisplay'));
    console.log('Cookie useralias:', this.getCookie('useralias'));
    console.log('Cookie ljUserAuth:', this.getCookie('ljUserAuth'));
    console.log('Cookie ljUserDisplay:', this.getCookie('ljUserDisplay'));

    // Read userId from ljUserAuth cookie
    try {
      const authCookie = this.getCookie('ljUserAuth');
      console.log('Raw authCookie string:', authCookie);
      if (authCookie) {
        const authData = JSON.parse(authCookie);
        console.log('Parsed authData:', authData);
        this.userId = authData?.userId || authData?.UserId || null;
        console.log('Extracted userId:', this.userId, 'type:', typeof this.userId);
        // Update AuthService with userId
        this.authService.setUserId(this.userId);
      } else {
        this.userId = null;
        console.log('No authCookie found, userId set to null');
        this.authService.setUserId(null);
      }
    } catch (err) {
      console.error('Error parsing ljUserAuth cookie:', err);
      this.userId = null;
    }
    console.log('=== END DEBUG ===');

    const hasAliasCookie = !!this.userAlias;

    if (this.isLoggedIn && hasAliasCookie) {
      this.authLabel = 'LogOff';
    } else if (!this.isLoggedIn && !hasAliasCookie) {
      this.authLabel = 'Register';
    } else if (!this.isLoggedIn && hasAliasCookie) {
      this.authLabel = 'Login';
    } else {
      // Fallback: treat as logged out with no cookie
      this.authLabel = 'Register';
    }
  }

  onAuthClick(): void {
    // Decide action based on current label
    if (this.authLabel === 'LogOff') {
      // Mark as logged off and CLEAR the alias to hide it
      this.isLoggedIn = false;
      this.userAlias = null; // ✅ Clear alias on logoff
      // (Optional) you could clear additional auth cookies/token here
      console.log('LogOff clicked - mark user as logged out, alias cleared');
      this.refreshAuthStateFromCookies();
      // Navigate to logoff page
      this.router.navigate(['/logoff']);
    } else if (this.authLabel === 'Login') {
      this.userAlias = null;
      this.router.navigate(['/login']);
    } else if (this.authLabel === 'Register') {
      this.router.navigate(['/register']);
    }
  }

  openVersion() {
    this.showVersion.set(true);
  }

  closeVersion() {
    this.showVersion.set(false);
  }

  /**
   * Debug function to check login status and display user information
   * Only runs when debugMode flag is enabled in environment
   */
  private debugUserInfo(): void {
    console.log('🔍 Header Component - Debug Check Started');
    console.log('🔍 Debug Mode Flag:', environment.debugMode);

    if (environment.debugMode) {
      console.log('✅ DEBUG MODE IS ENABLED');
      console.log('========================');
      console.log('🔐 Login Status:');
      console.log('  - isLoggedIn:', this.isLoggedIn);
      console.log('  - authLabel:', this.authLabel);
      console.log('👤 User Information:');
      console.log('  - userAlias:', this.userAlias);
      console.log('  - userId:', this.userId);
      console.log('🍪 Cookies:');
      console.log('  - useralias:', this.getCookie('useralias'));
      console.log('  - ljUserAuth:', this.getCookie('ljUserAuth'));
      console.log('  - ljUserDisplay:', this.getCookie('ljUserDisplay'));
      console.log('💾 LocalStorage:');
      console.log('  - itechjumpUserDisplay:', localStorage.getItem('itechjumpUserDisplay'));
      console.log('  - techjumpUserDisplay:', localStorage.getItem('techjumpUserDisplay'));
      console.log('========================');
    } else {
      console.log('❌ Debug mode is OFF');
    }
    console.log('🔍 Header Debug Check Complete');
  }
}
