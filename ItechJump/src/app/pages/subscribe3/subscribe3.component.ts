// src/app/subscribe3/subscribe3.component.ts
import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { API_ORIGIN } from '../../core/config/api-origin';
import { AuthService } from '../../core/services/auth.service';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';

const API_CREATE = `${API_ORIGIN}/api/techjumpsubscription`;
const API_USER_SUBSCRIPTION = `${API_ORIGIN}/api/ITechJump/userid/`;
const API_SUBSCRIPTION_BY_ALIAS = `${API_ORIGIN}/api/ITechJump/subscription/usersubscriptionsbyAlias`;

// Response from /api/ITechJump/userid/{userId}
interface UserSubscriptionResponse {
  ok: boolean;
  subscription: {
    PlanCode: string;
    PlanName: string;
    Description: string;
    SubscriptionId: number;
    Price: number;
    PlanStartDate: string;
    PlanEndDate: string;
    BillingPeriod: string;
    IsActive: boolean;
  };
}

// Response from /api/ITechJump/subscription/usersubscriptionsbyAlias
interface UserSubscriptionByAliasResponse {
  UserId: number;
  UserAlias: string;
  SubscriptionId: number;
  PlanCode: string;
  Status: string;
  AutoRenew: boolean;
  CurrentPeriodStartUtc: string;
  CurrentPeriodEndUtc: string;
  TrialEndUtc: string | null;
  PurchaseCurrency: string;
  PlanName: string;
  PlanDescription: string;
  PlanCurrentPrice: number;
  BillingPeriod: string;
  PlanIsActive: boolean;
}

// Unified interface for display
interface SubscriptionDto {
  // Identity
  SubscriptionId?: number;
  UserId?: number;
  UserAlias?: string;

  // Plan details
  PlanCode?: string;
  PlanName?: string;
  Description?: string;

  // Pricing
  Price?: number;
  PriceAtPurchase?: number;
  Currency?: string;
  Quantity?: number;

  // Dates
  PlanStartDate?: string;
  PlanEndDate?: string;
  StartUtc?: string;
  CurrentPeriodStartUtc?: string;
  CurrentPeriodEndUtc?: string;
  TrialEndUtc?: string | null;
  CreatedAt?: string;
  UpdatedAt?: string;

  // Status
  BillingPeriod?: string;
  IsActive?: boolean;
  Status?: string;
  AutoRenew?: boolean;

  // Extras
  TicketSets?: number;
}

interface SubscriptionResponse {
  ok: boolean;
  subscription: SubscriptionDto;
}

interface NavigationItem {
  label: string;
  to: string;
  emphasis?: boolean;
}

interface UserData {
  UserId?: number;
  UserAlias?: string;
  Email?: string;
  FirstName?: string;
  LastName?: string;
  [key: string]: any;
}

import { BrandLogoComponent } from '../../shared/brand-logo/brand-logo.component';

@Component({
  selector: 'app-subscribe3',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    BrandLogoComponent
  ],
  templateUrl: './subscribe3.component.html',
  styleUrls: ['./subscribe3.component.scss']
})
export class Subscribe3Component {
  userId = '';
  userAlias = '';
  planCode = '';
  userData: UserData | null = null;

  loading = signal(false);
  error = signal('');
  sub: SubscriptionDto | null = null;

  navigationItems: NavigationItem[] = [
    { label: 'PickPower',  to: '/pick/squarePower',   emphasis: true },
    { label: 'PickMega',   to: '/pick/squareMega',    emphasis: true },
    { label: 'PickTexas',  to: '/pick/squareTexas',   emphasis: true },
    { label: 'PickTwoStep',to: '/pick/squareTwoStep', emphasis: true },
    { label: 'PickCash5',  to: '/pick/squareCash5',   emphasis: true },
    { label: 'Profile',    to: '/profile' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private authService: AuthService
  ) {
    // Get userAlias from cookie on component initialization
    const aliasCookie = this.getCookie('useralias');
    this.userAlias = aliasCookie?.trim() || '';

    // Get userId from AuthService/cookie
    const authUserId = this.authService.getUserId() || this.authService.getUserIdFromCookie();
    if (authUserId) {
      this.userId = authUserId;
    }

    this.route.queryParamMap.subscribe(params => {
      this.planCode = (params.get('planCode') || '').trim();

      // Try to get userId and userAlias from query params first
      const paramUserId = (params.get('userId') || '').trim();
      const paramUserAlias = (params.get('userAlias') || '').trim();

      if (paramUserId) {
        this.userId = paramUserId;
      }
      if (paramUserAlias) {
        this.userAlias = paramUserAlias;
      }

      // Always fetch subscription by alias on render
      if (this.userAlias) {
        this.fetchSubscriptionByAlias();
      } else {
        this.error.set('User not logged in. Please log in first.');
      }
    });
  }

  // Helper to get cookie value
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

  // ---- Helpers (ports of your React helpers) ----

  fmtDate(v: any): string {
    if (!v) return '—';
    try {
      const d = new Date(v);
      if (isNaN(d.getTime())) return String(v);
      return d.toLocaleString(); // local timezone readable
    } catch {
      return String(v);
    }
  }

  fmtMoney(n: any, currency = 'USD'): string {
    const num = Number(n);
    if (!isFinite(num)) return '—';
    try {
      return new Intl.NumberFormat(undefined, {
        style: 'currency',
        currency,
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(num);
    } catch {
      return `$${num.toFixed(2)}`;
    }
  }

  statusClass(status: string | boolean | undefined): string {
    if (typeof status === 'boolean') {
      return status ? 'status-active' : 'status-ended';
    }
    const v = String(status || '').toLowerCase();
    if (v === 'active') return 'status-active';
    if (v === 'trialing') return 'status-trialing';
    if (v === 'canceled' || v === 'ended') return 'status-ended';
    return 'status-default';
  }

  get hdrPlan(): string {
    return this.sub?.PlanCode || this.planCode || '—';
  }

  get hdrName(): string {
    return this.sub?.PlanName || 'Subscription Created';
  }

// ---- API call to fetch user subscription data by userId ----

  private fetchUserData(userId: string): void {
    this.loading.set(true);
    this.error.set('');

    this.http.get<UserSubscriptionResponse>(`${API_USER_SUBSCRIPTION}${userId}`).subscribe({
      next: (res) => {
        if (res?.ok && res?.subscription) {
          // Map the response to SubscriptionDto
          this.sub = {
            PlanCode: res.subscription.PlanCode,
            PlanName: res.subscription.PlanName,
            Description: res.subscription.Description,
            SubscriptionId: res.subscription.SubscriptionId,
            Price: res.subscription.Price,
            PlanStartDate: res.subscription.PlanStartDate,
            PlanEndDate: res.subscription.PlanEndDate,
            BillingPeriod: res.subscription.BillingPeriod,
            IsActive: res.subscription.IsActive
          };

          console.log('✓ User subscription loaded from userid API:', this.sub);
          this.loading.set(false);
        } else {
          // Try fetching by alias if userId fetch didn't return expected data
          this.fetchSubscriptionByAlias();
        }
      },
      error: (err) => {
        console.error('✗ Error fetching user subscription by userId:', err);
        // Fallback to fetch by alias
        this.fetchSubscriptionByAlias();
      }
    });
  }

  // ---- API call to fetch subscription by userAlias ----

  private fetchSubscriptionByAlias(): void {
    if (!this.userAlias) {
      this.error.set('User alias not available.');
      this.loading.set(false);
      return;
    }

    this.http.get<UserSubscriptionByAliasResponse>(
      `${API_SUBSCRIPTION_BY_ALIAS}?useralias=${this.userAlias}`
    ).subscribe({
      next: (data) => {
        // Map the response to SubscriptionDto with all available fields
        this.sub = {
          // Identity
          SubscriptionId: data.SubscriptionId,
          UserId: data.UserId,
          UserAlias: data.UserAlias,

          // Plan details
          PlanCode: data.PlanCode,
          PlanName: data.PlanName,
          Description: data.PlanDescription,

          // Pricing
          Price: data.PlanCurrentPrice,
          PriceAtPurchase: data.PlanCurrentPrice,
          Currency: data.PurchaseCurrency,
          Quantity: 1, // Default to 1 if not provided

          // Dates
          StartUtc: data.CurrentPeriodStartUtc,
          CurrentPeriodStartUtc: data.CurrentPeriodStartUtc,
          CurrentPeriodEndUtc: data.CurrentPeriodEndUtc,
          TrialEndUtc: data.TrialEndUtc,
          PlanStartDate: data.CurrentPeriodStartUtc,
          PlanEndDate: data.CurrentPeriodEndUtc,

          // Status
          BillingPeriod: data.BillingPeriod,
          IsActive: data.PlanIsActive,
          Status: data.Status,
          AutoRenew: data.AutoRenew
        };

        console.log('✓ User subscription loaded from alias API:', this.sub);

        // Now create subscription if planCode is provided
        if (this.planCode) {
          this.createSubscription();
        } else {
          this.loading.set(false);
        }
      },
      error: (err) => {
        console.error('✗ Error fetching subscription by alias:', err);
        this.error.set('Failed to fetch subscription information.');
        this.loading.set(false);
      }
    });
  }

  // ---- API call (port of useEffect) ----

  private createSubscription(): void {
    const body = {
      UserId: Number(this.userId || 0),
      UserAlias: this.userAlias,
      PlanCode: (this.planCode || '').toUpperCase()
    };

    // Basic guard before calling API (same as React version)
    if (!body.UserId || !body.UserAlias || !body.PlanCode) {
      this.error.set('Missing required fields. Please go back and confirm your plan.');
      return;
    }

    this.loading.set(true);
    this.error.set('');
    this.sub = null;

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Accept: 'application/json'
    });

    this.http.post<SubscriptionResponse>(API_CREATE, body, { headers }).subscribe({
      next: (res) => {
        if (res?.ok && res?.subscription) {
          this.sub = res.subscription;
          console.log('✓ Subscription created successfully:', this.sub);
        } else {
          this.error.set('Invalid response from server.');
        }
      },
      error: (err) => {
        const statusText = err?.status ? `HTTP ${err.status} ${err.statusText || ''}` : '';
        const bodyText = err?.error ? (typeof err.error === 'string' ? err.error : '') : '';
        const message = err?.message || 'Request failed.';
        const combined =
          statusText || bodyText
            ? `${statusText}${bodyText ? ` – ${bodyText}` : ''}`
            : message;
        this.error.set(combined || 'Request failed.');
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }

  // ---- Navigation ----

  onNavigate(path: string) {
    this.router.navigate([path]);
  }
}
