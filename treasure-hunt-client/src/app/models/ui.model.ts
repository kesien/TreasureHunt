export interface MapConfig {
  center: [number, number];
  zoom: number;
  maxZoom: number;
  minZoom: number;
}

export interface QRCodeConfig {
  width: number;
  height: number;
  colorDark: string;
  colorLight: string;
}

export interface NotificationConfig {
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}
