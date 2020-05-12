#!/bin/sh

# ./sd-card-write.sh --host catsirenpi

# Final host name (not initial login)
host_name=""

while [[ $# -ge 1 ]]; do
    i="$1"
    case $i in
        -h|--host)
            host_name=$2
            shift
            ;;
        *)
            echo "Unrecognized option $1"
            exit 1
            ;;
    esac
    shift
done

if [ -z "$host_name" ]; then
  echo "Final Pi host name is required (-h | --host)" >&2
  exit 1
fi

disk_name=$(diskutil list external | grep -o '^/dev\S*')
if [ -z "$disk_name" ]; then
    echo "Didn't find an external disk" ; exit -1
fi

matches=$(echo -n "$disk_name" | grep -c '^')
if [ $matches -ne 1 ]; then
    echo "Found ${matches} external disk(s); expected 1" ; exit -1
fi

disk_free=$(df -l -h | grep "$disk_name" | egrep -oi '(\s+/Volumes/.*)' | egrep -o '(/.*)')

if [ -z "$disk_free" ]; then
    echo "Disk ${disk_name} doesn't appear mounted. Try reinserting SD card" ; exit -1
fi

volume=$(echo "$disk_free" | sed -e 's/\/.*\///g')

# Spit out disk info for user confirmation
diskutil list external
echo $disk_free
echo

read -p "Format ${disk_name} (${volume}) (y/n)?" CONT
if [ "$CONT" = "n" ]; then
  exit -1
fi

image_path=./downloads
image_zip="$image_path/image.zip"
image_iso="$image_path/image.img"

# Consider checking latest ver/sha online, download only if newer
# https://downloads.raspberrypi.org/raspbian_lite/images/?C=M;O=D
# For now just delete any prior download zip to force downloading latest version
if [ ! -f $image_zip ]; then
  mkdir -p ./downloads
  echo "Downloading latest Raspbian lite image"
  # curl often gave "error 18 - transfer closed with outstanding read data remaining"
  wget -O $image_zip "https://downloads.raspberrypi.org/raspbian_lite_latest"

  if [ $? -ne 0 ]; then
    echo "Download failed" ; exit -1;
  fi
fi

echo "Extracting ${image_zip} ISO"
unzip -p $image_zip > $image_iso

if [ $? -ne 0 ]; then
    echo "Unzipping image ${image_zip} failed" ; exit -1;
fi

echo "Formatting ${disk_name} as FAT32"
sudo diskutil eraseDisk FAT32 PI MBRFormat "$disk_name"

if [ $? -ne 0 ]; then
    echo "Formatting disk ${disk_name} failed" ; exit -1;
fi

echo "Unmounting ${disk_name} before writing image"
diskutil unmountdisk "$disk_name"

if [ $? -ne 0 ]; then
    echo "Unmounting disk ${disk_name} failed" ; exit -1;
fi

echo "Copying ${image_iso} to ${disk_name}. ctrl+t as desired for status"
sudo dd bs=1m if="$image_iso" of="$disk_name" conv=sync

if [ $? -ne 0 ]; then
  echo "Copying ${image_iso} to ${disk_name} failed" ; exit -1
fi

# Remount for further SD card mods. Drive may not be quite ready.
attempt=0
until [ $attempt -ge 3 ]
do
  sleep 2s
  echo "Remounting ${disk_name}"
  diskutil mountDisk "$disk_name" && break
  attempt=$[$attempt+1]
done

echo "Removing ${image_iso}. Re-extract later if needed from ${image_zip}"
rm $image_iso

volume="/Volumes/boot"

echo "Enabling ssh"
touch "$volume"/ssh

if [ $? -ne 0 ]; then
  echo "Configuring ssh failed" ; exit -1
fi

echo "Configuring Wi-Fi"

wifi_ssid=$(/System/Library/PrivateFrameworks/Apple80211.framework/Resources/airport -I | awk -F: '/ SSID/{print $2}')
wifi_ssid=`echo $wifi_ssid | sed 's/^ *//g'` # trim

echo "Wi-Fi password for ${wifi_ssid}:"
read -s wifi_pwd

cat >"$volume"/wpa_supplicant.conf <<EOL
ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
update_config=1
country=US

network={
	ssid="${wifi_ssid}"
	psk="${wifi_pwd}"
}
EOL

if [ $? -ne 0 ]; then
  echo "Configuring wifi failed" ; exit -1
fi

echo "Copying setup script. After Pi boot, run: sudo /boot/setup.sh"
cp setup.sh "$volume"

echo "Modifying setup script"
# Replace "${host}" placeholder in the setup script on SD card with final host name passed to script
sed -i -e "s/\${host}/${host_name}/" "$volume/setup.sh"

echo "Copying docker pull script for app updates"
cp pull.sh "$volume"

cp raspbian-build.sh "$volume"

echo "Image burned. Remove SD card, insert in PI and power on"
sudo diskutil eject "$disk_name"

echo "Removing any prior PI SSH known hosts entry"
ssh-keygen -R raspberrypi.local # initial
ssh-keygen -R "$host_name.local"

echo "Power up the PI and give it a minute then"
echo "  ssh pi@raspberrypi.local"
echo "  yes, raspberry"
echo "  sudo /boot/setup.sh"