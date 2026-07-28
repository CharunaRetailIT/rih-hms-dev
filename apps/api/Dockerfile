# syntax=docker/dockerfile:1
# ─────────────────────────────────────────────────────────────────────────────
# RIT HMS API — ASP.NET Core 8 (framework-dependent).
# BUILD CONTEXT MUST BE THE REPO ROOT: the runtime image bundles db/postgres/**,
# which ProvisioningService reads at signup to migrate + seed new tenant DBs.
#   docker build -f apps/api/Dockerfile -t rit-hms-api .
# ─────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore as its own layer (re-runs only when the csproj changes).
COPY apps/api/Hms.Api.csproj apps/api/
RUN dotnet restore apps/api/Hms.Api.csproj

# Build + publish.
COPY apps/api/ apps/api/
RUN dotnet publish apps/api/Hms.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl for the container HEALTHCHECK / compose probes.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish ./

# Raw SQL the API reads at runtime to provision tenant DBs on signup. Keep the
# db/postgres layout intact: ProvisioningService reads migrations/ and the
# provision_baseline.sql one level up (see Provisioning__MigrationsPath below).
COPY db/postgres/ ./db/postgres/

ENV ASPNETCORE_URLS=http://0.0.0.0:5000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    Provisioning__MigrationsPath=/app/db/postgres/migrations

EXPOSE 5000

# Liveness — process up. (Compose uses /health/ready to gate dependents on the DB.)
HEALTHCHECK --interval=15s --timeout=5s --start-period=40s --retries=10 \
  CMD curl -fsS http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Hms.Api.dll"]
