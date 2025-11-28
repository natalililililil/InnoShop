FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Products.Domain/Products.Domain.csproj Products.Domain/
COPY Products.Application/Products.Application.csproj Products.Application/
COPY Products.Infrastructure/Products.Infrastructure.csproj Products.Infrastructure/
COPY Products.Api/Products.Api.csproj Products.Api/

RUN dotnet restore Products.Api/Products.Api.csproj

COPY Products.Domain/ Products.Domain/
COPY Products.Application/ Products.Application/
COPY Products.Infrastructure/ Products.Infrastructure/
COPY Products.Api/ Products.Api/

RUN dotnet publish Products.Api/Products.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Products.Api.dll"]