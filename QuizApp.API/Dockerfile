#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QuizApp.API/QuizApp.API.csproj", "QuizApp.API/"]
RUN dotnet restore "QuizApp.API/QuizApp.API.csproj"
COPY . .
WORKDIR "/src/QuizApp.API"
RUN dotnet build "QuizApp.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuizApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuizApp.API.dll"]