# Home Assignment

You will be required to write unit tests and automated tests for a payment application to demonstrate your skills. 

# Application information 

It’s an small microservice that validates provided Credit Card data and returns either an error or type of credit card application. 

# API Requirements 

API that validates credit card data. 

Input parameters: Card owner, Credit Card number, issue date and CVC. 

Logic should verify that all fields are provided, card owner does not have credit card information, credit card is not expired, number is valid for specified credit card type, CVC is valid for specified credit card type. 

API should return credit card type in case of success: Master Card, Visa or American Express. 

API should return all validation errors in case of failure. 


# Technical Requirements 

 - Write unit tests that covers 80% of application 
 - Write integration tests (preferably using Reqnroll framework) 
 - As a bonus: 
    - Create a pipeline where unit tests and integration tests are running with help of Docker. 
    - Produce tests execution results. 

# Running the  application 

1. Fork the repository
2. Clone the repository on your local machine 
3. Compile and Run application Visual Studio 2022.

# Solution

## Automated Tests

- Unit tests live in `tests/CardValidation.Core.Tests` and cover the validation service plus the MVC filter logic. Run them (and the integration tests below) with:
  ```
  dotnet test CardValidation.sln
  ```

- Integration tests live in `tests/CardValidation.Web.IntegrationTests` and exercise the `/CardValidation/card/credit/validate` API through `WebApplicationFactory`. The tests expect the service to reject malformed payloads and return the correct payment system type.

The solution uses `coverlet.collector` to generate coverage during the same `dotnet test` invocation.

## Docker Test Pipeline

- Local runs: execute all unit + integration tests inside a Dockerized .NET SDK by running:
  ```
  ./scripts/run-tests-docker.sh
  ```
  This pulls `mcr.microsoft.com/dotnet/sdk:8.0`, mounts the repository into the container, and writes TRX + coverage artifacts to `TestResults/`.

- Continuous integration: the workflow defined in `.github/workflows/tests.yml` runs on every push/Pull Request and offers two options:
  - `run-tests` job executes inside the `mcr.microsoft.com/dotnet/sdk:8.0` container, runs `dotnet test` with coverage, generates HTML reports, and uploads `TestResults/` + `coverage-report/`.
  - `run-tests-docker` job stays on the default Ubuntu runner, invokes the same `./scripts/run-tests-docker.sh` to exercise Docker-in-Docker, then reuses ReportGenerator + artifact upload.

## Test Coverage & HTML Reports

This repository ships with a local `dotnet-tools` manifest that includes [ReportGenerator](https://reportgenerator.io). To generate a browsable coverage report:

1. Run the full test suite with coverage enabled:
   ```
   dotnet test CardValidation.sln -c Release --collect:"XPlat Code Coverage" --results-directory ./TestResults
   ```
2. Restore the manifest tools (first run only):
   ```
   dotnet tool restore
   ```
3. Produce the HTML report from all Cobertura files:
   ```
   dotnet tool run reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html"
   ```
4. Open `coverage-report/index.htm` in a browser to inspect the results.
