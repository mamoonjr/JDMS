FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY JDMS.sln .
COPY src/JDMS.Domain/JDMS.Domain.csproj src/JDMS.Domain/
COPY src/JDMS.Application/JDMS.Application.csproj src/JDMS.Application/
COPY src/JDMS.Infrastructure/JDMS.Infrastructure.csproj src/JDMS.Infrastructure/
COPY src/JDMS.Web/JDMS.Web.csproj src/JDMS.Web/
RUN dotnet restore src/JDMS.Web/JDMS.Web.csproj
COPY src/ src/
RUN dotnet publish src/JDMS.Web/JDMS.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JDMS.Web.dll"]
