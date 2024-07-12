# Cart Inventory

This is WEB application for quick inventarisation of cartridges and printers modules \
It is based on the idea of ​​using barcodes and a barcode scanner

![Screenshot of  Web UI](https://medvedev-it.ru/wp-content/uploads/2024/07/Screenshot-2024-07-12-124817-2.png)

# Installation via install script
> SCRIPT UNDER DEVELOPMENT

1. Download last release archive
2. Run `sudo chmod +x install-linux.sh && sudo ./install-linux.sh'

# Manual install

1. Download and install dotnet-sdk-8.0
2. Download and install MySQL server
3. Create user and table for programm
4. Download last release archive
5. Edit "MySQLConnection" block in appsettings.json
6. Run command `dotnet Cart_Inventory.dll` and go http://localhost:5000
