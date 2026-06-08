import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Alert } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private toastr: ToastrService) {}

  showSuccess(message: string): void {
    this.toastr.success(message, 'Éxito');
  }

  showError(message: string): void {
    this.toastr.error(message, 'Error');
  }

  showWarning(message: string): void {
    this.toastr.warning(message, 'Advertencia');
  }

  showAlert(alert: Alert): void {
    const title = `🚨 ${alert.severity.toUpperCase()} - ${alert.parameterName}`;
    this.toastr.warning(alert.message, title, {
      timeOut: 10000,
      extendedTimeOut: 5000,
      closeButton: true,
      progressBar: true
    });
  }
}
