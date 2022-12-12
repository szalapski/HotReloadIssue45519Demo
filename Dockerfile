#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TradeCars.sln", "."]
COPY ["TradeCars/Server/TradeCars.Server.csproj", "TradeCars/Server/"]
COPY ["TradeCars/Client/TradeCars.Client.csproj", "TradeCars/Client/"]
COPY ["TradeCars/Shared/TradeCars.Shared.csproj", "TradeCars/Shared/"]
RUN dotnet restore --source https://nuget.generalmills.com/nuget/

COPY . .
WORKDIR "/src/TradeCars"
RUN dotnet build "Server/TradeCars.Server.csproj" -c Release -o /app/build

# create cert so Docker can run https locally (does not register cert)
RUN dotnet dev-certs https -ep /tmp/httpsCert.pfx -p ""

FROM build AS publish
# set time zone
RUN ln -snf /usr/share/zoneinfo/America/Chicago /etc/localtime && echo America/Chicago > /etc/timezone

ARG branch_build_commit

RUN dotnet publish "Server/TradeCars.Server.csproj" -c Release -o /app/publish \
    && echo -n $branch_build_commit > /app/publish/buildInfo.txt \
    && date +" %Y-%m-%d %T %z" >> /app/publish/buildInfo.txt \
    && cat /app/publish/buildInfo.txt

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# this command will replace place url parameter for app.css.min line in index to force refresh of css on new builds
#RUN sed -i "s/app.min.css/app.min.css?version=$branch_build_commit/g" /tmp/output/wwwroot/index.html

EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://*:8080

RUN mkdir -p /app/machine_keys && chmod 777 /app/machine_keys

COPY --from=build /tmp/*.pfx /app/
RUN chmod a+r /app/*.pfx

RUN ls /app/*.pfx

#COPY --from=build /tmp/output/ .
#COPY --from=css-build /tmp/*.css ./wwwroot/css/
COPY TradeCars/Server/GmiSha2Root.pem /etc/ssl/certs/

ENTRYPOINT ["dotnet", "TradeCars.Server.dll"]