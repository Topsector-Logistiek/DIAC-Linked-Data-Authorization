#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["ApiDiac/ApiDiac.csproj", "ApiDiac/"]
COPY ["ApiDiac.Domain/ApiDiac.Domain.csproj", "ApiDiac.Domain/"]
COPY ["ApiDiac.Data/ApiDiac.Data.csproj", "ApiDiac.Data/"]
COPY ["ApiDiac.Services/ApiDiac.Services.csproj", "ApiDiac.Services/"]
RUN dotnet restore "ApiDiac/ApiDiac.csproj"

COPY . .
WORKDIR "/src/ApiDiac"
RUN dotnet build "ApiDiac.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiDiac.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiDiac.dll"]