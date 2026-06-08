import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { SensorReading } from '../models/sensor.model';
import { Alert } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private ws: WebSocket | null = null;
  private readingSubject = new BehaviorSubject<SensorReading | null>(null);
  private alertSubject = new BehaviorSubject<Alert | null>(null);
  private connectionStateSubject = new BehaviorSubject<boolean>(false);
  private reconnectTimer: any = null;
  private url = 'ws://localhost:5000/hubs/waterdata';

  public reading$: Observable<SensorReading | null> = this.readingSubject.asObservable();
  public alertNotification$: Observable<Alert | null> = this.alertSubject.asObservable();
  public isConnected$: Observable<boolean> = this.connectionStateSubject.asObservable();

  async startConnection(): Promise<void> {
    try {
      await this.connect();
    } catch (err) {
      console.error('SignalR connection failed, retrying...', err);
      this.scheduleReconnect();
    }
  }

  private async connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.ws = new WebSocket(this.url);

      this.ws.onopen = () => {
        // Send SignalR handshake protocol
        this.sendMessage({ protocol: 'json', version: 1 });
        // Handshake ends with \x1e (record separator)
        if (this.ws) this.ws.send('\x1e');
      };

      this.ws.onmessage = (event) => {
        const data = event.data as string;

        // Handle SignalR protocol messages (JSON terminated by \x1e)
        const messages = data.split('\x1e').filter((m: string) => m.trim());

        for (const msg of messages) {
          try {
            const parsed = JSON.parse(msg);

            // Check for handshake response
            if (parsed.type === undefined || parsed.type === 6) {
              // Handshake complete
              this.connectionStateSubject.next(true);
              console.log('SignalR handshake complete');
              resolve();
              continue;
            }

            // Handle invocation messages
            if (parsed.type === 1) {
              const target = parsed.target;
              const args = parsed.arguments || [];

              if (target === 'ReceiveReading' && args.length > 0) {
                this.readingSubject.next(args[0] as SensorReading);
              } else if (target === 'ReceiveAlertNotification' && args.length > 0) {
                this.alertSubject.next(args[0] as Alert);
              }
            }
          } catch (e) {
            // Skip non-JSON messages (like ping/pong)
          }
        }
      };

      this.ws.onerror = (err) => {
        console.error('SignalR WebSocket error:', err);
        this.connectionStateSubject.next(false);
        reject(err);
      };

      this.ws.onclose = (event) => {
        console.log('SignalR WebSocket closed:', event.code, event.reason);
        this.connectionStateSubject.next(false);
        if (!event.wasClean) {
          this.scheduleReconnect();
        }
      };
    });
  }

  private sendMessage(message: any): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message) + '\x1e');
    }
  }

  private scheduleReconnect(): void {
    if (this.reconnectTimer) return;
    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      console.log('Attempting SignalR reconnect...');
      this.startConnection();
    }, 5000);
  }

  async stopConnection(): Promise<void> {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
    if (this.ws) {
      this.ws.close(1000, 'Client disconnecting');
      this.ws = null;
    }
    this.connectionStateSubject.next(false);
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
