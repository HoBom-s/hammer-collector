# Hammer Collector

Hammer 경매 플랫폼의 Log Collector Service.
Gateway에서 발행하는 Kafka 이벤트를 수신하여 PostgreSQL에 저장합니다.

## Stack

- ASP.NET (.NET 10)
- Kafka (이벤트 수신)
- PostgreSQL (로그 저장)
- EF Core + Serilog

## Features

- `gateway-request-log` 토픽에서 HTTP 요청 로그 수집
- `service-error-log` 토픽에서 서비스 에러 로그 수집
- 자동 DB 마이그레이션 (앱 시작 시)
- Health check (`/health`)

## Services

| Service | Description |
|---------|-------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway) | API Gateway |
| [hammer-user](https://github.com/HoBom-s/hammer-user) | User & Auth |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction) | Auction API |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | Data Collector |
| [hammer-support](https://github.com/HoBom-s/hammer-support) | Logging, FCM, Support |

## Getting Started

```bash
cp .env.example .env.development
# .env.development 파일에서 DB/Kafka 접속 정보 수정

dotnet restore
dotnet run --project src/Hammer.Collector.Api
```

## Branch Strategy

- `main` — Production
- `develop` — Development (default)
