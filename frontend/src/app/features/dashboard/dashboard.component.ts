import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WaterDataService } from '../../core/services/water-data.service';
import { SignalRService } from '../../core/services/signalr.service';
import { SensorReading } from '../../core/models/sensor.model';
import { Subscription } from 'rxjs';

interface StatusParam {
  name: string; icon: string; value: number; unit: string;
  min: number; max: number; color: string; gradient: string;
  status: 'normal' | 'warning' | 'critical';
}

interface ChartData {
  title: string; icon: string; unit: string; color: string;
  points: number[]; maxVal: number; running: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatSelectModule, MatTableModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  template: `
    <div class="dashboard">
      <!-- Status Cards Row -->
      <div class="status-cards">
        <div class="status-card" *ngFor="let param of statusParams">
          <div class="card-bg" [style.background]="param.gradient"></div>
          <div class="card-content">
            <div class="card-header">
              <mat-icon>{{ param.icon }}</mat-icon>
              <span>{{ param.name }}</span>
            </div>
            <div class="card-value">
              {{ param.value | number:'1.2-2' }}
              <span class="card-unit">{{ param.unit }}</span>
            </div>
            <div class="card-status" [class.warning]="param.status==='warning'" [class.critical]="param.status==='critical'">
              {{ param.status === 'normal' ? '✓ Normal' : param.status === 'warning' ? '⚠ Precaución' : '🔴 Crítico' }}
            </div>
            <div class="card-range">
              <div class="range-bar">
                <div class="range-fill" [style.width.%]="getParamPercent(param)" [style.background]="param.color"></div>
              </div>
              <span>{{ param.min }}-{{ param.max }} {{ param.unit }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Real-time Charts Grid with SVG Sparklines -->
      <div class="charts-grid">
        <mat-card class="chart-card" *ngFor="let chart of charts">
          <mat-card-header>
            <mat-card-title>
              <mat-icon [style.color]="chart.color">{{ chart.icon }}</mat-icon>
              {{ chart.title }}
            </mat-card-title>
            <span class="chart-value" [style.color]="chart.color">
              {{ chart.running | number:'1.2-2' }} {{ chart.unit }}
            </span>
          </mat-card-header>
          <mat-card-content>
            <svg class="sparkline" viewBox="0 0 300 100" preserveAspectRatio="none">
              <defs>
                <linearGradient [attr.id]="'grad-'+chart.title" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" [attr.stop-color]="chart.color" stop-opacity="0.3"/>
                  <stop offset="100%" [attr.stop-color]="chart.color" stop-opacity="0.02"/>
                </linearGradient>
              </defs>
              <!-- Area fill -->
              <path *ngIf="chart.points.length > 1"
                    [attr.d]="getAreaPath(chart.points, chart.maxVal)"
                    [attr.fill]="'url(#grad-'+chart.title+')'" stroke="none"/>
              <!-- Line -->
              <polyline *ngIf="chart.points.length > 1"
                        [attr.points]="getLinePoints(chart.points, chart.maxVal)"
                        fill="none" [attr.stroke]="chart.color" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
              <!-- Last point dot -->
              <circle *ngIf="chart.points.length > 0"
                      [attr.cx]="getLastX(chart.points)" [attr.cy]="getLastY(chart.points, chart.maxVal)"
                      r="4" [attr.fill]="chart.color" stroke="#fff" stroke-width="2"/>
            </svg>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Latest Readings Table -->
      <mat-card class="table-card">
        <mat-card-header>
          <mat-card-title><mat-icon>table_chart</mat-icon> Últimas Lecturas</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="recentReadings" class="readings-table">
            <ng-container matColumnDef="time">
              <th mat-header-cell *matHeaderCellDef>Hora</th>
              <td mat-cell *matCellDef="let r">{{ r.timestamp | date:'HH:mm:ss' }}</td>
            </ng-container>
            <ng-container matColumnDef="sensor">
              <th mat-header-cell *matHeaderCellDef>Sensor</th>
              <td mat-cell *matCellDef="let r">{{ r.sensorName }}</td>
            </ng-container>
            <ng-container matColumnDef="ph">
              <th mat-header-cell *matHeaderCellDef>pH</th>
              <td mat-cell *matCellDef="let r" [class.critical-value]="r.ph > 8.5 || r.ph < 6.5">{{ r.ph | number:'1.1-1' }}</td>
            </ng-container>
            <ng-container matColumnDef="turbidity">
              <th mat-header-cell *matHeaderCellDef>Turbiedad</th>
              <td mat-cell *matCellDef="let r" [class.critical-value]="r.turbidity > 5">{{ r.turbidity | number:'1.1-1' }}</td>
            </ng-container>
            <ng-container matColumnDef="oxygen">
              <th mat-header-cell *matHeaderCellDef>O₂ Disuelto</th>
              <td mat-cell *matCellDef="let r" [class.critical-value]="r.dissolvedOxygen < 4">{{ r.dissolvedOxygen | number:'1.1-1' }}</td>
            </ng-container>
            <ng-container matColumnDef="temp">
              <th mat-header-cell *matHeaderCellDef>Temp °C</th>
              <td mat-cell *matCellDef="let r" [class.critical-value]="r.temperature > 28">{{ r.temperature | number:'1.1-1' }}</td>
            </ng-container>
            <ng-container matColumnDef="conductivity">
              <th mat-header-cell *matHeaderCellDef>Conduct.</th>
              <td mat-cell *matCellDef="let r">{{ r.conductivity | number:'1.0-0' }}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="tableColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: tableColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard { display: flex; flex-direction: column; gap: 20px; }
    .status-cards { display: grid; grid-template-columns: repeat(5, 1fr); gap: 16px; }
    .status-card {
      background: #fff; border-radius: 16px; overflow: hidden; position: relative;
      box-shadow: 0 1px 3px rgba(0,0,0,0.06); transition: transform 0.2s, box-shadow 0.2s;
    }
    .status-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
    .card-bg { height: 4px; }
    .card-content { padding: 16px; }
    .card-header { display: flex; align-items: center; gap: 6px; color: #64748b; font-size: 0.72rem; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 8px; }
    .card-header mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .card-value { font-size: 2rem; font-weight: 800; color: #1e293b; letter-spacing: -0.03em; line-height: 1; }
    .card-unit { font-size: 0.75rem; font-weight: 500; color: #94a3b8; margin-left: 4px; }
    .card-status { margin-top: 6px; font-size: 0.72rem; font-weight: 600; color: #16a34a; }
    .card-status.warning { color: #d97706; }
    .card-status.critical { color: #dc2626; }
    .card-range { margin-top: 10px; }
    .range-bar { height: 4px; background: #f1f5f9; border-radius: 2px; overflow: hidden; }
    .range-fill { height: 100%; border-radius: 2px; transition: width 0.5s ease; }
    .range-bar + span { font-size: 0.6rem; color: #94a3b8; display: block; margin-top: 2px; }

    .charts-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
    .chart-card { border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
    .chart-card mat-card-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 16px 0; }
    .chart-card mat-card-title { display: flex; align-items: center; gap: 8px; font-size: 0.85rem; font-weight: 600; color: #334155; }
    .chart-card mat-card-title mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .chart-value { font-size: 1.5rem; font-weight: 800; line-height: 1; }
    .chart-card mat-card-content { padding: 12px; }
    .sparkline { width: 100%; height: 120px; }

    .table-card { border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
    .table-card mat-card-title { display: flex; align-items: center; gap: 8px; font-size: 0.9rem; font-weight: 600; color: #334155; }
    .readings-table { width: 100%; font-size: 0.78rem; }
    .critical-value { color: #dc2626; font-weight: 700; }

    @media (max-width: 1400px) { .status-cards { grid-template-columns: repeat(3, 1fr); } .charts-grid { grid-template-columns: repeat(2, 1fr); } }
    @media (max-width: 900px) { .status-cards { grid-template-columns: repeat(2, 1fr); } .charts-grid { grid-template-columns: 1fr; } }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  subscriptions = new Subscription();
  recentReadings: SensorReading[] = [];
  tableColumns = ['time', 'sensor', 'ph', 'turbidity', 'oxygen', 'temp', 'conductivity'];

  statusParams: StatusParam[] = [
    { name: 'pH', icon: 'science', value: 0, unit: 'pH', min: 0, max: 14, color: '#3b82f6', gradient: 'linear-gradient(90deg, #3b82f6, #60a5fa)', status: 'normal' },
    { name: 'Turbiedad', icon: 'waves', value: 0, unit: 'NTU', min: 0, max: 100, color: '#d97706', gradient: 'linear-gradient(90deg, #92400e, #d97706)', status: 'normal' },
    { name: 'Oxígeno Dis.', icon: 'air', value: 0, unit: 'mg/L', min: 0, max: 15, color: '#16a34a', gradient: 'linear-gradient(90deg, #15803d, #16a34a)', status: 'normal' },
    { name: 'Temperatura', icon: 'thermostat', value: 0, unit: '°C', min: 0, max: 40, color: '#dc2626', gradient: 'linear-gradient(90deg, #b91c1c, #ef4444)', status: 'normal' },
    { name: 'Conductividad', icon: 'bolt', value: 0, unit: 'µS/cm', min: 0, max: 1500, color: '#7c3aed', gradient: 'linear-gradient(90deg, #6d28d9, #8b5cf6)', status: 'normal' },
  ];

  charts: ChartData[] = [];

  constructor(private waterDataService: WaterDataService, private signalR: SignalRService) {
    const paramDefs = [
      { title: 'pH en Tiempo Real', icon: 'science', unit: 'pH', color: '#3b82f6' },
      { title: 'Turbiedad en Tiempo Real', icon: 'waves', unit: 'NTU', color: '#d97706' },
      { title: 'Oxígeno Disuelto', icon: 'air', unit: 'mg/L', color: '#16a34a' },
      { title: 'Temperatura', icon: 'thermostat', unit: '°C', color: '#dc2626' },
      { title: 'Conductividad', icon: 'bolt', unit: 'µS/cm', color: '#7c3aed' },
    ];
    this.charts = paramDefs.map(p => ({ ...p, points: [], maxVal: 100, running: 0 }));
  }

  ngOnInit(): void {
    this.waterDataService.getLatestReadings().subscribe(readings => {
      if (readings.length > 0) this.updateData(readings);
    });
    this.subscriptions.add(this.signalR.reading$.subscribe(reading => { if (reading) this.addReading(reading); }));
  }

  private updateData(readings: SensorReading[]): void {
    const r = readings[0];
    const vals = [r.ph, r.turbidity, r.dissolvedOxygen, r.temperature, r.conductivity];
    this.statusParams.forEach((p, i) => { p.value = vals[i]; });
    this.updateStatuses();
    this.recentReadings = [r, ...this.recentReadings].slice(0, 20);
  }

  private addReading(reading: SensorReading): void {
    this.updateData([reading]);
    const vals = [reading.ph, reading.turbidity, reading.dissolvedOxygen, reading.temperature, reading.conductivity];
    this.charts.forEach((c, i) => {
      c.running = vals[i];
      c.points = [...c.points, vals[i]].slice(-30);
      c.maxVal = Math.max(c.maxVal, vals[i] * 1.2);
    });
  }

  private updateStatuses(): void {
    this.statusParams.forEach(p => {
      const v = p.value;
      if (p.name === 'pH') p.status = v > 8.5 || v < 6.5 ? 'critical' : (v > 8 || v < 7 ? 'warning' : 'normal');
      else if (p.name === 'Turbiedad') p.status = v > 50 ? 'critical' : (v > 5 ? 'warning' : 'normal');
      else if (p.name === 'Oxígeno Dis.') p.status = v < 4 ? 'critical' : (v < 5 ? 'warning' : 'normal');
      else if (p.name === 'Temperatura') p.status = v > 33 ? 'critical' : (v > 28 ? 'warning' : 'normal');
      else if (p.name === 'Conductividad') p.status = v > 1200 ? 'critical' : (v > 800 ? 'warning' : 'normal');
    });
  }

  getParamPercent(param: StatusParam): number {
    return Math.min(100, Math.max(0, ((param.value - param.min) / (param.max - param.min)) * 100));
  }

  // SVG sparkline helpers
  readonly W = 300; readonly H = 100; readonly PAD = 8;

  getLinePoints(points: number[], maxVal: number): string {
    if (points.length === 0) return '';
    const max = maxVal || 1, stepX = (this.W - this.PAD * 2) / Math.max(points.length - 1, 1);
    return points.map((v, i) => {
      const x = this.PAD + i * stepX;
      const y = this.H - this.PAD - (v / max) * (this.H - this.PAD * 2);
      return `${x},${y}`;
    }).join(' ');
  }

  getAreaPath(points: number[], maxVal: number): string {
    const line = this.getLinePoints(points, maxVal);
    if (!line) return '';
    const firstPt = `${this.PAD},${this.H - this.PAD}`;
    const lastPt = `${this.W - this.PAD},${this.H - this.PAD}`;
    return `M${firstPt} L${line} L${lastPt} Z`;
  }

  getLastX(points: number[]): number {
    if (points.length === 0) return this.PAD;
    return this.PAD + ((points.length - 1) * (this.W - this.PAD * 2)) / Math.max(points.length - 1, 1);
  }

  getLastY(points: number[], maxVal: number): number {
    if (points.length === 0) return this.H - this.PAD;
    const max = maxVal || 1;
    return this.H - this.PAD - (points[points.length - 1] / max) * (this.H - this.PAD * 2);
  }

  ngOnDestroy(): void { this.subscriptions.unsubscribe(); }
}
