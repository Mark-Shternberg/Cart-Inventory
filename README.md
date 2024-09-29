# Cart Inventory

This is WEB application for quick inventarisation of cartridges and printers modules \
It is based on the idea of ​​using barcodes and a barcode scanner

![Screenshot of Web UI](https://medvedev-it.ru/wp-content/uploads/2024/09/Screenshot-2024-09-28-164148.png)

# Installation via install script

1. Download last release archive
2. `mkdir cart-inventory-install && tar -xzvf cart-inventory-v* -C cart-inventory-install && rm cart-inventory-v*`
3. `cd cart-inventory-install`
4. Run `sudo chmod +x install.sh && su root ./install.sh`

# Manual install

1. Download and install dotnet-sdk-8.0
2. Download and install MySQL server
3. Create user and table for programm
4. Download last release archive
5. Edit "MySQLConnection" block in appsettings.json
6. Run command `dotnet Cart_Inventory.dll`
7. Install reverse proxy to http://localhost:5000

# How to use

1. Add models of cartridges or modules<br>
  *you can add model with several barcodes, separate them with `,`
2. Add printers with linked related models of cartridges and models
  *link with models is necessary, or printer will not be added
3. Now in the NEW INVENTORISATION page you can add cartridges via barcode scanner or entering barcode manually
4. All done

# Upgrade via upgrade script

1. Download last release archive
2. `mkdir -p cart-inventory-upgrade && tar -xzvf cart-inventory-v* -C cart-inventory-upgrade`
3. `cd cart-inventory-upgrade`
4. Run `sudo chmod +x install.sh && sudo ./install.sh -update`

# Manual upgrade

1. Download last release archive
2. `mkdir cart-inventory-upgrade && tar -xzvf cart-inventory-v* -C cart-inventory-upgrade`
3. Move all exept appsettings.json from cart-inventory-upgrade to your program folder