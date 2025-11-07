import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth';
import {ZXingScannerComponent, ZXingScannerModule} from '@zxing/ngx-scanner';
import {BarcodeFormat} from '@zxing/library';

@Component({
  selector: 'app-join',
  imports: [ReactiveFormsModule, FormsModule, ZXingScannerModule],
  templateUrl: './join.component.html',
  styleUrl: './join.component.scss',
  standalone: true
})
export class JoinComponent implements OnInit, OnDestroy {
  @ViewChild('scanner', { static: false }) scanner!: ZXingScannerComponent;
  joinForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  success: string | null = null;
  teamCode: string | null = null;
  showQrScanner = false;

  scannerEnabled = false;
  hasPermission = false;
  permissionDenied = false;
  availableDevices: MediaDeviceInfo[] = [];
  selectedDevice: MediaDeviceInfo | undefined;
  lastScannedCode: string | null = null;

  // QR Code formats to scan
  allowedFormats = [BarcodeFormat.QR_CODE];

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.joinForm = this.formBuilder.group({
      teamCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      teamName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
    });
  }

  ngOnInit(): void {
    // Check if team code was provided in URL (from QR code scan)
    this.teamCode = this.route.snapshot.paramMap.get('teamCode');
    if (this.teamCode) {
      this.joinForm.patchValue({ teamCode: this.teamCode });
      this.success = 'Team code scanned successfully! Enter your team name to continue.';
    }
  }

  ngOnDestroy() {
    if (this.scannerEnabled) {
      this.toggleScanner();
    }
  }

  toggleScanner(): void {
    if (this.scannerEnabled) {
      this.scannerEnabled = false;
      this.hasPermission = false;
      this.permissionDenied = false;
    } else {
      this.scannerEnabled = true;
      this.error = null;
    }

  }

  onCamerasFound(devices: MediaDeviceInfo[]): void {
    this.availableDevices = devices;
    this.hasPermission = true;

    // Select the back camera by default if available
    const backCamera = devices.find(device =>
      device.label.toLowerCase().includes('back') ||
      device.label.toLowerCase().includes('rear') ||
      device.label.toLowerCase().includes('environment')
    );

    this.selectedDevice = backCamera || devices[0];
  }

  onCamerasNotFound(): void {
    this.hasPermission = false;
    this.availableDevices = [];
    this.error = 'No cameras found on this device.';
  }

  onPermissionResponse(permission: boolean): void {
    this.hasPermission = permission;
    if (!permission) {
      this.permissionDenied = true;
      this.error = 'Camera permission is required to scan QR codes.';
    }
  }

  onDeviceChange(deviceId: MediaDeviceInfo): void {
    this.selectedDevice = deviceId;
  }

  onScanSuccess(result: string): void {
    this.lastScannedCode = result;
    this.error = null;

    // Try to extract team code from QR code
    // Expected format: https://yourapp.com/join/{teamCode} or just the team code
    const teamCode = this.extractTeamCodeFromQR(result);

    if (teamCode) {
      // Auto-fill the form with scanned code
      this.joinForm.patchValue({
        teamCode: teamCode
      });

      // Focus on team name field for user convenience
      const teamNameField = document.getElementById('teamName') as HTMLInputElement;
      if (teamNameField) {
        setTimeout(() => teamNameField.focus(), 100);
      }

      // Stop scanner after successful scan
      this.toggleScanner();
    } else {
      this.error = 'Invalid QR code format. Please scan a valid team QR code.';
    }
  }

  onScanError(error: any): void {
    console.warn('QR scan error:', error);
    // Don't show errors for scan failures - they're expected during scanning
  }

  onScanFailure(error: any): void {
    // This is called when no QR code is detected - normal behavior
    // Don't show error messages for this
  }

  private extractTeamCodeFromQR(qrContent: string): string | null {
    // Handle different QR code formats
    if (qrContent.includes('/join/')) {
      // Extract from URL format: https://yourapp.com/join/{teamCode}
      const match = qrContent.match(/\/join\/([^/?]+)/);
      return match ? match[1] : null;
    } else if (qrContent.includes('teamCode=')) {
      // Extract from parameter format: ?teamCode=ABC123
      const match = qrContent.match(/teamCode=([^&]+)/);
      return match ? match[1] : null;
    } else if (/^[A-Za-z0-9]{3,20}$/.test(qrContent.trim())) {
      // Assume it's a direct team code if it matches expected format
      return qrContent.trim();
    }

    return null;
  }

  onSubmit(): void {
    if (this.joinForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.error = null;
    this.success = null;

    const joinRequest = {
      teamCode: this.joinForm.value.teamCode.toUpperCase(),
      teamName: this.joinForm.value.teamName.trim(),
    };

    this.authService.teamJoin(joinRequest).subscribe({
      next: (response) => {
        console.log('Team join successful', response);
        this.success = `Welcome ${response.teamName}! Joined event: ${response.eventName}`;
        setTimeout(() => {
          this.router.navigate(['/team/dashboard']);
        }, 2000);
      },
      error: (error) => {
        console.error('Team join failed', error);
        this.handleJoinError(error);
        this.isLoading = false;
      },
    });
  }

  toggleQrScanner(): void {
    this.showQrScanner = !this.showQrScanner;
    // TODO: Implement QR scanner functionality
    if (this.showQrScanner) {
      this.error = 'QR scanner coming soon! For now, enter the team code manually.';
    }
  }

  formatTeamCode(event: any): void {
    let value = event.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (value.length > 6) {
      value = value.substring(0, 6);
    }
    this.joinForm.patchValue({ teamCode: value });
  }

  navigateToAdminLogin(): void {
    this.router.navigate(['/admin/login']);
  }

  private handleJoinError(error: any): void {
    if (error.status === 404) {
      this.error = 'Team code not found. Please check the code and try again.';
    } else if (error.status === 400) {
      if (error.error?.message?.includes('already joined')) {
        this.error = 'This team has already been joined. Each team can only be joined once.';
      } else if (error.error?.message?.includes('event not active')) {
        this.error = 'This event is not currently active. Please check with the organizer.';
      } else {
        this.error = error.error?.message || 'Invalid team code or team name.';
      }
    } else if (error.status === 0) {
      this.error = 'Unable to connect to server. Please check your connection.';
    } else {
      this.error = 'Failed to join team. Please try again.';
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.joinForm.controls).forEach((key) => {
      const control = this.joinForm.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  // Helper methods for template
  get teamCodeControl() {
    return this.joinForm.get('teamCode');
  }

  get teamNameControl() {
    return this.joinForm.get('teamName');
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.joinForm.get(controlName);
    return !!(control && control.hasError(errorName) && (control.dirty || control.touched));
  }

  get isFormValid(): boolean {
    return this.joinForm.valid;
  }
}
