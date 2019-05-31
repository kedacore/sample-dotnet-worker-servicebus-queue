FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-preview5-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview5-alpine AS build
WORKDIR /src
COPY ["Keda.Samples.Dotnet.QueueWorker/Keda.Samples.Dotnet.QueueWorker.csproj", "Keda.Samples.Dotnet.QueueWorker/"]
RUN dotnet restore "Keda.Samples.Dotnet.QueueWorker/Keda.Samples.Dotnet.QueueWorker.csproj"
COPY . .
WORKDIR "/src/Keda.Samples.Dotnet.QueueWorker"
RUN dotnet build "Keda.Samples.Dotnet.QueueWorker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Keda.Samples.Dotnet.QueueWorker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Keda.Samples.Dotnet.QueueWorker.dll"]