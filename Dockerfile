FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY WebhookArc/WebhookArc.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./WebhookArc/ ./
RUN rm -f appsettings.json
RUN touch appsettings.json
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
EXPOSE 50001
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "WebhookArc.dll"]