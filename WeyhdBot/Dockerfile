FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["WeyhdBot/WeyhdBot.csproj", "WeyhdBot/"]
COPY ["Framework/WeyhdBot.Core/WeyhdBot.Core.csproj", "Framework/WeyhdBot.Core/"]
RUN dotnet restore "WeyhdBot/WeyhdBot.csproj"
COPY . .
WORKDIR "/src/WeyhdBot"
RUN dotnet build "WeyhdBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WeyhdBot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WeyhdBot.dll"]