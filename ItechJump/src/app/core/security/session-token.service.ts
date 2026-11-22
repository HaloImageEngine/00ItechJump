import { Injectable } from '@angular/core';

const STORAGE_KEY = 'itjSessionToken';

@Injectable({ providedIn: 'root' })
export class SessionTokenService {
  get(): string | null {
    try {
      if (typeof sessionStorage === 'undefined') return null;
      return sessionStorage.getItem(STORAGE_KEY);
    } catch {
      return null;
    }
  }

  set(token: string | null): void {
    try {
      if (typeof sessionStorage === 'undefined') return;
      if (!token) {
        sessionStorage.removeItem(STORAGE_KEY);
        return;
      }
      sessionStorage.setItem(STORAGE_KEY, token);
    } catch {
      /* storage blocked */
    }
  }

  clear(): void {
    this.set(null);
  }
}