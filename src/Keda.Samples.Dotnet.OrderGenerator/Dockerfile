FROM mcr.microsoft.com/dotnet/core/runtime:3.1.1-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine AS build
WORKDIR /src
COPY ["Keda.Samples.Dotnet.OrderGenerator/Keda.Samples.Dotnet.OrderGenerator.csproj", "Keda.Samples.Dotnet.OrderGenerator/"]
RUN dotnet restore "Keda.Samples.Dotnet.OrderGenerator/Keda.Samples.Dotnet.OrderGenerator.csproj"
COPY . .
WORKDIR "/src/Keda.Samples.Dotnet.OrderGenerator"
RUN dotnet build "Keda.Samples.Dotnet.OrderGenerator.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Keda.Samples.Dotnet.OrderGenerator.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Keda.Samples.Dotnet.OrderGenerator.dll"]