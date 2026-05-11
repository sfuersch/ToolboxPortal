FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -r linux-x64 --self-contained true -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/playwright:v1.57.0-noble AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

COPY --from=build /app/publish .

RUN chmod +x /app/ToolboxPortal

EXPOSE 8080

ENTRYPOINT ["/app/ToolboxPortal"]