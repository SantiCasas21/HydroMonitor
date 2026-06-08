FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Shared/Shared.csproj Shared/
COPY src/WaterDataService/WaterDataService.csproj WaterDataService/
RUN dotnet restore WaterDataService/WaterDataService.csproj

COPY src/Shared/ Shared/
COPY src/WaterDataService/ WaterDataService/
RUN dotnet publish WaterDataService/WaterDataService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 5001
COPY --from=build /app .
ENTRYPOINT ["dotnet", "WaterDataService.dll"]
