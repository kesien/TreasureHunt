export interface AdminLoginRequest {
  username: string;
  password: string;
}

export interface AdminLoginResponse {
  token: string;
  expiresAt: string;
}

export interface TeamJoinRequest {
  teamCode: string;
  teamName: string;
}

export interface TeamJoinResponse {
  sessionToken: string;
  teamId: number;
  eventId: number;
  teamName: string;
  eventName: string;
}
