export interface AlertRule {
  id: string;
  parameterName: string;
  minThreshold: number | null;
  maxThreshold: number | null;
  severity: string;
  isActive: boolean;
  description: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface Alert {
  id: string;
  alertRuleId: string | null;
  ruleDescription: string | null;
  sensorId: string;
  readingId: string;
  parameterName: string;
  actualValue: number;
  minThreshold: number | null;
  maxThreshold: number | null;
  message: string;
  severity: string;
  isAcknowledged: boolean;
  acknowledgedAt: string | null;
  acknowledgedBy: string | null;
  createdAt: string;
}

export interface AlertStats {
  totalAlerts: number;
  activeAlerts: number;
  criticalAlerts: number;
  warningAlerts: number;
  infoAlerts: number;
  acknowledgedAlerts: number;
}

export interface CreateAlertRuleDto {
  parameterName: string;
  minThreshold: number | null;
  maxThreshold: number | null;
  severity: string;
  description: string | null;
}
