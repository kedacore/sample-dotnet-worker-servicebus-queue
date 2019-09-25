FROM mcr.microsoft.com/dotnet/core/runtime:3.0.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-alpine AS build
WORKDIR /src
COPY ["Keda.Samples.Dotnet.OrderProcessor/Keda.Samples.Dotnet.OrderProcessor.csproj", "Keda.Samples.Dotnet.OrderProcessor/"]
COPY ["Keda.Samples.Dotnet.Contracts/Keda.Samples.Dotnet.Contracts.csproj", "Keda.Samples.Dotnet.Contracts/"]
RUN dotnet restore "Keda.Samples.Dotnet.OrderProcessor/Keda.Samples.Dotnet.OrderProcessor.csproj"
COPY . .
WORKDIR "/src/Keda.Samples.Dotnet.OrderProcessor"
RUN dotnet build "Keda.Samples.Dotnet.OrderProcessor.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Keda.Samples.Dotnet.OrderProcessor.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Keda.Samples.Dotnet.OrderProcessor.dll"]