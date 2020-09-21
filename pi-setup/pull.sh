#!/bin/bash

name=cat-siren
username=thnk2wn

while [[ $# -ge 1 ]]; do
    i="$1"
    case $i in
        -d|--debug)
            name=cat-siren-debug
            shift
            ;;
        *)
            echo "Unrecognized option $1"
            exit 1
            ;;
    esac
    shift
done

image="ghcr.io/$username/$name:latest"
imageIdBefore=$(docker images --format '{{.ID}}' $image)

echo "Pulling latest image - $image"
docker pull $image

if [ $? -ne 0 ]; then
    echo "Pull failed. Are you logged in $(whoami)?"

    if [[ ! -f "~/.docker/config.json" ]]
    then
        docker login || exit 1
        docker pull $image
    else
        exit 1
    fi
fi

imageIdAfter=$(docker images --format '{{.ID}}' $image)

if [ "$imageIdBefore" = "$imageIdAfter" ]; then
  echo "Nothing to do; pull did not result in a new image"
  exit 1
fi

buildVer=$(docker inspect -f '{{ index .Config.Labels "BUILD_VER" }}' $image)
createdOn=$(docker inspect -f '{{ index .Config.Labels "org.opencontainers.image.created" }}' $image)

echo "New image => Id: $imageIdAfter, Build Version $buildVer, Created on: $createdOn"

echo "Stopping and removing existing container"
docker stop $name || true && docker rm $name || true

# For debugging library loading issues add: -e "LD_DEBUG=all"
# http://www.bnikolic.co.uk/blog/linux-ld-debug.html
# Debug dependencies example: ldd /opt/vs/lib/libmmal.so

echo "Creating new container and starting"
docker run \
    --privileged \
    -e Siren__ResetMotionAfter=15 \
    -e Siren__CaptureDuration=10 \
    -e Siren__WarmupInterval=75 \
    -e Serilog__MinimumLevel=Information \
    -e Serilog__MinimumLevel__Override__MMALSharp=Information \
    --mount type=bind,source=/opt/vc/lib,target=/opt/vc/lib,readonly \
    --mount type=bind,source=/opt/vc/bin,target=/opt/vc/bin,readonly \
    -v /home/pi/motion-media:/var/lib/siren/media \
    -d \
    --name $name \
    $image

echo "Cleaning up old images"
docker image prune -f

echo "Running. Tailing logs; ctrl+c to stop"
docker logs -f $name