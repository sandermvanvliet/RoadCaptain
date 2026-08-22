FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

ARG CONFIGURATION=Release
RUN dotnet publish --no-self-contained \
    -c ${CONFIGURATION} \
    -r linux-x64 \
    -p:RuntimeIdentifiers=linux-x64 \
    -o /app/publish \
    src/RoadCaptain.App.Web/RoadCaptain.App.Web.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RoadCaptain.App.Web.dll"]
