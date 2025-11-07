import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { DashboardStats, EventModel, EventStats } from '../../models/event.model';
import { EventService } from '../../services/event';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  standalone: true,
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Component state
  isLoading = false;
  errorMessage: string | null = null;

  // Dashboard data
  allEvents: EventModel[] = [];
  filteredEvents: EventModel[] = [];
  eventStats: EventStats | undefined;

  // Search and filtering
  searchTerm = '';

  // Delete confirmation
  eventToDelete: EventModel | null = null;
  isDeletingEvent = false;

  constructor(
    private eventService: EventService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = null;

    this.eventService.getAllEvents().subscribe({
      next: (events) => {
        console.log(events);
        this.allEvents = events || [];
        this.filteredEvents = [...this.allEvents];
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage =
          err?.error?.message || 'Failed to load dashboard data. Please try again.';
        this.isLoading = false;
      },
    });

    this.eventService.getEventStats().subscribe((stats) => {
      this.eventStats = stats;
    });
  }

  refreshData(): void {
    this.loadDashboardData();
  }

  filterEvents(): void {
    if (!this.searchTerm.trim()) {
      this.filteredEvents = [...this.allEvents];
    } else {
      const term = this.searchTerm.toLowerCase().trim();
      this.filteredEvents = this.allEvents.filter(
        (event) =>
          event.name.toLowerCase().includes(term) ||
          event.description?.toLowerCase().includes(term) ||
          event.status.toLowerCase().includes(term)
      );
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'status-active';
      case 'created':
        return 'status-draft';
      case 'finished':
        return 'status-completed';
      default:
        return 'status-draft';
    }
  }

  getStatusDisplayText(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'Active';
      case 'created':
        return 'Draft';
      case 'finished':
        return 'Completed';
      default:
        return status;
    }
  }

  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      console.log(date);
      return date.toLocaleDateString('hu-HU', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
      });
    } catch {
      return 'Invalid date';
    }
  }

  trackByEventId(index: number, event: EventModel): number {
    return event.id;
  }

  createNewEvent(): void {
    this.router.navigate(['/admin/events/new']);
  }

  viewEvent(eventId: string): void {
    this.router.navigate(['/admin/events', eventId]);
  }

  editEvent(eventId: string): void {
    this.router.navigate(['/admin/events', eventId, 'edit']);
  }

  viewAllEvents(): void {
    this.router.navigate(['/admin/events']);
  }

  confirmDeleteEvent(event: EventModel): void {
    this.eventToDelete = event;
  }

  cancelDelete(): void {
    this.eventToDelete = null;
    this.isDeletingEvent = false;
  }

  async deleteEvent(): Promise<void> {
    if (!this.eventToDelete) return;

    this.isDeletingEvent = true;

    try {
      await this.eventService.deleteEvent(this.eventToDelete.id).toPromise();

      // Remove from local arrays
      this.allEvents = this.allEvents.filter((e) => e.id !== this.eventToDelete!.id);
      this.filterEvents(); // Refresh filtered list

      // Close modal
      this.eventToDelete = null;
    } catch (error: any) {
      console.error('Failed to delete event:', error);
      this.errorMessage = error?.error?.message || 'Failed to delete event. Please try again.';
    } finally {
      this.isDeletingEvent = false;
    }
  }
}
