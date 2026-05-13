FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["mct-timer/mct-timer.csproj", "mct-timer/"]
RUN dotnet restore "mct-timer/mct-timer.csproj"

COPY . .
WORKDIR /src/mct-timer
RUN dotnet publish "mct-timer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "mct-timer.dll"]
