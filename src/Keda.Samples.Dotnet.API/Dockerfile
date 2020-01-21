FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine AS build
WORKDIR /src
COPY ["Keda.Samples.Dotnet.API/Keda.Samples.Dotnet.API.csproj", "Keda.Samples.Dotnet.API/"]
COPY ["Keda.Samples.Dotnet.Contracts/Keda.Samples.Dotnet.Contracts.csproj", "Keda.Samples.Dotnet.Contracts/"]
RUN dotnet restore "Keda.Samples.Dotnet.API/Keda.Samples.Dotnet.API.csproj"
COPY . .
WORKDIR "/src/Keda.Samples.Dotnet.API"
RUN dotnet build "Keda.Samples.Dotnet.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Keda.Samples.Dotnet.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Keda.Samples.Dotnet.API.dll"]