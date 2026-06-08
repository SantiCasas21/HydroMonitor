export interface SensorReading {
  id: string;
  sensorId: string;
  sensorName: string;
  location: string;
  ph: number;
  turbidity: number;
  dissolvedOxygen: number;
  temperature: number;
  conductivity: number;
  timestamp: string;
}

export interface Sensor {
  id: string;
  name: string;
  location: string;
  latitude: number | null;
  longitude: number | null;
  description: string | null;
  isActive: boolean;
  installedAt: string;
  latestReading: SensorReading | null;
}

export interface CreateSensorDto {
  name: string;
  location: string;
  latitude: number | null;
  longitude: number | null;
  description: string | null;
}

export interface ParameterDataPoint {
  timestamp: string;
  value: number;
}

export interface ReadingStats {
  avgPh: number;
  avgTurbidity: number;
  avgDissolvedOxygen: number;
  avgTemperature: number;
  avgConductivity: number;
  minPh: number;
  maxPh: number;
  minTurbidity: number;
  maxTurbidity: number;
  totalReadings: number;
  from: string;
  to: string;
}

export interface PaginatedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
