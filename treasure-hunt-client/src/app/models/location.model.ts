export interface LocationModel {
  id: number;
  eventId: number;
  name: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  order: number;
  isRequired: boolean;
  isCompleted?: boolean; // For team view
  completedAt?: Date; // For team view
}

export interface CreateLocationRequest {
  name: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  order: number;
  isRequired: boolean;
}
