# Imagen base de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EXAPARCIALALVARO.csproj", "."]
RUN dotnet restore "EXAPARCIALALVARO.csproj"
COPY . .
RUN dotnet build "EXAPARCIALALVARO.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "EXAPARCIALALVARO.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EXAPARCIALALVARO.dll"]