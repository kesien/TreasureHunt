import { LocationModel } from './location.model';
import { TeamModel } from './team.model';

export enum EventType {
  Easter = 'Easter',
  Halloween = 'Halloween'
}

export enum EventStatus {
  Creaed = 'Creaed',
  Active = 'Active',
  Finished = 'Finished'
}

export interface EventModel {
  id: number;
  name: string;
  description: string;
  startTime: string;
  endTime: string;
  eventType: EventType;
  status: EventStatus;
  teamTrackingEnabled: boolean;
  locationCount: number;
  teamCount: number;
  locations?: LocationModel[];
  teams?: TeamModel[];
}

export interface CreateEventRequest {
  name: string;
  description: string;
  startTime: Date;
  endTime: Date;
  eventType: EventType;
}

export interface EventStats {
  totalEvents: number;
  activeEvents: number;
  upcomingEvents: number;
  activeTeams: number;
  totalTeams: number;
  completedEvents: number;
}

// Dashboard-specific aggregated stats
export interface DashboardStats {
  totalEvents: number;
  activeEvents: number;
  totalTeams: number;
  totalLocations: number;
  completedTreasureHunts: number;
  averageCompletionRate: number;
  recentActivity?: {
    teamJoins: number;
    locationsVisited: number;
    eventsCreated: number;
  };
}
