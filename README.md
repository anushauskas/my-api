# DDS dotnet API template

Template is fork from [Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture/tree/main#clean-architecture-solution-template)

## Getting started

The easiest way to install .NET template is 
```sh
git clone https://github.com/anushauskas/my-api.git
cd my-api
dotnet new install .
```

NB: for dotnet cli tool prior to version 7 you need to run
```sh
# Template Install
dotnet new --install .
# Template Uninstall
dotnet new --uninstall .
```

At this point you should be able to see template 
```sh
dotnet new list

Template Name            Short Name   Language  Tags         
-----------------------  -----------  --------  -------------------------------------
DDS dotnet API Solution  dds-api-sln  [C#]      API/ASP.NET/Clean Architecture
```

Navigate to your repositories folder and run 
```sh
dotnet new dds-api-sln -o MyService
```

## Development

### Docker

```sh
# Start and deploy database
docker-compose up deploydb
# Start authentication service
docker-compose up auth
# Start open telemetry collector service
docker-compose up collector
```


Fake authentication service running at [http://localhost:8181/token](http://localhost:8181/token). Use: 

* user: local
* password: local

Open telemetry collector grpc at localhost:4317

Grafana running at [localhost:3000](http://localhost:3000/)

SQL Server 2017 instance running on localhost non default port 1440 to not clash with local instance instalation.

* instance: localhost,1440
* user: sa
* password: Yukon900

```sh
sqlcmd -S"localhost,1440" -Usa -PYukon900
```

To destroy docker environment run

```sh
docker-compose down
```

To reinstall template after modifications

```sh
cd ap-core-dotnet-template
# Uninstall template
dotnet new uninstall .\
# Clean all untracked files that are not part of source controll and also template
git clean -dfx
# Install template
dotnet new install .\
```

## Changes

### sqlproj

DDS services often has custom sql procedure code therefore  template use sqlproj for MS SQL database project.
Project is setup for SQL Server 2014 compatibility. 

Database initialization from EF Core removed. 

### docker-compose

Required services 
* MS SQL Database
* OpenTelemetry Collector 

Optional services
* Loki
* Prometheus
* Grafana

### Authentication

Replaced in-host Identity server authentication with with external Bearrer Token authentication. 

### OpenTelemetry

Application use [opentelemetry sdk](https://github.com/open-telemetry/opentelemetry-dotnet)

## References

* [Clean Architecture with ASP.NET Core 3.0 • Jason Taylor](https://www.youtube.com/watch?v=dK4Yb6-LxAk) explains templates
* [Unleashing Clean Architecture in .NET 8: Exploring the Solution Template](https://www.youtube.com/watch?v=jhgxdDhNicI) dotnet 8 updates
* [Clean Architecture](https://github.com/ardalis/CleanArchitecture) other implementation of clean architecture
* [Microsoft eShopOnWeb ASP.NET Core Reference Application](https://github.com/dotnet-architecture/eShopOnWeb)
* [.NET Microservices Sample Reference Application](https://github.com/dotnet-architecture/eShopOnContainers)
* [.NET observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
* []()
