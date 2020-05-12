#!/bin/bash
# sudo install.sh

# Bring system current
apt-get update && sudo apt-get -y upgrade

# Install required pi-gen dependencies
apt-get -y install coreutils quilt parted qemu-user-static debootstrap zerofree zip \
dosfstools bsdtar libcap2-bin grep rsync xz-utils file git curl bc

# Get pi-gen git repo
git clone https://github.com/RPi-Distro/pi-gen.git
pushd pi-gen
chmod u+x build.sh
popd