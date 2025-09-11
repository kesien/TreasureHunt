import { LocationModel } from './location.model';
import { TeamModel } from './team.model';

export interface EventModel {
  id: number;
  name: string;
  description: string;
  startTime: Date;
  endTime: Date;
  eventType: 'Easter' | 'Halloween';
  status: 'Scheduled' | 'Active' | 'Completed';
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
  eventType: 'Easter' | 'Halloween';
  teamTrackingEnabled: boolean;
}

export interface EventStats {
  totalEvents: number;
  activeEvents: number;
  totalTeams: number;
  completedEvents: number;
}
