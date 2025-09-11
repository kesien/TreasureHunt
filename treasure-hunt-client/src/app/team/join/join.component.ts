import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-join',
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './join.component.html',
  styleUrl: './join.component.scss',
  standalone: true
})
export class JoinComponent implements OnInit {
  joinForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  success: string | null = null;
  teamCode: string | null = null;
  showQrScanner = false;

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
