# Stage 1 - build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.102 AS build

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

WORKDIR /build
COPY . .
RUN echo $(ls .)
RUN dotnet restore
RUN dotnet publish -c Release -o /app
RUN echo $(ls /app)

# Stage 2 - Final runtime base image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.2-buster-slim-arm32v7 AS final

WORKDIR /app
COPY --from=build /app .

# --- 3rd party lib required env vars ---

# Currently needed for camera access
ENV LD_LIBRARY_PATH /opt/vc/lib

# For Unosquare camera (which just shells to raspistill)
ENV PATH="/opt/vc/bin:${PATH}"

# --- PI System Level Env Vars ---

# Time will be UTC by Default, regardless of host PI settings
ENV TZ=America/New_York

# --- Serilog env variable defaults ---

# Default log level
ENV Serilog__MinimumLevel Information

# Log level for Camera Library
ENV Serilog__MinimumLevel__Override__MMALSharp Information

# Tone down MS ASP.NET Core logging
ENV Serilog__MinimumLevel__Override__Microsoft Warning

# --- Siren App Custom Env Variable Defaults ---

# Number of seconds to capture footage after motion detection
ENV Siren__CaptureDuration 10

# Passive Infrared (PIR) GPIO Pin
ENV Siren__PirPin 17

# Cooldown period in seconds after motion detection before detection will be enabled again
ENV Siren__ResetMotionAfter 15

# Number of days after which to delete previously captured siren footage.
ENV Siren__MediaCleanupAfterDays 1

# Needs over a minute for accurate initial PIR sensor readings.
ENV Siren__WarmupInterval 90

ENTRYPOINT ["dotnet", "./siren.dll"]