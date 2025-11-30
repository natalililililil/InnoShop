FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Users.Domain/Users.Domain.csproj Users.Domain/
COPY Users.Application/Users.Application.csproj Users.Application/
COPY Users.Infrastructure/Users.Infrastructure.csproj Users.Infrastructure/
COPY Users.Api/Users.Api.csproj Users.Api/

RUN dotnet restore Users.Api/Users.Api.csproj

COPY Users.Domain/ Users.Domain/
COPY Users.Application/ Users.Application/
COPY Users.Infrastructure/ Users.Infrastructure/
COPY Users.Api/ Users.Api/

RUN dotnet publish Users.Api/Users.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]