FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# Copy csproj and restore as distinc layers
COPY Zomo.Core/*.csproj Zomo.Core/
COPY Zomo.Desktop/*.csproj Zomo.Desktop/
COPY Zomo.Server/*.csproj Zomo.Server/
COPY Zomo.ShineBot/*.csproj Zomo.ShineBot/

RUN dotnet restore Zomo.Desktop/Zomo.Desktop.csproj
RUN dotnet restore Zomo.Server/Zomo.Server.csproj

# Copy and build libs and apps
COPY Zomo.Core/ Zomo.Core/
COPY Zomo.Desktop/ Zomo.Desktop/
COPY Zomo.Server/ Zomo.Server/
COPY Zomo.ShineBot/ Zomo.ShineBot/

# Server Build
WORKDIR /source/Zomo.Server
RUN dotnet build -c release --no-restore

FROM build AS test
WORKDIR /source/tests
COPY Zomo.Core.Tests/ .
ENTRYPOINT ["dotnet", "Zomo.Core.Tests", "--logger:trx"]




FROM build AS publish
RUN dotnet publish -c release --no-build -o /app
# Make plugins dir
RUN mkdir /app/Plugins
# ShineBot Plugin Build
WORKDIR /source/Zomo.ShineBot
RUN dotnet build -c release --no-restore -o /plugins/
RUN mv /plugins/Zomo.ShineBot.dll /app/Plugins

# Build needed deps
ENV TZ=America/Los_Angeles
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN apt-get -y update

# install needed tools
RUN apt-get -y install curl ffmpeg libsodium-dev libopus-dev

#Youtube-dl
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
RUN chmod a+rx /usr/local/bin/youtube-dl

# final
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
#No idea what dependices need so im just taking the root
COPY --from=publish / / 
COPY --from=publish /usr/lib/x86_64-linux-gnu/libopus.so /app
COPY --from=publish /usr/lib/x86_64-linux-gnu/libsodium.so /app
ENTRYPOINT ["dotnet", "Zomo.Server.dll"]