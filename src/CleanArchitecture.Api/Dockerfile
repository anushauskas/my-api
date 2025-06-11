#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY global.json global.json
COPY Directory.Build.props Directory.Build.props
COPY Directory.Packages.props Directory.Packages.props
COPY src/CleanArchitecture.Domain/CleanArchitecture.Domain.csproj CleanArchitecture.Domain/
COPY src/CleanArchitecture.Application/CleanArchitecture.Application.csproj CleanArchitecture.Application/
COPY src/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj CleanArchitecture.Infrastructure/
COPY src/CleanArchitecture.Api/CleanArchitecture.Api.csproj CleanArchitecture.Api/

RUN dotnet restore "CleanArchitecture.Api/CleanArchitecture.Api.csproj"
COPY src/ .
WORKDIR "/src/CleanArchitecture.Api"
RUN dotnet build -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.Api.dll"]