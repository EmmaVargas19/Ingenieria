FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /app
EXPOSE 80

COPY ChocarApp.sln ChocarApp.sln
COPY src/ChocarService/ChocarService.csproj src/ChocarService/ChocarService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# Restore package deps
RUN dotnet restore ChocarApp.sln

# Copy the app folders over
COPY src/ChocarService src/ChocarService
WORKDIR /app/src/ChocarService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "ChocarService.dll" ]