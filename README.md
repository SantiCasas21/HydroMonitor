# 🌊 HydroMonitor

![.NET 9](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular 18](https://img.shields.io/badge/Angular%2018-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Ocelot](https://img.shields.io/badge/Ocelot-6A2F8A?style=for-the-badge&logo=.net&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-59B4D9?style=for-the-badge&logo=.net&logoColor=white)

**HydroMonitor** es un sistema full-stack de **monitoreo inteligente de la calidad del agua** desarrollado con **arquitectura de microservicios en .NET 9** y un **frontend en Angular 18**. Este proyecto simula una red de estaciones IoT que recolectan datos de sensores (pH, turbiedad, oxígeno disuelto, temperatura y conductividad) en tiempo real, los transmiten mediante **SignalR WebSocket** y los visualizan en un **dashboard interactivo** con alertas automáticas. Está diseñado específicamente para demostrar competencias en sistemas distribuidos, API Gateway con **Ocelot**, manejo de **bases de datos relacionales duales** (PostgreSQL + SQL Server) y desarrollo de SPAs reactivas.

---

## ✨ Características Principales

* **Simulación IoT en Tiempo Real:** Un servicio hospedado genera lecturas cada 3 segundos para 3 estaciones de monitoreo usando funciones senoidales con ruido realista, incluyendo spikes aleatorios que disparan alertas.
* **Dashboard Interactivo:** 5 tarjetas de estado con indicadores Normal/Precaución/Crítico, gráficos SVG sparkline en tiempo real y tabla de últimas lecturas con colores condicionales.
* **Sistema de Alertas Inteligente:** Evaluación automática de lecturas contra reglas configurables de umbrales (pH, turbiedad, oxígeno, temperatura, conductividad) con severidades Info, Warning y Critical.
* **API Gateway Unificado:** Ocelot centraliza el acceso a los microservicios con ruteo, circuit breaker (Polly) y proxy WebSocket para SignalR.
* **Bases de Datos Duales:** PostgreSQL para datos de sensores (optimizado para series temporales) y SQL Server para gestión de alertas y reglas de negocio.
* **Visualización Histórica:** Consulta de series temporales con gráficos SVG interactivos, selector de parámetro y rango de fechas.
* **Cliente WebSocket Nativo:** Implementación personalizada del protocolo SignalR sobre WebSocket en el frontend, sin dependencias externas.

---

## 📋 Requisitos Previos

| Herramienta | Versión mínima | Verificar con |
|---|---|---|
| .NET SDK | **9.0** | `dotnet --version` |
| Node.js | **18+** | `node --version` |
| Angular CLI | **18.x** | `ng version` |
| Docker Desktop | **4.x** | `docker --version` |
| PostgreSQL | 16 (vía Docker) | — |
| SQL Server | 2022 (vía Docker) | — |

### Instalar Angular CLI (si aún no está)
```bash
npm install -g @angular/cli@18
```

---

## ⚡ Configuración Inicial — Docker

Las bases de datos se ejecutan en contenedores Docker. Asegúrese de tener Docker Desktop corriendo antes de continuar.

```bash
# Levantar PostgreSQL y SQL Server
docker compose up -d postgres sqlserver

# Verificar que ambos estén saludables
docker ps
```

Las migraciones de Entity Framework Core se aplican automáticamente al iniciar cada microservicio.

---

## 🚀 Compilar y Ejecutar

### Backend — Microservicios .NET

Ejecute cada servicio en una terminal separada:

```bash
# Terminal 1 — WaterDataService (Puerto 5001)
dotnet run --project src/WaterDataService

# Terminal 2 — AlertService (Puerto 5002)
dotnet run --project src/AlertService

# Terminal 3 — API Gateway (Puerto 5000)
dotnet run --project src/ApiGateway
```

> Los tres servicios se comunican entre sí. El simulador de sensores arranca automáticamente a los 5 segundos y comienza a emitir datos por SignalR. El AlertService consulta las lecturas cada 5 segundos y evalúa las reglas de alerta.

### Frontend — Angular SPA

```bash
cd frontend
npm install --legacy-peer-deps
ng serve
```

### Acceso Rápido

| Servicio | URL |
|---|---|
| 🌐 **Dashboard** | http://localhost:4200/dashboard |
| 🚨 **Alertas** | http://localhost:4200/alerts |
| 📡 **Sensores** | http://localhost:4200/sensors |
| 📊 **Históricos** | http://localhost:4200/history |
| 📘 **Swagger WaterData** | http://localhost:5001/swagger |
| 📙 **Swagger Alert** | http://localhost:5002/swagger |
| 🚪 **API Gateway** | http://localhost:5000 |

---

## 🧪 Ejecutar Pruebas

```bash
# Todos los tests del solution
dotnet test HydroMonitor.sln

# Solo un proyecto
dotnet test tests/WaterDataService.Tests
```

**Resultado esperado:** 10/10 tests pasan

| Proyecto de Test | Tests | Frameworks |
|---|---|---|
| `WaterDataService.Tests` | 6 | xUnit + Moq + FluentAssertions |
| `AlertService.Tests` | 4 | xUnit + Moq + FluentAssertions |

---

## 🗂️ Estructura del Proyecto

```
HydroMonitor/
├── HydroMonitor.sln
├── docker-compose.yml              ← PostgreSQL + MSSQL + servicios
├── global.json                     ← Pin SDK .NET 9.0
├── README.md
│
├── src/
│   ├── Shared/                     ← Librería compartida
│   │   ├── Constants/
│   │   │   └── WaterQualityParameters.cs   ← Parámetros y rangos seguros
│   │   └── Events/
│   │       └── SensorReadingEvent.cs       ← Contrato de eventos
│   │
│   ├── WaterDataService/           ← Microservicio 1 (PostgreSQL)
│   │   ├── Models/                 ← Sensor, SensorReading + DTOs
│   │   ├── Data/
│   │   │   └── WaterDataDbContext.cs       ← EF Core + PostgreSQL
│   │   ├── Repositories/           ← SensorRepository, ReadingRepository
│   │   ├── Services/               ← SensorService, ReadingService,
│   │   │   │                          SensorSimulatorHostedService
│   │   ├── Hubs/
│   │   │   └── WaterDataHub.cs            ← SignalR Hub
│   │   └── Controllers/            ← SensorsController, ReadingsController
│   │
│   ├── AlertService/               ← Microservicio 2 (MSSQL)
│   │   ├── Models/                 ← AlertRule, Alert + DTOs
│   │   ├── Data/
│   │   │   └── AlertDbContext.cs          ← EF Core + SQL Server
│   │   ├── Repositories/           ← AlertRuleRepository, AlertRepository
│   │   ├── Services/               ← AlertRuleService, AlertEvaluationService,
│   │   │                          │   WaterDataPollingService
│   │   └── Controllers/            ← AlertRulesController, AlertsController
│   │
│   ├── ApiGateway/                 ← Ocelot API Gateway
│   │   ├── ocelot.json             ← Configuración de rutas + Polly
│   │   └── Program.cs
│   │
│   └── frontend/                   ← Angular 18 SPA
│       └── src/app/
│           ├── core/models/        ← Interfaces TypeScript (Sensor, Alert, etc.)
│           ├── core/services/      ← WaterDataService, AlertService, SignalRService
│           ├── layout/             ← Header, Sidebar, MainLayout
│           └── features/           ← Dashboard, Alerts, Sensors, History
│
└── tests/
    ├── WaterDataService.Tests/     ← 6 tests unitarios
    └── AlertService.Tests/         ← 4 tests unitarios
```

---

## 🎨 Paleta del Dashboard

| Color | Hex | Uso |
|---|---|---|
| Azul océano | `#3b82f6` | pH, botones primarios |
| Ámbar alerta | `#d97706` | Turbiedad, warning |
| Verde natural | `#16a34a` | Oxígeno disuelto, estado normal |
| Rojo crítico | `#dc2626` | Temperatura elevada, alertas críticas |
| Violeta | `#7c3aed` | Conductividad, históricos |
| Slate oscuro | `#1e293b` | Textos principales |
| Slate medio | `#64748b` | Textos secundarios, iconos |
| Fondo gris | `#f0f4f8` | Fondo general |

---

## 🛠️ Stack Tecnológico

* **Backend:** .NET 9 Web API con arquitectura de microservicios
* **API Gateway:** Ocelot 23.4 + Polly (Circuit Breaker, Retry)
* **Tiempo Real:** SignalR sobre WebSocket (cliente nativo, sin dependencias npm)
* **Frontend:** Angular 18 con Standalone Components, lazy loading y Angular Material
* **Gráficos:** SVG sparklines y charts personalizados (sin librerías externas de charting)
* **Base de Datos 1:** PostgreSQL 16 (datos de sensores, series temporales)
* **Base de Datos 2:** SQL Server 2022 (alertas y reglas de negocio)
* **ORM:** Entity Framework Core 9 con migraciones automáticas
* **Logging:** Serilog (structured logging en todos los servicios)
* **Contenedores:** Docker + Docker Compose
* **Testing:** xUnit + Moq + FluentAssertions + EF Core InMemory

---

## 🏗️ Decisiones Arquitectónicas

| Decisión | Elección | Justificación |
|---|---|---|
| **Bases de datos separadas** | PostgreSQL + MSSQL | Demuestra dominio de ambos motores; PostgreSQL optimizado para time-series con índices BRIN |
| **Polling vs Message Broker** | Polling HTTP entre servicios | Simplifica el despliegue local sin RabbitMQ/Kafka; en producción se migraría a Azure Service Bus |
| **Sin autenticación** | CORS abierto | Enfoque portfolio; en producción se añadiría JWT + Ocelot auth middleware |
| **Repository Pattern** | Sí | Demuestra Clean Architecture y facilita el testing aislado con Moq |
| **Custom SVG Charts** | En vez de librerías npm | Evita dependencias pesadas, muestra capacidad de construir visualizaciones desde cero |
| **Standalone Components** | Angular 18 moderno | API actual de Angular, lazy loading nativo, mejor tree-shaking |

---

## 📡 API Endpoints

### WaterDataService (vía Gateway: `/waterdata/api/`)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/sensors` | Listar sensores (paginado, filtrable por estado) |
| `GET` | `/sensors/{id}` | Detalle de sensor con su última lectura |
| `POST` | `/sensors` | Registrar nueva estación de monitoreo |
| `PUT` | `/sensors/{id}` | Actualizar datos de estación |
| `DELETE` | `/sensors/{id}` | Desactivar estación (soft delete) |
| `GET` | `/readings` | Lecturas paginadas por sensor |
| `GET` | `/readings/latest` | Última lectura de cada sensor activo |
| `GET` | `/readings/history` | Serie temporal para un parámetro (sensor, from, to) |
| `GET` | `/readings/stats` | Estadísticas agregadas (promedios, mín, máx) |
| `POST` | `/readings` | Ingresar lectura manual |
| `WS` | `/hubs/waterdata` | **SignalR Hub** — streaming en tiempo real |

### AlertService (vía Gateway: `/alert/api/`)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/alert-rules` | Listar reglas de alerta (filtrables) |
| `POST` | `/alert-rules` | Crear nueva regla de umbral |
| `PUT` | `/alert-rules/{id}` | Actualizar regla existente |
| `DELETE` | `/alert-rules/{id}` | Desactivar regla |
| `GET` | `/alerts` | Alertas paginadas (filtro: severidad, estado, parámetro) |
| `GET` | `/alerts/active` | Alertas activas sin reconocer |
| `GET` | `/alerts/stats` | Estadísticas de alertas (últimas 24h) |
| `PUT` | `/alerts/{id}/acknowledge` | Reconocer una alerta |

---

## 👤 Autor

**Santiago Casas** — Desarrollador Backend .NET & Arquitecto de Microservicios

---

🤖 *Portfolio project built for a software engineering role.*
