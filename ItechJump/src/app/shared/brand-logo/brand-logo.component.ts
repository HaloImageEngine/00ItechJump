import { Component, Input } from '@angular/core';

export type BrandLogoVariant = 'small' | 'wide' | 'rocket';

const BRAND_SRC: Record<BrandLogoVariant, string> = {
  small: 'assets/img/ITechJump_SmallLogo.png',
  wide: 'assets/img/ITechJump_WideLogo.png',
  rocket: 'assets/img/ITechJump_Rocket.png',
};

@Component({
  selector: 'app-brand-logo',
  standalone: true,
  template: `
    <img
      class="brand-logo"
      [class.brand-logo--small]="variant === 'small'"
      [class.brand-logo--wide]="variant === 'wide'"
      [class.brand-logo--rocket]="variant === 'rocket'"
      [src]="src"
      [attr.alt]="decorative ? null : alt"
      [attr.aria-hidden]="decorative ? true : null"
      loading="lazy"
      decoding="async"
    />
  `,
  styles: [
    `
      :host {
        display: inline-block;
        line-height: 0;
      }

      img {
        display: block;
        object-fit: contain;
      }
    `,
  ],
})
export class BrandLogoComponent {
  @Input() variant: BrandLogoVariant = 'small';
  @Input() alt = 'ITechJump';
  /** When true, image is visual-only (companion text provides the accessible name). */
  @Input() decorative = false;

  get src(): string {
    return BRAND_SRC[this.variant];
  }
}