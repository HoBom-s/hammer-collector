FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Hammer.Collector.slnx ./
COPY src/Hammer.Collector.Domain/Hammer.Collector.Domain.csproj src/Hammer.Collector.Domain/
COPY src/Hammer.Collector.Application/Hammer.Collector.Application.csproj src/Hammer.Collector.Application/
COPY src/Hammer.Collector.Infrastructure/Hammer.Collector.Infrastructure.csproj src/Hammer.Collector.Infrastructure/
COPY src/Hammer.Collector.Api/Hammer.Collector.Api.csproj src/Hammer.Collector.Api/
RUN dotnet restore src/Hammer.Collector.Api/Hammer.Collector.Api.csproj

COPY src/ src/
RUN dotnet publish src/Hammer.Collector.Api/Hammer.Collector.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --no-create-home appuser

COPY --from=build /app .

USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Hammer.Collector.Api.dll"]
