#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 3810

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Timetable/Timetable.csproj", "Timetable/"]
RUN dotnet restore "Timetable/Timetable.csproj"
COPY . .
WORKDIR "/src/Timetable"
RUN dotnet build "Timetable.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Timetable.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:3810
ENTRYPOINT ["dotnet", "Timetable.dll"]