#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY . .
EXPOSE 8889

ENTRYPOINT ["dotnet", "MixSchoolManagement.Web.dll", "-b", "0.0.0.0"]