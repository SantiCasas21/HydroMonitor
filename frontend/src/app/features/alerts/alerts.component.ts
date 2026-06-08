import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AlertService } from '../../core/services/alert.service';
import { Alert } from '../../core/models/alert.model';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatTableModule, MatIconModule, MatButtonModule, MatChipsModule, MatSelectModule, MatDialogModule, MatProgressSpinnerModule],
  template: `
    <div class="alerts-page">
      <div class="page-header">
        <h2><mat-icon>warning</mat-icon> Panel de Alertas</h2>
        <p>Monitoreo de umbrales de calidad del agua en tiempo real</p>
      </div>

      <!-- Stats Row -->
      <div class="stats-row" *ngIf="stats">
        <div class="stat-card critical"><span class="stat-num">{{ stats.criticalAlerts }}</span><span class="stat-label">Críticas</span></div>
        <div class="stat-card warning"><span class="stat-num">{{ stats.warningAlerts }}</span><span class="stat-label">Advertencias</span></div>
        <div class="stat-card info"><span class="stat-num">{{ stats.infoAlerts }}</span><span class="stat-label">Información</span></div>
        <div class="stat-card total"><span class="stat-num">{{ stats.totalAlerts }}</span><span class="stat-label">Total 24h</span></div>
      </div>

      <!-- Filters -->
      <div class="filters">
        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Severidad</mat-label>
          <mat-select [(ngModel)]="severityFilter" (selectionChange)="loadAlerts()">
            <mat-option value="">Todas</mat-option>
            <mat-option value="Critical">Crítica</mat-option>
            <mat-option value="Warning">Advertencia</mat-option>
            <mat-option value="Info">Info</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Estado</mat-label>
          <mat-select [(ngModel)]="ackFilter" (selectionChange)="loadAlerts()">
            <mat-option [value]="null">Todos</mat-option>
            <mat-option [value]="false">Activas</mat-option>
            <mat-option [value]="true">Reconocidas</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <!-- Alerts Table -->
      <mat-card class="table-card">
        <mat-card-content>
          <table mat-table [dataSource]="alerts" class="alerts-table">
            <ng-container matColumnDef="severity">
              <th mat-header-cell *matHeaderCellDef>Sev.</th>
              <td mat-cell *matCellDef="let a">
                <span class="sev-badge" [class.sev-critical]="a.severity==='Critical'" [class.sev-warning]="a.severity==='Warning'" [class.sev-info]="a.severity==='Info'">
                  {{ a.severity === 'Critical' ? 'CRIT' : a.severity === 'Warning' ? 'WARN' : 'INFO' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="time">
              <th mat-header-cell *matHeaderCellDef>Hora</th>
              <td mat-cell *matCellDef="let a">{{ a.createdAt | date:'HH:mm:ss' }}</td>
            </ng-container>
            <ng-container matColumnDef="parameter">
              <th mat-header-cell *matHeaderCellDef>Parámetro</th>
              <td mat-cell *matCellDef="let a"><strong>{{ a.parameterName }}</strong></td>
            </ng-container>
            <ng-container matColumnDef="value">
              <th mat-header-cell *matHeaderCellDef>Valor</th>
              <td mat-cell *matCellDef="let a">{{ a.actualValue | number:'1.2-2' }}</td>
            </ng-container>
            <ng-container matColumnDef="message">
              <th mat-header-cell *matHeaderCellDef>Mensaje</th>
              <td mat-cell *matCellDef="let a">{{ a.message }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Acciones</th>
              <td mat-cell *matCellDef="let a">
                <button mat-stroked-button color="primary" size="small" (click)="acknowledge(a)" [disabled]="a.isAcknowledged">
                  <mat-icon>check</mat-icon> {{ a.isAcknowledged ? 'Reconocida' : 'Reconocer' }}
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="alertColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: alertColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .alerts-page { display: flex; flex-direction: column; gap: 20px; }
    .page-header h2 { display: flex; align-items: center; gap: 10px; margin: 0; font-size: 1.4rem; color: #1e293b; }
    .page-header h2 mat-icon { color: #d97706; }
    .page-header p { margin: 4px 0 0 34px; color: #94a3b8; font-size: 0.85rem; }

    .stats-row { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .stat-card {
      display: flex; flex-direction: column; align-items: center; padding: 20px;
      border-radius: 12px; color: white; box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    .stat-card.critical { background: linear-gradient(135deg, #dc2626, #ef4444); }
    .stat-card.warning { background: linear-gradient(135deg, #d97706, #f59e0b); }
    .stat-card.info { background: linear-gradient(135deg, #2563eb, #3b82f6); }
    .stat-card.total { background: linear-gradient(135deg, #475569, #64748b); }
    .stat-num { font-size: 2.2rem; font-weight: 800; line-height: 1; }
    .stat-label { font-size: 0.8rem; opacity: 0.9; margin-top: 4px; text-transform: uppercase; letter-spacing: 0.05em; }

    .filters { display: flex; gap: 16px; }
    .filter-field { width: 180px; }

    .table-card { border-radius: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.06); }
    .alerts-table { width: 100%; font-size: 0.82rem; }
    .sev-badge { padding: 2px 8px; border-radius: 6px; font-size: 0.7rem; font-weight: 700; letter-spacing: 0.05em; }
    .sev-critical { background: #fef2f2; color: #dc2626; }
    .sev-warning { background: #fffbeb; color: #d97706; }
    .sev-info { background: #eff6ff; color: #2563eb; }
  `]
})
export class AlertsComponent implements OnInit {
  alerts: Alert[] = [];
  stats: any = null;
  alertColumns = ['severity', 'time', 'parameter', 'value', 'message', 'actions'];
  severityFilter = '';
  ackFilter: boolean | null = false;

  constructor(private alertService: AlertService) {}

  ngOnInit(): void {
    this.loadAlerts();
    this.alertService.getAlertStats().subscribe(s => this.stats = s);
  }

  loadAlerts(): void {
    this.alertService.getAlerts(1, 50, this.severityFilter || undefined, this.ackFilter ?? undefined).subscribe(res => {
      this.alerts = res.data;
    });
  }

  acknowledge(alert: Alert): void {
    this.alertService.acknowledgeAlert(alert.id, 'Operador').subscribe(() => this.loadAlerts());
  }
}
