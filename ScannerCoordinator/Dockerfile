FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5100
EXPOSE 5101

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ScannerCoordinator.csproj", "ScannerCoordinator/"]
COPY . .
RUN dotnet restore "ScannerCoordinator.csproj"
RUN dotnet build "ScannerCoordinator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p password
RUN dotnet publish "ScannerCoordinator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /publish
COPY --from=publish /app/publish /publish
COPY --from=publish /https /https
ENTRYPOINT ["dotnet", "/publish/ScannerCoordinator.dll"]
