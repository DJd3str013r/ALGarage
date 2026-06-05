# =====================================================================================
# ÄLGarage — imagem do app (Blazor Web App, .NET 10).
# Multi-stage. Alvo principal: Raspberry Pi (linux/arm64). Também roda em amd64.
#
# Build (no Pi ou via buildx cross-compile):
#   docker buildx build --platform linux/arm64 -t algarage-web:local --load .
#
# IMPORTANTE: usamos a imagem aspnet baseada em Debian (NÃO a -alpine/-extra "noble-chiseled"
# sem ICU), porque a i18n (en + pt-BR) exige a biblioteca ICU de globalização.
# =====================================================================================

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore com cache: copia só o necessário para resolver pacotes primeiro.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY ALGarage.sln ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore

# Publica só o Web (host). Self-contained=false: usamos a imagem runtime do .NET.
RUN dotnet publish src/ALGarage.Web/ALGarage.Web.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Usuário não-root.
RUN useradd --uid 1001 --create-home appuser
USER appuser

COPY --from=build /app/publish ./

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ALGarage.Web.dll"]
