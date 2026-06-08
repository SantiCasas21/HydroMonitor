import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WaterDataService } from '../../core/services/water-data.service';
import { Sensor } from '../../core/models/sensor.model';

@Component({
  selector: 'app-sensors',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, MatChipsModule, MatProgressSpinnerModule],
  template: `
    <div class="sensors-page">
      <div class="page-header">
        <h2><mat-icon>sensors</mat-icon> Estaciones de Monitoreo</h2>
        <p>Sensores desplegados en la red de monitoreo de calidad hídrica</p>
      </div>

      <div class="sensors-grid">
        <mat-card class="sensor-card" *ngFor="let sensor of sensors">
          <div class="sensor-header">
            <div class="sensor-icon">
              <mat-icon>water</mat-icon>
            </div>
            <div class="sensor-info">
              <h3>{{ sensor.name }}</h3>
              <div class="location">
                <mat-icon>location_on</mat-icon>
                <span>{{ sensor.location }}</span>
              </div>
            </div>
            <mat-chip [class.active]="sensor.isActive" [class.inactive]="!sensor.isActive">
              {{ sensor.isActive ? 'Activo' : 'Inactivo' }}
            </mat-chip>
          </div>

          <div class="sensor-readings" *ngIf="sensor.latestReading">
            <div class="reading-item">
              <span class="reading-label">pH</span>
              <span class="reading-value" [class.warn]="sensor.latestReading.ph > 8.5">{{ sensor.latestReading.ph | number:'1.2-2' }}</span>
            </div>
            <div class="reading-item">
              <span class="reading-label">Turbiedad</span>
              <span class="reading-value" [class.warn]="sensor.latestReading.turbidity > 5">{{ sensor.latestReading.turbidity | number:'1.1-1' }} NTU</span>
            </div>
            <div class="reading-item">
              <span class="reading-label">O₂ Disuelto</span>
              <span class="reading-value" [class.warn]="sensor.latestReading.dissolvedOxygen < 4">{{ sensor.latestReading.dissolvedOxygen | number:'1.1-1' }} mg/L</span>
            </div>
            <div class="reading-item">
              <span class="reading-label">Temp.</span>
              <span class="reading-value" [class.warn]="sensor.latestReading.temperature > 28">{{ sensor.latestReading.temperature | number:'1.1-1' }} °C</span>
            </div>
            <div class="reading-item">
              <span class="reading-label">Conductividad</span>
              <span class="reading-value">{{ sensor.latestReading.conductivity | number:'1.0-0' }} µS/cm</span>
            </div>
          </div>

          <div class="sensor-meta" *ngIf="sensor.latestReading">
            <mat-icon>update</mat-icon>
            <span>Última lectura: {{ sensor.latestReading.timestamp | date:'medium' }}</span>
          </div>

          <div class="sensor-no-data" *ngIf="!sensor.latestReading">
            <p>Sin datos disponibles</p>
          </div>

          <div class="sensor-coords" *ngIf="sensor.latitude">
            <mat-icon>gps_fixed</mat-icon>
            <span>{{ sensor.latitude }}, {{ sensor.longitude }}</span>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .sensors-page { display: flex; flex-direction: column; gap: 20px; }
    .page-header h2 { display: flex; align-items: center; gap: 10px; margin: 0; font-size: 1.4rem; color: #1e293b; }
    .page-header h2 mat-icon { color: #3b82f6; }
    .page-header p { margin: 4px 0 0 34px; color: #94a3b8; font-size: 0.85rem; }

    .sensors-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(380px, 1fr)); gap: 20px; }
    .sensor-card { padding: 24px; border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); transition: transform 0.2s ease; }
    .sensor-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.1); }

    .sensor-header { display: flex; align-items: flex-start; gap: 16px; margin-bottom: 20px; }
    .sensor-icon { background: #eff6ff; border-radius: 12px; width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; }
    .sensor-icon mat-icon { color: #3b82f6; font-size: 28px; width: 28px; height: 28px; }
    .sensor-info h3 { margin: 0 0 4px; font-size: 1rem; color: #1e293b; }
    .location { display: flex; align-items: center; gap: 4px; color: #94a3b8; font-size: 0.8rem; }
    .location mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .active { background: #f0fdf4 !important; color: #16a34a !important; }
    .inactive { background: #f1f5f9 !important; color: #94a3b8 !important; }

    .sensor-readings { display: grid; grid-template-columns: repeat(5, 1fr); gap: 12px; margin-bottom: 16px; }
    .reading-item { text-align: center; }
    .reading-label { display: block; font-size: 0.65rem; text-transform: uppercase; color: #94a3b8; font-weight: 600; letter-spacing: 0.05em; margin-bottom: 2px; }
    .reading-value { font-size: 0.95rem; font-weight: 700; color: #334155; }
    .reading-value.warn { color: #dc2626; }

    .sensor-meta { display: flex; align-items: center; gap: 6px; color: #94a3b8; font-size: 0.75rem; margin-bottom: 8px; }
    .sensor-meta mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .sensor-coords { display: flex; align-items: center; gap: 6px; color: #64748b; font-size: 0.75rem; }
    .sensor-coords mat-icon { font-size: 14px; width: 14px; height: 14px; color: #ef4444; }
    .sensor-no-data { text-align: center; color: #94a3b8; padding: 20px; }
  `]
})
export class SensorsComponent implements OnInit {
  sensors: Sensor[] = [];

  constructor(private waterDataService: WaterDataService) {}

  ngOnInit(): void {
    this.waterDataService.getSensors().subscribe(res => {
      this.sensors = res.data.filter(s => s.isActive);
    });
  }
}
