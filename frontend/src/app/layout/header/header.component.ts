import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { SignalRService } from '../../core/services/signalr.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule, MatToolbarModule, MatIconModule, MatBadgeModule, MatButtonModule],
  template: `
    <mat-toolbar class="header-toolbar">
      <div class="header-left">
        <mat-icon class="header-icon">water_drop</mat-icon>
        <span class="header-title">WaterQuality Monitor</span>
        <span class="header-subtitle">Sistema de Monitoreo Inteligente</span>
      </div>
      <div class="header-right">
        <div class="status-indicator" [class.connected]="isConnected">
          <mat-icon>{{ isConnected ? 'cloud_done' : 'cloud_off' }}</mat-icon>
          <span>{{ isConnected ? 'Streaming Live' : 'Desconectado' }}</span>
        </div>
      </div>
    </mat-toolbar>
  `,
  styles: [`
    .header-toolbar {
      background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
      border-bottom: 1px solid #e2e8f0;
      box-shadow: 0 1px 3px rgba(0,0,0,0.05);
      height: 64px;
      display: flex;
      justify-content: space-between;
    }
    .header-left { display: flex; align-items: center; gap: 12px; }
    .header-icon {
      color: #3b82f6;
      font-size: 28px;
      width: 28px; height: 28px;
    }
    .header-title {
      font-weight: 700;
      font-size: 1.25rem;
      color: #1e293b;
      letter-spacing: -0.02em;
    }
    .header-subtitle {
      font-size: 0.8rem;
      color: #94a3b8;
      margin-left: 8px;
      padding-left: 12px;
      border-left: 1px solid #e2e8f0;
    }
    .header-right { display: flex; align-items: center; }
    .status-indicator {
      display: flex; align-items: center; gap: 6px;
      padding: 6px 14px;
      border-radius: 20px;
      font-size: 0.8rem;
      font-weight: 500;
      background: #fef2f2; color: #dc2626;
      transition: all 0.3s ease;
    }
    .status-indicator.connected { background: #f0fdf4; color: #16a34a; }
    .status-indicator mat-icon { font-size: 16px; width: 16px; height: 16px; }
  `]
})
export class HeaderComponent implements OnInit {
  isConnected = false;

  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {
    this.signalR.isConnected$.subscribe(c => this.isConnected = c);
  }
}
