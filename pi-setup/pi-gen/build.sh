#!/bin/bash

# echo "password" | sudo -S ./build.sh
read -r -t 2 -d $'\0' pi_pwd

if [ -z "$pi_pwd" ]; then
  echo "Pi user password is required (pass via stdin)" >&2
  exit 1
fi

# Assume our build.sh is a level above pi-gen
pushd pi-gen

user_name="siren"

# setup environment variables to tweak config. SD card write script will prompt for WIFI creds.
cat > config <<EOL
export IMG_NAME=raspbian-lite-catsiren
export RELEASE=buster
export DEPLOY_ZIP=1
export LOCALE_DEFAULT=en_US.UTF-8
export TARGET_HOSTNAME=catsirenpi
export KEYBOARD_KEYMAP=us
export KEYBOARD_LAYOUT="English (US)"
export TIMEZONE_DEFAULT=America/New_York
export FIRST_USER_NAME="${user_name}"
export FIRST_USER_PASS="${pi_pwd}"
export ENABLE_SSH=1
EOL

# Skip stages 3-5, only want Raspbian lite
touch ./stage3/SKIP ./stage4/SKIP ./stage5/SKIP
touch ./stage4/SKIP_IMAGES ./stage5/SKIP_IMAGES

custom_dir="./stage2/04-custom"
if [ -d "$custom_dir" ]; then rm -Rf $custom_dir; fi
mkdir $custom_dir && pushd $custom_dir

cat > 01-run.sh <<RUN
#!/bin/bash -e
on_chroot << EOF
  sed -i "s/start_x=0/start_x=1/g" /boot/config.txt

  curl -sSL https://get.docker.com | sh
  sudo usermod -aG docker $user_name
EOF
RUN

chmod +x 01-run.sh

popd

# run build
sudo ./build.sh

ls ./deploy

popd

# On another machine, copy generated zip over
# scp 'pi@raspberrypi.local:~/pi-gen/deploy/*zip' ~/temp

# Then run sd-card-write.sh to write that image to SD card for PI use.