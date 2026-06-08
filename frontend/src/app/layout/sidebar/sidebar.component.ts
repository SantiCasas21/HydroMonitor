import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, MatListModule, MatIconModule],
  template: `
    <div class="sidebar-header">
      <mat-icon class="logo-icon">analytics</mat-icon>
      <div class="logo-text">
        <span class="logo-title">HydroMetrics</span>
        <span class="logo-subtitle">v1.0 · .NET 9 + Angular</span>
      </div>
    </div>
    <mat-nav-list class="nav-list">
      <a mat-list-item
         *ngFor="let item of navItems"
         [routerLink]="item.route"
         routerLinkActive="active-link"
         [routerLinkActiveOptions]="{ exact: item.route === '/dashboard' }"
         class="nav-item">
        <mat-icon class="nav-icon">{{ item.icon }}</mat-icon>
        <span class="nav-label">{{ item.label }}</span>
      </a>
    </mat-nav-list>
    <div class="sidebar-footer">
      <div class="tech-stack">
        <span class="tech-badge">.NET 9</span>
        <span class="tech-badge">PostgreSQL</span>
        <span class="tech-badge">MSSQL</span>
        <span class="tech-badge">Ocelot</span>
        <span class="tech-badge">SignalR</span>
      </div>
    </div>
  `,
  styles: [`
    .sidebar-header {
      padding: 20px 16px;
      display: flex;
      align-items: center;
      gap: 12px;
      border-bottom: 1px solid rgba(255,255,255,0.08);
    }
    .logo-icon { color: #60a5fa; font-size: 32px; width: 32px; height: 32px; }
    .logo-text { display: flex; flex-direction: column; }
    .logo-title { color: #f1f5f9; font-weight: 700; font-size: 1.1rem; letter-spacing: -0.02em; }
    .logo-subtitle { color: #94a3b8; font-size: 0.65rem; }
    .nav-list { padding: 12px 8px; }
    .nav-item {
      border-radius: 8px;
      margin-bottom: 2px;
      color: #cbd5e1 !important;
      height: 44px !important;
      transition: all 0.2s ease;
    }
    .nav-item:hover { background: rgba(255,255,255,0.06) !important; color: #f1f5f9 !important; }
    .active-link { background: rgba(59,130,246,0.2) !important; color: #60a5fa !important; }
    .active-link .nav-icon { color: #60a5fa; }
    .nav-icon { margin-right: 12px; color: #64748b; }
    .nav-label { font-size: 0.9rem; font-weight: 500; }
    .sidebar-footer {
      position: absolute;
      bottom: 0;
      width: 100%;
      padding: 16px;
      border-top: 1px solid rgba(255,255,255,0.08);
    }
    .tech-stack {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }
    .tech-badge {
      background: rgba(255,255,255,0.08);
      color: #94a3b8;
      padding: 3px 8px;
      border-radius: 4px;
      font-size: 0.65rem;
      font-weight: 500;
    }
  `]
})
export class SidebarComponent {
  navItems: NavItem[] = [
    { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
    { icon: 'warning', label: 'Alertas', route: '/alerts' },
    { icon: 'sensors', label: 'Sensores', route: '/sensors' },
    { icon: 'history', label: 'Históricos', route: '/history' },
  ];
}
