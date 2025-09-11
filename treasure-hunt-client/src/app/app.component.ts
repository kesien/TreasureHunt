import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import {NavigationEnd, Router, RouterLink, RouterOutlet} from '@angular/router';
import { Subject, filter, takeUntil } from 'rxjs';
import { AuthState, AuthService } from './services/auth';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LucideAngularModule, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  standalone: true
})
export class AppComponent {
  title = 'TreasureHunt';
  authState: AuthState | null = null;
  currentRoute = '';
  showNavigation = false;
  mobileMenuOpen = false;

  private destroy$ = new Subject<void>();

  constructor(private authService: AuthService, private router: Router) {
    // Track route changes to determine if navigation should be shown
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.url;
        this.updateNavigationVisibility();
        this.mobileMenuOpen = false;
      });
  }

  ngOnInit() {
    this.authService.authState$.pipe(takeUntil(this.destroy$)).subscribe((authState) => {
      this.authState = authState;
      this.updateNavigationVisibility();
    });

    setInterval(() => {
      this.authService.checkTokenExpiry();
    }, 60000);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateNavigationVisibility() {
    const hideNavRoutes = ['/admin/login', '/team/join'];
    const isHideRoute = hideNavRoutes.some((route) => this.currentRoute.startsWith(route));
    this.showNavigation = (this.authState?.isAuthenticated && !isHideRoute)!;
  }

  logout() {
    if (this.authState?.userType === 'admin') {
      this.authService.adminLogout();
    } else if (this.authState?.userType === 'team') {
      this.authService.teamLogout();
    }
  }

  navigateToHome() {
    if (this.authState?.userType === 'admin') {
      this.router.navigate(['/admin/dashboard']);
    } else if (this.authState?.userType === 'team') {
      this.router.navigate(['/team/dashboard']);
    }
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  get isAdmin(): boolean {
    return this.authState?.userType === 'admin';
  }

  get isTeam(): boolean {
    return this.authState?.userType === 'team';
  }

  get teamName(): string {
    return this.authState?.teamInfo?.teamName || '';
  }

  get eventName(): string {
    return this.authState?.teamInfo?.eventName || '';
  }

  isRouteActive(route: string): boolean {
    return this.currentRoute.startsWith(route);
  }
}
