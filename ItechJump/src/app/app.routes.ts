import { SearchComponent } from './pages/search/search.component';
import { Routes } from '@angular/router';
import { MainPageComponent } from './pages/main-page/main-page';
import { ReviewComponent } from './pages/review/review.component';
import { ReviewDetailComponent } from './pages/review-detail/review-detail.component';
import { ScoresComponent } from './pages/scores/scores.component';
import { RegisterComponent } from './pages/register/register.component';
import { ContactComponent } from './pages/contact/contact.component';
import { AboutComponent } from './pages/about/about.component';
import { SuccessComponent } from './pages/success/success.component';
import { CancelComponent } from './pages/cancel/cancel.component';
import { LoginComponent } from './pages/login/login.component';
import { LogoffComponent } from './pages/logoff/logoff.component';
import { PcTestComponent } from './pages/pc-test/pc-test.component';
import { VoicetrainCompoenent } from './pages/voicetrain/voicetrain.component';
import { YourQuestionComponent } from './pages/your-question/your-question.component';
import { authGuard } from './core/guards/auth.guard';





export const routes: Routes = [
  { path: '', redirectTo: 'main-page', pathMatch: 'full' }, // default route

  { path: 'main-page', component: MainPageComponent },
  { path: 'search', component: SearchComponent },
  { path: 'review', component: ReviewComponent, canActivate: [authGuard] },
  { path: 'review-detail/:testid', component: ReviewDetailComponent, canActivate: [authGuard] },
  { path: 'scores', component: ScoresComponent, canActivate: [authGuard] },
  { path: 'register', component: RegisterComponent },
  {
    path: 'subscribe1',
    loadComponent: () =>
      import('./pages/subscribe1/subscribe1.component').then((m) => m.Subscribe1Component),
    canActivate: [authGuard]
  },
  {
    path: 'subscribe2',
    loadComponent: () =>
      import('./pages/subscribe2/subscribe2.component').then((m) => m.Subscribe2Component),
    canActivate: [authGuard]
  },
  {
    path: 'subscribe3',
    loadComponent: () =>
      import('./pages/subscribe3/subscribe3.component').then((m) => m.Subscribe3Component),
    canActivate: [authGuard]
  },
  { path: 'success', component: SuccessComponent },
  { path: 'cancel', component: CancelComponent },
  { path: 'login', component: LoginComponent },
  { path: 'logoff', component: LogoffComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },
  { path: 'pc-test', component: PcTestComponent, canActivate: [authGuard] },
  { path: 'voicetrain', component: VoicetrainCompoenent, canActivate: [authGuard] },
  { path: 'your-question', component: YourQuestionComponent, canActivate: [authGuard] },

  { path: '**', redirectTo: 'main-page' } // wildcard fallback
];
