import { LocationModel } from './location.model';

export interface TeamModel {
  id: number;
  name: string;
  accessCode: string;
  eventId: number;
  completedLocations: number;
  totalLocations: number;
  completionPercentage: number;
  lastLatitude?: number;
  lastLongitude?: number;
  lastUpdateTime?: Date;
}

export interface CreateTeamRequest {
  name: string;
}

export interface TeamProgress {
  teamId: number;
  teamName: string;
  completedLocations: number;
  totalLocations: number;
  completionPercentage: number;
  currentLocation?: LocationModel;
  visitedLocations: number[];
}
