import { Routes } from '@angular/router';
import { adminGuard } from './guards/admin-guard';
import { JoinComponent } from './team/join/join.component';
import { LoginComponent } from './admin/login/login.component';
import { DashboardComponent } from './admin/dashboard/dashboard.component';
import { TeamDashboardComponent as TeamDashboard } from './team/team-dashboard/team-dashboard.component';
import { EventsComponent } from './admin/events/events.component';
import { EventDetailComponent } from './admin/event-detail/event-detail.component';
import { TeamManagementComponent } from './admin/team-management/team-management.component';
import { LiveMonitorComponent } from './admin/live-monitor/live-monitor.component';
import { teamGuard } from './guards/team-guard';
import { MapComponent } from './team/map/map.component';
import { LocationDetailComponent } from './team/location-detail/location-detail.component';
import { PhotoGalleryComponent } from './team/photo-gallery/photo-gallery.component';

export const routes: Routes = [
  // Root redirect
  {
    path: '',
    redirectTo: '/team/join',
    pathMatch: 'full',
  },

  // Quick join route from QR codes
  {
    path: 'join/:teamCode',
    component: JoinComponent,
    // canActivate: [AuthRedirectGuard],
  },

  // Admin routes (protected)
  {
    path: 'admin',
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'login',
        component: LoginComponent,
        // canActivate: [AuthRedirectGuard],
      },
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [adminGuard],
      },
      {
        path: 'events',
        component: EventsComponent,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id',
        component: EventDetailComponent,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id/teams',
        component: TeamManagementComponent,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id/monitor',
        component: LiveMonitorComponent,
        canActivate: [adminGuard],
      },
    ],
  },

  // Team routes (protected after join)
  {
    path: 'team',
    children: [
      {
        path: '',
        redirectTo: 'join',
        pathMatch: 'full',
      },
      {
        path: 'join',
        component: JoinComponent,
        // canActivate: [AuthRedirectGuard],
      },
      {
        path: 'dashboard',
        component: TeamDashboard,
        canActivate: [teamGuard],
      },
      {
        path: 'map',
        component: MapComponent,
        canActivate: [teamGuard],
      },
      {
        path: 'location/:id',
        component: LocationDetailComponent,
        canActivate: [teamGuard],
      },
      {
        path: 'photos',
        component: PhotoGalleryComponent,
        canActivate: [teamGuard],
      },
    ],
  },

  // Fallback route
  {
    path: '**',
    redirectTo: '/team/join',
  },
];
