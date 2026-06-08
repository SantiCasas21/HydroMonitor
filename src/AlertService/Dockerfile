FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Shared/Shared.csproj Shared/
COPY src/AlertService/AlertService.csproj AlertService/
RUN dotnet restore AlertService/AlertService.csproj

COPY src/Shared/ Shared/
COPY src/AlertService/ AlertService/
RUN dotnet publish AlertService/AlertService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 5002
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AlertService.dll"]
