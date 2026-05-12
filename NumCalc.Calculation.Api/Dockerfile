# Stage 1: build backend

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend

WORKDIR /src

COPY ./NumCalc.Shared ./NumCalc.Shared/
COPY ./NumCalc.Calculation.Business ./NumCalc.Calculation.Business/
COPY ./NumCalc.Calculation.Api ./NumCalc.Calculation.Api/

RUN dotnet publish ./NumCalc.Calculation.Api/NumCalc.Calculation.Api.csproj -c Release --output /app/publish

# Stage 2: run app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=backend /app/publish .

ENV ASPNETCORE_URLS=http://*:8080
ENV Python__VenvPath=/app/.venv
EXPOSE 8080

ENTRYPOINT ["dotnet", "NumCalc.Calculation.Api.dll"]
