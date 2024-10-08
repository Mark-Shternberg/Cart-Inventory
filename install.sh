#!/bin/bash
colGreen="\033[32m"
colRed="\033[31m"
resetCol="\033[0m"

if [ $(id -u) -ne 0 ]; then
  echo -e "${colRed}This script can be executed only as root, Exiting...${resetCol}"
  exit 1
fi

if [[ " $@ " =~ " -update " ]]; then
    if ! systemctl list-units --type=service --all  > /dev/null 2>&1 | grep -q "cart-inventory"; then
        echo -e "${colRed}cart-inventory isn't installed. Run script without arguments, Exiting...${resetCol}"
        exit 0
    fi

    if [[ ! -d /var/www/cartinventory ]]; then
        echo -e "${colRed}cart-inventory isn't installed in default directory.\nUpdate program manually, Exiting...${resetCol}"
        exit 0;
    fi
    systemctl stop cart-inventory.service
    sleep 1

    find "cartinventory" -type f ! -name "appsettings.json" -exec cp --parents {} "/var/www/" \;

    systemctl start cart-inventory.service
    if [ $? -eq 0 ]; then
        echo -e "${colGreen}\tCart Inventory upgraded!${resetCol}"
    else
        echo -e "$colRed Upgrade error. $resetCol"
        exit 0 
    fi
    exit 0 
fi

execute_by_distro() {
  if [ -f /etc/os-release ]; then
    . /etc/os-release
    DISTRO=$ID
  elif command -v lsb_release > /dev/null 2>&1; then
    DISTRO=$(lsb_release -si)
  elif [ -f /etc/lsb-release ]; then
    . /etc/lsb-release
    DISTRO=$DISTRIB_ID
  elif [ -f /etc/debian_version ]; then
    DISTRO="debian"
  elif [ -f /etc/redhat-release ]; then
    DISTRO="redhat"
  else
    DISTRO="unknown"
  fi

  case "$DISTRO" in
    ubuntu|debian)
      apt -qqq update 
      apt install mysql-server nginx jq -y || { echo -e "${colRed}Error installing MySQL/NGINX! Exiting...${resetCol}"; exit 1; }
      ;;
    centos|rhel)
      yum update -q -y
      yum install mysql-server nginx jq -y || { echo -e "${colRed}Error installing MySQL/NGINX! Exiting...${resetCol}"; exit 1; }
      ;;
    fedora)
      dnf update -q -y
      dnf install mysql-server nginx jq -y || { echo -e "${colRed}Error installing MySQL/NGINX! Exiting...${resetCol}"; exit 1; }
      ;;
    arch)
      pacman -Syu --noconfirm
      pacman -S mariadb nginx jq --noconfirm || { echo -e "${colRed}Error installing MariaDB/NGINX! Exiting...${resetCol}"; exit 1; }
      ;;
    *)
      echo -e "${colRed}Unknown or unsupported Linux distribution: $DISTRO\nExiting...${resetCol}"
      exit 1
      ;;
  esac
}

while true; do
    echo -n "Will be installed: .NET SDK 8.0, jq, mysql-server and nginx. Ok? [Yy/Nn]: "
    read accept
    case $accept in
        [yY] ) break;;
        [nN] ) echo "Exiting..."; exit 0;;
        * ) echo -e " $colRed Type only Y or N !$resetCol";;
    esac
done

if [[ ! -d /var/www ]]; then
  mkdir /var/www
fi

mv cartinventory /var/www/cartinventory
cd /var/www/cartinventory
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 8.0
if [ $? -eq 0 ]; then
    echo -e "${colGreen}\tDotNET installed${resetCol}"
else
  echo -e "$colRed Error while installing DotNET. $resetCol"
  exit 0 
fi
sleep 2
mv /root/.dotnet /var/www/cartinventory/
chown -R www-data:www-data /var/www/cartinventory
chmod -R 744 /var/www/cartinventory

DBpassword=$(tr -dc 'A-Za-z0-9!?%=' < /dev/urandom | head -c 10)

execute_by_distro

read -p "Enter root password for MySQL: " mysql_root_password

mysql -u root -p"$mysql_root_password" << eof
CREATE DATABASE cartinvent;
CREATE USER 'cartinvent'@'localhost' IDENTIFIED WITH mysql_native_password BY '$DBpassword';
GRANT ALL PRIVILEGES ON cartinvent.* TO 'cartinvent'@'localhost';
FLUSH PRIVILEGES;
eof

JSON_FILE="/var/www/cartinventory/appsettings.json"

jq --arg server "localhost" \
   --arg user "cartinvent" \
   --arg password "$DBpassword" \
   --arg database "cartinvent" \
   '.MySQLConnection.server = $server | 
    .MySQLConnection.user = $user | 
    .MySQLConnection.password = $password | 
    .MySQLConnection.database = $database' \
   "$JSON_FILE" > tmp.$$.json && mv tmp.$$.json "$JSON_FILE"

read -p "Enter your server IP/DNS for NGINX: " server_name

rm /etc/nginx/sites-enabled/default
echo -e "server {\n\
server_name $server_name;\n\
  location / {\n\
    proxy_pass http://localhost:5000;\n\
  }\n\
}" > /etc/nginx/sites-enabled/cartinventory

if command -v systemctl > /dev/null 2>&1; then
  echo -e "[Unit]\nDescription=WEB application for cartridges inventorisation\n\
[Service]\n\
User=www-data\n\
WorkingDirectory=/var/www/cartinventory\n\
ExecStart=/var/www/cartinventory/.dotnet/dotnet Cart_Inventory.dll\n\
Restart=always\nRestartSec=5\n\
[Install]\n\
WantedBy=multi-user.target" > /etc/systemd/system/cart-inventory.service

  systemctl daemon-reload
  systemctl reload nginx
  systemctl start cart-inventory.service

  if [ $? -eq 0 ]; then
    systemctl enable cart-inventory.service
    echo -e "${colGreen}\tGreat! Cart inventory service installed and started\n\
\tNow you can go to: http://$server_name!${resetCol}"
  else
    echo -e "$colRed Error starting service. Check logs! $resetCol"
    exit 0 
  fi

else
  echo -e "${colRed}\tsystemctl not found! Please configure and start the service manually.${resetCol}"
fi
