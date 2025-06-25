# CleanArchitecture API

The project was generated using the [DDS dotnet API template](https://github.com/anushauskas/my-api).

## Docker

```sh
cd CleanArchitecture
docker-compose build
docker-compose up deploydb
docker-compose up
```
At this point you should have running api at [localhost:5040](http://localhost:5040/swagger/index.html#/WeatherForecast/WeatherForecast_Get)

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


## Build

```sh
# Require database to be up and running:
docker-compose up deploydb
```

Once database is up and runnig, you should be able to build solution in devenv (Visual Studio)

`dotnet build` at the root of solution fails 
due to `.sqlproj` incompatibility with [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management). However you shoudl be able to build API at project level:

```sh
cd .\src\CleanArchitecture.Api\
dotnet build
```

Run `dotnet build -tl` to build the solution.

## Run

To run the web application:

```bash
cd .\src\CleanArchitecture.Api\
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\CleanArchitecture.Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::8.0.0-preview.6.18
```

## Test

<!--#if (UseApiOnly) -->
The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```
<!--#else -->
The solution contains unit, integration, functional, and acceptance tests.

To run the unit, integration, and functional tests (excluding acceptance tests):
```bash
dotnet test --filter "FullyQualifiedName!~AcceptanceTests"
```

To run the acceptance tests, first start the application:

```bash
cd .\src\CleanArchitecture.Api\
dotnet run
```

Then, in a new console, run the tests:
```bash
cd .\tests\CleanArchitecture.Api.AcceptanceTests\
dotnet test
```
<!--#endif -->

## Help
To learn more about the template go to the [project website](https://github.com/JasonTaylorDev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.