import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalRService } from './core/services/signalr.service';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`
})
export class AppComponent implements OnInit {
  constructor(
    private signalRService: SignalRService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.signalRService.startConnection();

    this.signalRService.alertNotification$.subscribe(alert => {
      if (alert) {
        this.notificationService.showAlert(alert);
      }
    });
  }
}
