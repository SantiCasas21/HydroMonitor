import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WaterDataService } from '../../core/services/water-data.service';
import { Sensor, ParameterDataPoint } from '../../core/models/sensor.model';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatSelectModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatInputModule, MatFormFieldModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div class="history-page">
      <div class="page-header">
        <h2><mat-icon>timeline</mat-icon> Datos Históricos</h2>
        <p>Consulta y análisis de series temporales de parámetros de calidad del agua</p>
      </div>

      <mat-card class="filter-card">
        <mat-card-content>
          <div class="filter-row">
            <mat-form-field appearance="outline">
              <mat-label>Sensor</mat-label>
              <mat-select [(ngModel)]="selectedSensorId" (selectionChange)="loadHistory()">
                <mat-option *ngFor="let s of sensors" [value]="s.id">{{ s.name }}</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Parámetro</mat-label>
              <mat-select [(ngModel)]="selectedParameter" (selectionChange)="loadHistory()">
                <mat-option value="pH">pH</mat-option>
                <mat-option value="Turbidity">Turbiedad (NTU)</mat-option>
                <mat-option value="DissolvedOxygen">Oxígeno Disuelto (mg/L)</mat-option>
                <mat-option value="Temperature">Temperatura (°C)</mat-option>
                <mat-option value="Conductivity">Conductividad (µS/cm)</mat-option>
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Desde</mat-label>
              <input matInput [matDatepicker]="fromPicker" [(ngModel)]="fromDate" (dateChange)="loadHistory()">
              <mat-datepicker-toggle matSuffix [for]="fromPicker"></mat-datepicker-toggle>
              <mat-datepicker #fromPicker></mat-datepicker>
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Hasta</mat-label>
              <input matInput [matDatepicker]="toPicker" [(ngModel)]="toDate" (dateChange)="loadHistory()">
              <mat-datepicker-toggle matSuffix [for]="toPicker"></mat-datepicker-toggle>
              <mat-datepicker #toPicker></mat-datepicker>
            </mat-form-field>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card class="chart-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon [style.color]="paramColor">show_chart</mat-icon>
            {{ selectedParameter }} - Datos Históricos
          </mat-card-title>
          <span class="data-count">{{ dataPoints.length }} registros</span>
        </mat-card-header>
        <mat-card-content>
          <div class="chart-container" *ngIf="dataPoints.length > 0 && !loading">
            <svg class="history-chart" viewBox="0 0 800 350" preserveAspectRatio="xMidYMid meet">
              <defs>
                <linearGradient id="hist-grad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" [attr.stop-color]="paramColor" stop-opacity="0.25"/>
                  <stop offset="100%" [attr.stop-color]="paramColor" stop-opacity="0.02"/>
                </linearGradient>
              </defs>
              <!-- Grid lines -->
              <line *ngFor="let y of yGridLines" x1="60" [attr.y1]="y" x2="780" [attr.y2]="y" stroke="#e2e8f0" stroke-width="0.5"/>
              <!-- Y-axis labels -->
              <text *ngFor="let label of yLabels; let i = index" x="55" [attr.y]="yLabelPositions[i]" text-anchor="end" class="axis-label" font-size="10" fill="#94a3b8">{{ label | number:'1.1-1' }}</text>
              <!-- X-axis labels -->
              <text *ngFor="let label of xLabels; let i = index" [attr.x]="xLabelPositions[i]" y="340" text-anchor="middle" class="axis-label" font-size="9" fill="#94a3b8">{{ label }}</text>
              <!-- Area fill -->
              <path [attr.d]="areaPath" fill="url(#hist-grad)" stroke="none"/>
              <!-- Line -->
              <polyline [attr.points]="linePoints" fill="none" [attr.stroke]="paramColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div class="no-data" *ngIf="dataPoints.length === 0 && !loading">
            <mat-icon>search_off</mat-icon>
            <p>Selecciona un sensor y parámetro para visualizar datos históricos</p>
          </div>
          <div class="loading" *ngIf="loading">
            <mat-spinner diameter="40"></mat-spinner>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .history-page { display: flex; flex-direction: column; gap: 20px; }
    .page-header h2 { display: flex; align-items: center; gap: 10px; margin: 0; font-size: 1.4rem; color: #1e293b; }
    .page-header h2 mat-icon { color: #7c3aed; }
    .page-header p { margin: 4px 0 0 34px; color: #94a3b8; font-size: 0.85rem; }
    .filter-card { border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
    .filter-row { display: flex; gap: 16px; flex-wrap: wrap; }
    .filter-row mat-form-field { flex: 1; min-width: 200px; }
    .chart-card { border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
    .chart-card mat-card-title { display: flex; align-items: center; gap: 8px; }
    .data-count { font-size: 0.75rem; color: #94a3b8; }
    .chart-container { width: 100%; }
    .history-chart { width: 100%; height: auto; max-height: 400px; }
    .axis-label { font-family: 'Inter', sans-serif; }
    .no-data, .loading { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 300px; color: #94a3b8; gap: 12px; }
    .no-data mat-icon { font-size: 48px; width: 48px; height: 48px; }
  `]
})
export class HistoryComponent implements OnInit {
  sensors: Sensor[] = [];
  selectedSensorId: string | null = null;
  selectedParameter = 'pH';
  fromDate: Date = new Date(Date.now() - 24 * 60 * 60 * 1000);
  toDate: Date = new Date();
  dataPoints: ParameterDataPoint[] = [];
  loading = false;

  paramColor = '#3b82f6';
  paramUnit = 'pH';

  // Chart computed properties
  get yGridLines(): number[] { const step = 270 / 6; return Array.from({ length: 7 }, (_, i) => 50 + i * step); }
  get yLabels(): number[] {
    const vals = this.dataPoints.map(d => d.value);
    if (!vals.length) return [0, 0, 0, 0, 0, 0, 0];
    const min = Math.min(...vals), max = Math.max(...vals), range = Math.max(max - min, 0.01);
    return Array.from({ length: 7 }, (_, i) => max - (range / 6) * i);
  }
  get yLabelPositions(): number[] { const step = 270 / 6; return Array.from({ length: 7 }, (_, i) => 52 + i * step); }
  get xLabels(): string[] {
    if (this.dataPoints.length < 2) return ['', '', '', '', '', ''];
    const step = Math.max(1, Math.floor(this.dataPoints.length / 5));
    return Array.from({ length: 6 }, (_, i) => {
      const idx = Math.min(i * step, this.dataPoints.length - 1);
      const d = new Date(this.dataPoints[idx].timestamp);
      return d.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
    });
  }
  get xLabelPositions(): number[] {
    if (this.dataPoints.length < 2) return [60, 204, 348, 492, 636, 780];
    return Array.from({ length: 6 }, (_, i) => 60 + i * 144);
  }

  get linePoints(): string {
    if (!this.dataPoints.length) return '';
    const vals = this.dataPoints.map(d => d.value);
    const min = Math.min(...vals), max = Math.max(...vals), range = Math.max(max - min, 0.01);
    const stepX = 720 / Math.max(this.dataPoints.length - 1, 1);
    return this.dataPoints.map((d, i) => {
      const x = 60 + i * stepX;
      const y = 320 - ((d.value - min) / range) * 270;
      return `${x},${y}`;
    }).join(' ');
  }

  get areaPath(): string {
    if (!this.dataPoints.length) return '';
    const vals = this.dataPoints.map(d => d.value);
    const min = Math.min(...vals), max = Math.max(...vals), range = Math.max(max - min, 0.01);
    const stepX = 720 / Math.max(this.dataPoints.length - 1, 1);
    const line = this.dataPoints.map((d, i) => {
      const x = 60 + i * stepX;
      const y = 320 - ((d.value - min) / range) * 270;
      return `${x},${y}`;
    }).join(' L');
    return `M60,320 L${line} L${60 + (this.dataPoints.length - 1) * stepX},320 Z`;
  }

  constructor(private waterDataService: WaterDataService) {}

  ngOnInit(): void {
    this.waterDataService.getSensors().subscribe(res => {
      this.sensors = res.data.filter(s => s.isActive);
      if (this.sensors.length > 0) {
        this.selectedSensorId = this.sensors[0].id;
        this.loadHistory();
      }
    });
  }

  loadHistory(): void {
    if (!this.selectedSensorId) return;
    this.paramColor = this.getParamColor(this.selectedParameter);
    this.paramUnit = this.getParamUnit(this.selectedParameter);
    this.loading = true;
    const from = this.fromDate.toISOString();
    const to = this.toDate.toISOString();
    this.waterDataService.getHistory(this.selectedSensorId, this.selectedParameter, from, to).subscribe(data => {
      this.dataPoints = data;
      this.loading = false;
    });
  }

  private getParamColor(param: string): string {
    const m: any = { 'pH': '#3b82f6', 'Turbidity': '#d97706', 'DissolvedOxygen': '#16a34a', 'Temperature': '#dc2626', 'Conductivity': '#7c3aed' };
    return m[param] || '#3b82f6';
  }

  private getParamUnit(param: string): string {
    const m: any = { 'pH': 'pH', 'Turbidity': 'NTU', 'DissolvedOxygen': 'mg/L', 'Temperature': '°C', 'Conductivity': 'µS/cm' };
    return m[param] || '';
  }
}
