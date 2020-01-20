FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-alpine AS build
WORKDIR /src
COPY ["Keda.Samples.Dotnet.API.csproj", ""]

COPY . .
WORKDIR "/src/."
RUN dotnet build "Keda.Samples.Dotnet.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Keda.Samples.Dotnet.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Keda.Samples.Dotnet.API.dll"]
