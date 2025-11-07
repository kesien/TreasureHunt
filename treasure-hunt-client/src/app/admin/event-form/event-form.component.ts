import {Component, OnDestroy, OnInit} from '@angular/core';
import {Subject} from 'rxjs';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {EventService} from '../../services/event';
import {ActivatedRoute, Router} from '@angular/router';
import {CreateEventRequest, EventModel} from '../../models/event.model';

@Component({
  selector: 'app-event-form',
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './event-form.component.html',
  styleUrl: './event-form.component.scss',
  standalone: true
})
export class EventFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Component state
  eventForm: FormGroup;
  isEditMode = false;
  isLoading = false;
  isSaving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Current event data
  currentEvent: EventModel | null = null;
  eventId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private eventService: EventService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.eventForm = this.createForm();
  }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.eventId = parseInt(this.route.snapshot.paramMap.get('id') ?? '');
    this.isEditMode = !!this.eventId;

    if (this.isEditMode) {
      this.loadEvent();
    } else {
      // Set default values for new event
      this.setDefaultValues();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      status: ['draft', [Validators.required]],
      maxTeams: ['', [Validators.min(1), Validators.max(1000)]],
      startDate: ['', [Validators.required]],
      endDate: [''],
      centerLat: ['', [Validators.required, Validators.min(-90), Validators.max(90)]],
      centerLng: ['', [Validators.required, Validators.min(-180), Validators.max(180)]],
      radius: ['5000', [Validators.min(100), Validators.max(50000)]],
      allowTeamRegistration: [true],
      requirePhotos: [false],
      showLeaderboard: [true],
      hasTimeLimit: [false],
      timeLimitMinutes: ['120', [Validators.min(15), Validators.max(1440)]]
    }, { validators: this.dateRangeValidator });
  }

  private dateRangeValidator(form: FormGroup) {
    const startDate = form.get('startDate')?.value;
    const endDate = form.get('endDate')?.value;

    if (startDate && endDate && new Date(endDate) <= new Date(startDate)) {
      form.get('endDate')?.setErrors({ dateRange: true });
      return { dateRange: true };
    }

    return null;
  }

  private setDefaultValues(): void {
    this.eventForm.patchValue({
      centerLat: 47.0379,
      centerLng: 21.9200,
      startDate: this.formatDateForInput(new Date()),
    });
  }

  private async loadEvent(): Promise<void> {
    if (!this.eventId) return;

    this.isLoading = true;
    this.errorMessage = null;

    try {
      this.eventService.getEvent(this.eventId).subscribe(ev => {
        this.currentEvent = ev;
        if (this.currentEvent) {
          this.populateForm(this.currentEvent);
        }
      })

    } catch (error: any) {
      console.error('Failed to load event:', error);
      this.errorMessage = error?.error?.message || 'Failed to load event data.';
    } finally {
      this.isLoading = false;
    }
  }

  private populateForm(event: EventModel): void {
    this.eventForm.patchValue({
      title: event.name,
      description: event.description || '',
      status: event.status,
      maxTeams: event.teamCount || '',
      startDate: this.formatDateForInput(new Date(event.startTime)),
      endDate: event.endTime ? this.formatDateForInput(new Date(event.endTime)) : '',
      teamTrackingEnabled: event.teamTrackingEnabled ?? true,
    });
  }

  private formatDateForInput(date: Date): string {
    // Format date for datetime-local input
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  async saveEvent(): Promise<void> {
    if (this.eventForm.invalid || this.isSaving) {
      this.markFormGroupTouched(this.eventForm);
      return;
    }

    this.isSaving = true;
    this.errorMessage = null;
    this.successMessage = null;

    try {
      const formValue = this.eventForm.value;

      if (this.isEditMode && this.eventId) {
        // Update existing event
        const updateRequest: CreateEventRequest = this.buildCreateRequest(formValue);
        const updatedEvent = await this.eventService.updateEvent(this.eventId, updateRequest).toPromise();
        this.eventService.updateEvent(this.eventId, updateRequest).subscribe(resp => {
          this.currentEvent = resp;
          this.successMessage = 'Event updated successfully!';
          setTimeout(() => {
            this.router.navigate(['/admin/events', this.eventId]);
          }, 2000);
        })

      } else {
        const createRequest: CreateEventRequest = this.buildCreateRequest(formValue);
        this.eventService.createEvent(createRequest).subscribe(resp => {
          this.successMessage = 'Event created successfully!';
          setTimeout(() => {
            this.router.navigate(['/admin/events', resp.id]);
          }, 2000);
        })
      }

    } catch (error: any) {
      console.error('Failed to save event:', error);
      this.errorMessage = error?.error?.message || 'Failed to save event. Please try again.';
    } finally {
      this.isSaving = false;
    }
  }

  async saveDraft(): Promise<void> {
    // Save as draft regardless of validation status
    const currentStatus = this.eventForm.get('status')?.value;
    this.eventForm.patchValue({ status: 'draft' });

    await this.saveEvent();

    // Restore original status if save failed
    if (this.errorMessage) {
      this.eventForm.patchValue({ status: currentStatus });
    }
  }

  private buildCreateRequest(formValue: any): CreateEventRequest {
    return {
      name: formValue.title.trim(),
      description: formValue.description?.trim() || undefined,
      startTime: new Date(formValue.startDate),
      endTime: formValue.endDate ? new Date(formValue.endDate) : new Date(),
      teamTrackingEnabled: formValue.teamTrackingEnabled,
      eventType: 'Easter'
    };
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  openLocationPicker(): void {
    // TODO: Implement location picker modal with interactive map
    // For now, show current coordinates
    const lat = this.eventForm.get('centerLat')?.value || 47.0379;
    const lng = this.eventForm.get('centerLng')?.value || 21.9200;

    const message = `Current coordinates: ${lat}, ${lng}\n\nLocation picker will be implemented in the next update. You can use tools like Google Maps to find coordinates.`;
    alert(message);
  }

  goBack(): void {
    if (this.isEditMode && this.eventId) {
      this.router.navigate(['/admin/events', this.eventId]);
    } else {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  viewEvent(): void {
    if (this.eventId) {
      this.router.navigate(['/admin/events', this.eventId]);
    }
  }
}
