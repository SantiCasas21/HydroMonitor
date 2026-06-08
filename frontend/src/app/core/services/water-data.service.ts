import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sensor, SensorReading, ParameterDataPoint, ReadingStats, PaginatedResult, CreateSensorDto } from '../models/sensor.model';

@Injectable({ providedIn: 'root' })
export class WaterDataService {
  private baseUrl = 'http://localhost:5000/waterdata/api';

  constructor(private http: HttpClient) {}

  getSensors(page = 1, pageSize = 20): Observable<PaginatedResult<Sensor>> {
    return this.http.get<PaginatedResult<Sensor>>(`${this.baseUrl}/sensors`, { params: { page, pageSize } });
  }

  getSensor(id: string): Observable<Sensor> {
    return this.http.get<Sensor>(`${this.baseUrl}/sensors/${id}`);
  }

  createSensor(dto: CreateSensorDto): Observable<Sensor> {
    return this.http.post<Sensor>(`${this.baseUrl}/sensors`, dto);
  }

  getReadings(sensorId: string, page = 1, pageSize = 50): Observable<PaginatedResult<SensorReading>> {
    return this.http.get<PaginatedResult<SensorReading>>(`${this.baseUrl}/readings`, { params: { sensorId, page, pageSize } });
  }

  getLatestReadings(sensorId?: string): Observable<SensorReading[]> {
    const params: any = {};
    if (sensorId) params.sensorId = sensorId;
    return this.http.get<SensorReading[]>(`${this.baseUrl}/readings/latest`, { params });
  }

  getHistory(sensorId: string, parameter: string, from: string, to: string): Observable<ParameterDataPoint[]> {
    return this.http.get<ParameterDataPoint[]>(`${this.baseUrl}/readings/history`, {
      params: { sensorId, parameter, from, to }
    });
  }

  getStats(sensorId: string, from?: string, to?: string): Observable<ReadingStats> {
    const params: any = { sensorId };
    if (from) params.from = from;
    if (to) params.to = to;
    return this.http.get<ReadingStats>(`${this.baseUrl}/readings/stats`, { params });
  }
}
