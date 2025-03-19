FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
USER root

# Expose the app on a defined port, configurable via a build argument
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}


# Copy everything
USER root
#COPY EPR.Calculator.Frontend.Common/. ./EPR.Calculator.Frontend.Common/.
#COPY EPR.Calculator.Frontend/. ./EPR.Calculator.Frontend/.
COPY EPR.Calculator.Frontend/*.csproj ./EPR.Calculator.Frontend/
COPY EPR.Calculator.Frontend.Common/*.csproj ./EPR.Calculator.Frontend.Common/
COPY NuGet.Config ./

# Use Build Argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

# Restore as distinct layers

USER root
COPY Directory.Build.props ./
RUN dotnet restore "EPR.Calculator.Frontend/EPR.Calculator.Frontend.csproj" --configfile "NuGet.Config"

# Install NPM
RUN apk update && apk add --no-cache npm
USER dotnet
ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

USER root
COPY EPR.Calculator.Frontend/package.json package.json
RUN npm install --ignore-scripts

# Build and publish a release
USER root
COPY EPR.Calculator.Frontend/. ./EPR.Calculator.Frontend/.
COPY EPR.Calculator.Frontend.Common/. ./EPR.Calculator.Frontend.Common/.
WORKDIR /home/dotnet/EPR.Calculator.Frontend

USER root
RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore-development:dotnet8.0
COPY --from=build-env /home/dotnet/EPR.Calculator.Frontend/out .

# Install tzdata
USER root
RUN apk add --no-cache tzdata

# Add internationalisation support
USER root
RUN apk add --no-cache icu-libs icu-data-full libintl
USER dotnet

USER dotnet
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Expose the app on a defined port, configurable via a build argument
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet EPR.Calculator.Frontend.dll
