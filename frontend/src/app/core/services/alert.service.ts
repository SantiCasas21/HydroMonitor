import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AlertRule, Alert, AlertStats, CreateAlertRuleDto } from '../models/alert.model';
import { PaginatedResult } from '../models/sensor.model';

@Injectable({ providedIn: 'root' })
export class AlertService {
  private baseUrl = 'http://localhost:5000/alert/api';

  constructor(private http: HttpClient) {}

  getRules(isActive?: boolean, parameter?: string): Observable<AlertRule[]> {
    const params: any = {};
    if (isActive !== undefined) params.isActive = isActive;
    if (parameter) params.parameter = parameter;
    return this.http.get<AlertRule[]>(`${this.baseUrl}/alert-rules`, { params });
  }

  createRule(dto: CreateAlertRuleDto): Observable<AlertRule> {
    return this.http.post<AlertRule>(`${this.baseUrl}/alert-rules`, dto);
  }

  updateRule(id: string, dto: Partial<CreateAlertRuleDto>): Observable<AlertRule> {
    return this.http.put<AlertRule>(`${this.baseUrl}/alert-rules/${id}`, dto);
  }

  deleteRule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/alert-rules/${id}`);
  }

  getAlerts(page = 1, pageSize = 20, severity?: string, isAcknowledged?: boolean, parameter?: string): Observable<PaginatedResult<Alert>> {
    const params: any = { page, pageSize };
    if (severity) params.severity = severity;
    if (isAcknowledged !== undefined) params.isAcknowledged = isAcknowledged;
    if (parameter) params.parameter = parameter;
    return this.http.get<PaginatedResult<Alert>>(`${this.baseUrl}/alerts`, { params });
  }

  getActiveAlerts(severity?: string): Observable<Alert[]> {
    const params: any = {};
    if (severity) params.severity = severity;
    return this.http.get<Alert[]>(`${this.baseUrl}/alerts/active`, { params });
  }

  getAlertStats(from?: string, to?: string): Observable<AlertStats> {
    const params: any = {};
    if (from) params.from = from;
    if (to) params.to = to;
    return this.http.get<AlertStats>(`${this.baseUrl}/alerts/stats`, { params });
  }

  acknowledgeAlert(id: string, acknowledgedBy: string): Observable<Alert> {
    return this.http.put<Alert>(`${this.baseUrl}/alerts/${id}/acknowledge`, { acknowledgedBy });
  }
}
