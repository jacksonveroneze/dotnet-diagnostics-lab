# =====================================================
# Build + Publish
# =====================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS publish

WORKDIR /src

COPY ./main/Api/Api.csproj ./main/Api/
COPY *.sln ./
COPY nuget.config ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

RUN dotnet restore ./main/Api/Api.csproj \
    -r linux-x64 \
    -p:PublishReadyToRun=true

COPY . .

RUN dotnet publish ./main/Api/Api.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    --no-restore \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=false \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    --output /publish

# =====================================================
# Etapa Final: Imagem de Runtime
# =====================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

ENV DOTNET_GCDynamicAdaptationMode=1

COPY --from=publish /publish /home/app

WORKDIR /home/app

ENTRYPOINT ["dotnet", "/home/app/Api.dll"]
