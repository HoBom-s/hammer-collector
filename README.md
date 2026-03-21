# Hammer Collector

Hammer 경매 플랫폼의 Data Collector Service.

## Stack

- ASP.NET (.NET 10)
- Kafka (이벤트 발행)

## Features

- 외부 경매 API 주기적 수집
- 데이터 정규화
- Kafka로 auction 서비스에 이벤트 전달

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
dotnet restore
dotnet run --project src/Hammer.Collector
```

## Branch Strategy

- `main` — Production
- `develop` — Development (default)
