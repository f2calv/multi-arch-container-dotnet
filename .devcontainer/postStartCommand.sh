#!/bin/sh

echo "postStartCommand.sh"
echo "-------------------"

sudo apt-get update
sudo apt-get upgrade -y

rustup --version
rustc --version

echo "Done"