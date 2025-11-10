import {Component, OnDestroy, OnInit} from '@angular/core';
import {firstValueFrom, Subject} from 'rxjs';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {EventService} from '../../services/event';
import {ActivatedRoute, Router} from '@angular/router';
import {CreateEventRequest, EventModel, EventStatus, EventType} from '../../models/event.model';
import {LocationManagerComponent} from '../location-manager/location-manager.component';

@Component({
  selector: 'app-event-form',
  imports: [
    ReactiveFormsModule,
    LocationManagerComponent
  ],
  templateUrl: './event-form.component.html',
  styleUrl: './event-form.component.scss',
  standalone: true
})
export class EventFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  eventForm: FormGroup;
  isEditMode = false;
  isLoading = false;
  isSaving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

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
    this.eventId = parseInt(this.route.snapshot.paramMap.get('id') ?? '');
    this.isEditMode = !!this.eventId;

    if (this.isEditMode) {
      this.loadEvent();
    } else {
      this.setDefaultValues();
    }
  }

  get descriptionLength() {
    const descValue = this.eventForm.get('description')?.value;
    if (descValue) {
      return (descValue as string).length;
    }
    return 0;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      eventType: [EventType.Halloween, [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', Validators.required],
      locations: [[], []]
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
      title: '',
      description: '',
      eventType: EventType.Halloween,
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
      eventType: event.eventType,
      startDate: this.formatDateForInput(new Date(event.startTime)),
      endDate: event.endTime ? this.formatDateForInput(new Date(event.endTime)) : '',
      locations: event.locations || []
    });
  }

  private formatDateForInput(date: Date): string {
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
        const updateRequest: CreateEventRequest = this.buildCreateRequest(formValue);
        const updatedEvent = await firstValueFrom(this.eventService.updateEvent(this.eventId, updateRequest));
        this.eventService.updateEvent(this.eventId, updateRequest).subscribe(resp => {
          this.currentEvent = resp;
          this.successMessage = 'Esemény sikeresen frissítve!';
          setTimeout(() => {
            this.router.navigate(['/admin/events', this.eventId]);
          }, 2000);
        })

      } else {
        const createRequest = this.buildCreateRequest(formValue);
        this.eventService.createEvent(createRequest).subscribe(resp => {
          this.successMessage = 'Esemény sikeresen létrehozva!';
          setTimeout(() => {
            this.router.navigate(['/admin/events', resp.id]);
          }, 2000);
        })
      }

    } catch (error: any) {
      console.error('Failed to save event:', error);
      this.errorMessage = error?.error?.message || 'Hiba történt az esemény mentésekor. Kérlek próbáld újra.';
    } finally {
      this.isSaving = false;
    }
  }

  private buildCreateRequest(formValue: any): CreateEventRequest {
    return {
      name: formValue.title.trim(),
      description: formValue.description?.trim() || undefined,
      startTime: new Date(formValue.startDate),
      endTime: formValue.endDate ? new Date(formValue.endDate) : new Date(),
      eventType: formValue.eventType
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
      this.router.navigate(['/admin/events']);
    } else {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  viewEvent(): void {
    if (this.eventId) {
      this.router.navigate(['/admin/events', this.eventId]);
    }
  }

  protected readonly EventType = EventType;
}
