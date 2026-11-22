import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { environment } from '../../../environments/environment.prod';
import { BrandLogoComponent } from '../brand-logo/brand-logo.component';


@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterModule, BrandLogoComponent],
  templateUrl: '../footer/footer.component.html',
  styleUrls: ['../footer/footer.component.scss']
})
export class FooterComponent {
  year = new Date().getFullYear();
  versionId = environment.versionId;
  appVersion = environment.appVersion;
}
