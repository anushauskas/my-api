FROM mcr.microsoft.com/dotnet/runtime AS base

RUN apt update \
	&& apt install -y curl gnupg unzip \
	&& curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
	&& curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list | tee /etc/apt/sources.list.d/msprod.list \
	&& apt update

ENV ACCEPT_EULA=Y
RUN apt install -y mssql-tools unixodbc-dev

WORKDIR /tools
RUN curl -L -o sqlpackage.zip https://aka.ms/sqlpackage-linux \
	&& unzip sqlpackage.zip -d sqlpackage \
    && chmod a+x sqlpackage/sqlpackage

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/CleanArchitecture.Database/ CleanArchitecture.Database/
RUN dotnet build "CleanArchitecture.Database/CleanArchitecture.Database.sqlproj" -c Release /p:NetCoreBuild=true -o /app/build

FROM base AS final
WORKDIR /app
COPY --from=build /src/CleanArchitecture.Database/_publish/CleanArchitecture.Local.publish.xml CleanArchitecture.Local.publish.xml
COPY --from=build /src/CleanArchitecture.Database/_publish/CleanArchitecture.Test.publish.xml CleanArchitecture.Test.publish.xml
COPY --from=build /src/CleanArchitecture.Database/setup.sh setup.sh
COPY --from=build /app/build .

ENTRYPOINT /app/setup.sh
