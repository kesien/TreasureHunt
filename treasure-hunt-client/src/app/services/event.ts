import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { EventStats, CreateEventRequest, EventModel } from '../models/event.model';
import { CreateLocationRequest, LocationModel } from '../models/location.model';
import { CreateTeamRequest, TeamModel, TeamProgress } from '../models/team.model';
import { AuthService } from './auth';
import { Team } from './team';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  private apiUrl = environment.apiUrl || 'http://localhost:5000';

  // Reactive state management
  private eventsSubject = new BehaviorSubject<EventModel[]>([]);
  private currentEventSubject = new BehaviorSubject<EventModel | null>(null);
  private eventStatsSubject = new BehaviorSubject<EventStats | null>(null);

  public events$ = this.eventsSubject.asObservable();
  public currentEvent$ = this.currentEventSubject.asObservable();
  public eventStats$ = this.eventStatsSubject.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {}

  // Event CRUD operations
  getAllEvents(): Observable<EventModel[]> {
    const headers = this.authService.getAdminHeaders();

    return this.http.get<EventModel[]>(`${this.apiUrl}/api/events`, { headers }).pipe(
      tap((events) => this.eventsSubject.next(events)),
      catchError(this.handleError)
    );
  }

  getEvent(id: number): Observable<EventModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http.get<EventModel>(`${this.apiUrl}/api/events/${id}`, { headers }).pipe(
      tap((event) => this.currentEventSubject.next(event)),
      catchError(this.handleError)
    );
  }

  createEvent(eventData: CreateEventRequest): Observable<EventModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http.post<EventModel>(`${this.apiUrl}/api/events`, eventData, { headers }).pipe(
      tap((newEvent) => {
        const currentEvents = this.eventsSubject.value;
        this.eventsSubject.next([...currentEvents, newEvent]);
      }),
      catchError(this.handleError)
    );
  }

  updateEvent(id: number, eventData: Partial<CreateEventRequest>): Observable<EventModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .put<EventModel>(`${this.apiUrl}/api/events/${id}`, eventData, { headers })
      .pipe(
        tap((updatedEvent) => {
          const currentEvents = this.eventsSubject.value;
          const index = currentEvents.findIndex((e) => e.id === id);
          if (index !== -1) {
            currentEvents[index] = updatedEvent;
            this.eventsSubject.next([...currentEvents]);
          }
          if (this.currentEventSubject.value?.id === id) {
            this.currentEventSubject.next(updatedEvent);
          }
        }),
        catchError(this.handleError)
      );
  }

  deleteEvent(id: number): Observable<void> {
    const headers = this.authService.getAdminHeaders();

    return this.http.delete<void>(`${this.apiUrl}/api/events/${id}`, { headers }).pipe(
      tap(() => {
        const currentEvents = this.eventsSubject.value;
        const filteredEvents = currentEvents.filter((e) => e.id !== id);
        this.eventsSubject.next(filteredEvents);
        if (this.currentEventSubject.value?.id === id) {
          this.currentEventSubject.next(null);
        }
      }),
      catchError(this.handleError)
    );
  }

  startEvent(id: number): Observable<EventModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .post<EventModel>(`${this.apiUrl}/api/events/${id}/start`, {}, { headers })
      .pipe(
        tap((updatedEvent) => {
          this.updateEventInState(updatedEvent);
        }),
        catchError(this.handleError)
      );
  }

  endEvent(id: number): Observable<EventModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http.post<EventModel>(`${this.apiUrl}/api/events/${id}/end`, {}, { headers }).pipe(
      tap((updatedEvent) => {
        this.updateEventInState(updatedEvent);
      }),
      catchError(this.handleError)
    );
  }

  getEventStats(): Observable<EventStats> {
    const headers = this.authService.getAdminHeaders();

    return this.http.get<EventStats>(`${this.apiUrl}/api/events/stats`, { headers }).pipe(
      tap((stats) => this.eventStatsSubject.next(stats)),
      catchError(this.handleError)
    );
  }

  getEventTeams(eventId: number): Observable<TeamModel[]> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .get<TeamModel[]>(`${this.apiUrl}/api/events/${eventId}/teams`, { headers })
      .pipe(catchError(this.handleError));
  }

  createTeam(eventId: number, teamData: CreateTeamRequest): Observable<TeamModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .post<TeamModel>(`${this.apiUrl}/api/events/${eventId}/teams`, teamData, { headers })
      .pipe(
        tap((newTeam) => {
          const currentEvent = this.currentEventSubject.value;
          if (currentEvent && currentEvent.id === eventId) {
            const updatedEvent = {
              ...currentEvent,
              teams: [...(currentEvent.teams || []), newTeam],
              teamCount: (currentEvent.teamCount || 0) + 1,
            };
            this.currentEventSubject.next(updatedEvent);
          }
        }),
        catchError(this.handleError)
      );
  }

  deleteTeam(teamId: number): Observable<void> {
    const headers = this.authService.getAdminHeaders();

    return this.http.delete<void>(`${this.apiUrl}/api/events/teams/${teamId}`, { headers }).pipe(
      tap(() => {
        const currentEvent = this.currentEventSubject.value;
        if (currentEvent && currentEvent.teams) {
          const updatedEvent = {
            ...currentEvent,
            teams: currentEvent.teams.filter((t) => t.id !== teamId),
            teamCount: Math.max((currentEvent.teamCount || 0) - 1, 0),
          };
          this.currentEventSubject.next(updatedEvent);
        }
      }),
      catchError(this.handleError)
    );
  }

  getTeamProgress(eventId: number): Observable<TeamProgress[]> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .get<TeamProgress[]>(`${this.apiUrl}/api/events/${eventId}/teams/progress`, { headers })
      .pipe(catchError(this.handleError));
  }

  getEventLocations(eventId: number): Observable<LocationModel[]> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .get<LocationModel[]>(`${this.apiUrl}/api/events/${eventId}/locations`, { headers })
      .pipe(catchError(this.handleError));
  }

  createLocation(eventId: number, locationData: CreateLocationRequest): Observable<LocationModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .post<LocationModel>(`${this.apiUrl}/api/events/${eventId}/locations`, locationData, {
        headers,
      })
      .pipe(
        tap((newLocation) => {
          // Update current event with new location if loaded
          const currentEvent = this.currentEventSubject.value;
          if (currentEvent && currentEvent.id === eventId) {
            const updatedEvent = {
              ...currentEvent,
              locations: [...(currentEvent.locations || []), newLocation],
              locationCount: (currentEvent.locationCount || 0) + 1,
            };
            this.currentEventSubject.next(updatedEvent);
          }
        }),
        catchError(this.handleError)
      );
  }

  updateLocation(
    locationId: number,
    locationData: Partial<CreateLocationRequest>
  ): Observable<LocationModel> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .put<LocationModel>(`${this.apiUrl}/api/events/locations/${locationId}`, locationData, {
        headers,
      })
      .pipe(
        tap((updatedLocation) => {
          // Update current event with updated location if loaded
          const currentEvent = this.currentEventSubject.value;
          if (currentEvent && currentEvent.locations) {
            const index = currentEvent.locations.findIndex((l) => l.id === locationId);
            if (index !== -1) {
              const updatedLocations = [...currentEvent.locations];
              updatedLocations[index] = updatedLocation;
              const updatedEvent = {
                ...currentEvent,
                locations: updatedLocations,
              };
              this.currentEventSubject.next(updatedEvent);
            }
          }
        }),
        catchError(this.handleError)
      );
  }

  deleteLocation(locationId: number): Observable<void> {
    const headers = this.authService.getAdminHeaders();

    return this.http
      .delete<void>(`${this.apiUrl}/api/events/locations/${locationId}`, { headers })
      .pipe(
        tap(() => {
          // Update current event by removing location if loaded
          const currentEvent = this.currentEventSubject.value;
          if (currentEvent && currentEvent.locations) {
            const updatedEvent = {
              ...currentEvent,
              locations: currentEvent.locations.filter((l) => l.id !== locationId),
              locationCount: Math.max((currentEvent.locationCount || 0) - 1, 0),
            };
            this.currentEventSubject.next(updatedEvent);
          }
        }),
        catchError(this.handleError)
      );
  }

  // Utility methods
  private updateEventInState(updatedEvent: EventModel): void {
    // Update in events list
    const currentEvents = this.eventsSubject.value;
    const index = currentEvents.findIndex((e) => e.id === updatedEvent.id);
    if (index !== -1) {
      currentEvents[index] = updatedEvent;
      this.eventsSubject.next([...currentEvents]);
    }

    // Update current event if it's the same
    if (this.currentEventSubject.value?.id === updatedEvent.id) {
      this.currentEventSubject.next(updatedEvent);
    }
  }

  // Clear state (useful for logout)
  clearState(): void {
    this.eventsSubject.next([]);
    this.currentEventSubject.next(null);
    this.eventStatsSubject.next(null);
  }

  // Error handling
  private handleError = (error: any) => {
    console.error('Event Service Error:', error);
    return throwError(() => error);
  };
}
