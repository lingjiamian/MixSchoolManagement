#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MixSchoolManagement.Web/MixSchoolManagement.Web.csproj", "MixSchoolManagement.Web/"]
COPY ["MixSchoolManagement.Application/MixSchoolManagement.Application.csproj", "MixSchoolManagement.Application/"]
COPY ["MixSchoolManagement.Core/MixSchoolManagement.Core.csproj", "MixSchoolManagement.Core/"]
COPY ["MixSchoolManagement.EntityFrameworkCore/MixSchoolManagement.EntityFrameworkCore.csproj", "MixSchoolManagement.EntityFrameworkCore/"]
RUN dotnet restore "MixSchoolManagement.Web/MixSchoolManagement.Web.csproj"
COPY . .
WORKDIR "/src/MixSchoolManagement.Web"
RUN dotnet build "MixSchoolManagement.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MixSchoolManagement.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MixSchoolManagement.Web.dll"]