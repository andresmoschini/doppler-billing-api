FROM mcr.microsoft.com/dotnet/core/sdk:2.1.504 AS restore
WORKDIR /src
COPY ./Billing.sln ./
COPY Billing.API/Billing.API.csproj ./Billing.API/
COPY Billing.API.Test/Billing.API.Test.csproj ./Billing.API.Test/
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet build -c Release

FROM build AS test
RUN dotnet test /p:CollectCoverage=true

FROM build AS publish
RUN dotnet publish "Billing.API/Billing.API.csproj" -c Release -o /app/publish

# When it is running in a W2016 host, it uses mcr.microsoft.com/dotnet/core/aspnet:2.1.8-nanoserver-sac2016
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1.8 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
COPY ./libadonetHDB.dll .
COPY ./wwwroot_extras/ /app/wwwroot/
ENTRYPOINT ["dotnet", "Billing.API.dll"]
