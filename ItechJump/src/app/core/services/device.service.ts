import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs';

/**
 * Single source of truth for Mobile vs Desktop layout.
 *
 * Keep MOBILE_MAX_WIDTH in sync with `$mobile-max` in src/styles/_breakpoints.scss
 * so TS-driven and CSS-driven switches flip at the same width.
 */
export const MOBILE_MAX_WIDTH = 900;
export const MOBILE_QUERY = `(max-width: ${MOBILE_MAX_WIDTH}px)`;

export type DeviceMode = 'mobile' | 'desktop';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly document = inject(DOCUMENT);
  private readonly breakpoints = inject(BreakpointObserver);

  /** true when the viewport is phone / small-tablet sized. */
  readonly isMobile = toSignal(
    this.breakpoints.observe(MOBILE_QUERY).pipe(map((r) => r.matches)),
    { initialValue: this.breakpoints.isMatched(MOBILE_QUERY) }
  );

  readonly isDesktop = computed(() => !this.isMobile());
  readonly mode = computed<DeviceMode>(() => (this.isMobile() ? 'mobile' : 'desktop'));

  constructor() {
    // Expose the mode as a class on <html> (is-mobile / is-desktop) for global CSS hooks.
    effect(() => {
      const root = this.document.documentElement;
      const mobile = this.isMobile();
      root.classList.toggle('is-mobile', mobile);
      root.classList.toggle('is-desktop', !mobile);
    });
  }
}
