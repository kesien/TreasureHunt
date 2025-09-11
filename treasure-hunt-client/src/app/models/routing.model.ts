export interface RouteRequest {
  coordinates: [number, number][];
  transportMode: 'car' | 'bicycle' | 'foot';
  optimize?: boolean;
}

export interface RouteResponse {
  distance: number;
  duration: number;
  geometry: string; // GeoJSON LineString
  waypoints: RouteWaypoint[];
}

export interface RouteWaypoint {
  location: [number, number];
  name?: string;
  hint?: string;
}
