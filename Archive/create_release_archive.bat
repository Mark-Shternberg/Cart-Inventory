@echo off
setlocal

:: Укажите пути к папкам
set "source=C:\Users\MedvedevN\Nextcloud\DEV\Cart_Inventory\bin\Release\net8.0\publish"
set "destination=C:\Users\MedvedevN\Nextcloud\DEV\Cart_Inventory\Archive"
set "archiveName=Cart_Inventory_Archive.tar.gz"

:: Создание архива
tar -czvf "%destination%\%archiveName%" -C "%source%" .

:: Проверка на ошибки
if %errorlevel% neq 0 (
    echo Ошибка при создании архива.
) else (
    echo Архив успешно создан: %destination%\%archiveName%
)

endlocal