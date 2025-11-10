// location-manager.component.ts
import { Component, forwardRef, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  ControlValueAccessor,
  NG_VALUE_ACCESSOR,
  FormsModule
} from '@angular/forms';
import * as L from 'leaflet';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface NominatimResult {
  place_id: number;
  lat: string;
  lon: string;
  display_name: string;
  address?: any;
}

export interface LocationData {
  id?: number;
  name: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  order: number;
  isRequired: boolean;
}

@Component({
  selector: 'app-location-manager',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './location-manager.component.html',
  styleUrls: ['./location-manager.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => LocationManagerComponent),
      multi: true
    }
  ]
})
export class LocationManagerComponent implements OnInit, OnDestroy, AfterViewInit, ControlValueAccessor {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  locations: LocationData[] = [];
  searchQuery = '';
  searchResults: NominatimResult[] = [];
  isSearching = false;
  showResults = false;
  showLocationForm = false;

  locationForm!: FormGroup;
  editingIndex: number | null = null;

  private map?: L.Map;
  private markers: L.Marker[] = [];

  // ControlValueAccessor
  private onChange: any = () => {};
  private onTouched: any = () => {};

  constructor(private fb: FormBuilder) {
    this.createLocationForm();
  }

  ngOnInit(): void {
    // Setup search with debounce
    this.searchSubject$
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(query => {
        this.searchAddress(query);
      });
  }

  ngAfterViewInit(): void {
    this.initializeMap();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.map) {
      this.map.remove();
    }
  }

  // ControlValueAccessor implementation
  writeValue(value: LocationData[]): void {
    if (value) {
      this.locations = value;
      this.updateMarkers();
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  private createLocationForm(): void {
    this.locationForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(500)]],
      address: ['', [Validators.required, Validators.maxLength(300)]],
      latitude: [0, [Validators.required]],
      longitude: [0, [Validators.required]],
      isRequired: [true]
    });
  }

  private initializeMap(): void {
    // Fix for default marker icons
    const iconRetinaUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png';
    const iconUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png';
    const shadowUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png';

    const DefaultIcon = L.icon({
      iconRetinaUrl,
      iconUrl,
      shadowUrl,
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41]
    });

    L.Marker.prototype.options.icon = DefaultIcon;

    // Initialize map centered on Szeged
    this.map = L.map('location-map').setView([46.2530, 20.1414], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    setTimeout(() => {
      this.map?.invalidateSize();
    }, 100);
  }

  onSearchChange(event: Event): void {
    const query = (event.target as HTMLInputElement).value;
    this.searchQuery = query;

    if (query.length >= 3) {
      this.searchSubject$.next(query);
    } else {
      this.searchResults = [];
      this.showResults = false;
    }
  }

  private async searchAddress(query: string): Promise<void> {
    if (!query || query.length < 3) {
      this.searchResults = [];
      return;
    }

    this.isSearching = true;
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(query)}&limit=10&countrycodes=hu&addressdetails=1`,
        {
          headers: {
            'User-Agent': 'TreasureHuntApp/1.0'
          }
        }
      );

      const data: NominatimResult[] = await response.json();

      // Filter and sort results to prioritize exact address matches
      const sortedResults = data
        .filter(result => {
          // Keep results that have street-level or better detail
          const type = result.address?.road || result.address?.building || result.address?.house_number;
          return type || result.display_name.toLowerCase().includes(query.toLowerCase());
        })
        .sort((a, b) => {
          // Prioritize results with house numbers
          const aHasNumber = !!a.address?.house_number;
          const bHasNumber = !!b.address?.house_number;
          if (aHasNumber && !bHasNumber) return -1;
          if (!aHasNumber && bHasNumber) return 1;
          return 0;
        })
        .slice(0, 5);

      this.searchResults = sortedResults.length > 0 ? sortedResults : data.slice(0, 5);
      this.showResults = true;
    } catch (error) {
      console.error('Search error:', error);
      this.searchResults = [];
    } finally {
      this.isSearching = false;
    }
  }

  selectSearchResult(result: NominatimResult): void {
    this.locationForm.patchValue({
      address: result.display_name,
      latitude: parseFloat(result.lat),
      longitude: parseFloat(result.lon)
    });

    this.showLocationForm = true;
    this.showResults = false;
    this.searchQuery = '';

    // Pan map to selected location
    if (this.map) {
      this.map.setView([parseFloat(result.lat), parseFloat(result.lon)], 15);
    }
  }

  saveLocation(): void {
    if (this.locationForm.invalid) {
      Object.keys(this.locationForm.controls).forEach(key => {
        this.locationForm.get(key)?.markAsTouched();
      });
      return;
    }

    const locationData: LocationData = this.locationForm.value;

    if (this.editingIndex !== null) {
      // Update existing
      this.locations[this.editingIndex] = {
        ...locationData,
        id: this.locations[this.editingIndex].id,
        order: this.editingIndex
      };
    } else {
      // Add new
      this.locations.push({
        ...locationData,
        id: Date.now(),
        order: this.locations.length
      });
    }

    this.updateMarkers();
    this.onChange(this.locations);
    this.cancelLocationForm();
  }

  editLocation(index: number): void {
    this.editingIndex = index;
    const location = this.locations[index];
    this.locationForm.patchValue(location);
    this.showLocationForm = true;

    // Pan map to location
    if (this.map) {
      this.map.setView([location.latitude, location.longitude], 15);
    }
  }

  deleteLocation(index: number): void {
    if (confirm('Biztosan törölni szeretnéd ezt a lokációt?')) {
      this.locations.splice(index, 1);
      // Reindex orders
      this.locations.forEach((loc, i) => loc.order = i);
      this.updateMarkers();
      this.onChange(this.locations);
    }
  }

  cancelLocationForm(): void {
    this.showLocationForm = false;
    this.editingIndex = null;
    this.locationForm.reset({
      isRequired: true
    });
  }

  private updateMarkers(): void {
    if (!this.map) return;

    // Clear existing markers
    this.markers.forEach(marker => marker.remove());
    this.markers = [];

    if (this.locations.length === 0) return;

    // Add new markers
    this.locations.forEach((location, index) => {
      const customIcon = L.divIcon({
        className: 'custom-marker',
        html: `<div style="background-color: #059669; color: white; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; border: 3px solid white; box-shadow: 0 2px 8px rgba(0,0,0,0.3);">${index + 1}</div>`,
        iconSize: [32, 32],
        iconAnchor: [16, 16]
      });

      const marker = L.marker([location.latitude, location.longitude], { icon: customIcon })
        .addTo(this.map!)
        .bindPopup(`<b>${location.name}</b><br>${location.address}`);

      this.markers.push(marker);
    });

    // Fit bounds to show all markers
    if (this.locations.length > 0) {
      const bounds = L.latLngBounds(this.locations.map(loc => [loc.latitude, loc.longitude]));
      this.map.fitBounds(bounds, { padding: [50, 50] });
    }
  }

  closeSearchResults(): void {
    this.showResults = false;
  }
}
