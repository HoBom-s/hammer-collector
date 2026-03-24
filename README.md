# Hammer Collector

Hammer 경매 플랫폼의 Data Collector Service.
Gateway에서 발행하는 Kafka 이벤트를 수신하여 PostgreSQL에 저장하고, 분석 API를 제공합니다.

## Stack

- ASP.NET (.NET 10)
- Kafka (이벤트 수신)
- PostgreSQL (로그 저장)
- EF Core + Serilog

## Features

- `gateway-request-log` 토픽에서 HTTP 요청 로그 수집
- `service-error-log` 토픽에서 서비스 에러 로그 수집
- 트래픽/에러 분석 API
- 자동 DB 마이그레이션 (앱 시작 시)
- Health check (`/health`)

## Services

| Service | Description |
|-----------------------------------------------------------------|-----------------------|
| [hammer-gateway](https://github.com/HoBom-s/hammer-gateway) | API Gateway |
| [hammer-user](https://github.com/HoBom-s/hammer-user) | User & Auth |
| [hammer-auction](https://github.com/HoBom-s/hammer-auction) | Auction API |
| [hammer-collector](https://github.com/HoBom-s/hammer-collector) | Data Collector |
| [hammer-support](https://github.com/HoBom-s/hammer-support) | Logging, FCM, Support |

## Getting Started

### 환경 설정

`.env.example`을 복사해서 `.env.development`를 만들고 값을 채운다.

```bash
cp .env.example .env.development
```

### 로컬 실행

```bash
dotnet run --project src/Hammer.Collector.Api
```

### Docker 실행

```bash
docker build -t hammer-collector .
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=live \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=bear;Username=...;Password=...;Search Path=hammer" \
  -e Kafka__BootstrapServers="localhost:9092" \
  hammer-collector
```

> `.env` 파일은 로컬 개발용. Docker 배포 시에는 컨테이너 환경변수로 직접 주입한다.

## Branch Strategy

- `main` — Production
- `develop` — Development (default)
