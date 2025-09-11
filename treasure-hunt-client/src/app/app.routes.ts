import { Routes } from '@angular/router';
import { adminGuard } from './guards/admin-guard';
import { Join } from './team/join/join';
import { Login } from './admin/login/login';
import { Dashboard } from './admin/dashboard/dashboard';
import { Dashboard as TeamDashboard } from './team/dashboard/dashboard';
import { Events } from './admin/events/events';
import { EventDetail } from './admin/event-detail/event-detail';
import { TeamManagement } from './admin/team-management/team-management';
import { LiveMonitor } from './admin/live-monitor/live-monitor';
import { teamGuard } from './guards/team-guard';
import { Map } from './team/map/map';
import { LocationDetail } from './team/location-detail/location-detail';
import { PhotoGallery } from './team/photo-gallery/photo-gallery';

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
    component: Join,
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
        component: Login,
        // canActivate: [AuthRedirectGuard],
      },
      {
        path: 'dashboard',
        component: Dashboard,
        canActivate: [adminGuard],
      },
      {
        path: 'events',
        component: Events,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id',
        component: EventDetail,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id/teams',
        component: TeamManagement,
        canActivate: [adminGuard],
      },
      {
        path: 'events/:id/monitor',
        component: LiveMonitor,
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
        component: Join,
        // canActivate: [AuthRedirectGuard],
      },
      {
        path: 'dashboard',
        component: TeamDashboard,
        canActivate: [teamGuard],
      },
      {
        path: 'map',
        component: Map,
        canActivate: [teamGuard],
      },
      {
        path: 'location/:id',
        component: LocationDetail,
        canActivate: [teamGuard],
      },
      {
        path: 'photos',
        component: PhotoGallery,
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
