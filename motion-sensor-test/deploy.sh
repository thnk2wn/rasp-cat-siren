dotnet publish -r linux-arm -c Debug -o bin/publish
rsync -av bin/publish/* pi@catsirenpi.local:motion-test