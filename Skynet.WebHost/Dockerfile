#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Skynet.WebHost/Skynet.WebHost.csproj", "Skynet.WebHost/"]
RUN dotnet restore "Skynet.WebHost/Skynet.WebHost.csproj"
COPY . .
WORKDIR "/src/Skynet.WebHost"
RUN dotnet build "Skynet.WebHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Skynet.WebHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Skynet.WebHost.dll"]