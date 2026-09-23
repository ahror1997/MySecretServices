# MySecretServices

## Domain

http://localhost:8123 <=> `{domain}`

## Endpoints

`POST` `{domain}`/api/v1/printer/print

* body example (required)
```
{
    "id": 1,
    "name": "Coca Cola",
    "barcode": 123456789,
    "price": 12000,
    "copy": 2,
    "path": "C:\\Users\\Documents\\label.btw"
}
```

`GET` `{domain}`/api/v1/libra/status

* query example (required)
```
?ip=192.168.255.255
```

`GET` `{domain}`/api/v1/libra/clear-goods

* query example (required)
```
?ip=192.168.255.255
```

`POST` `{domain}`/api/v1/libra/uploadProducts

* query example (required)
```
?ip=192.168.255.255
```
* body example (required)
```
[
    {
        "id": 8,  //int ItemCode
        "price": 36000,  //double Price
        "name": "Coca cola",  //string NameFirst (первые 28 символов) + NameSecond (следующие 28)
        "groupCode": 20,  //int GroupCode 21 or 20
        "goodType": 1,  //int  GoodsType 0 or 1
        "PLUNumber": 1,  //int  PLUNumber, должен равняться id товара
    }
]
```
* ограничения драйвера: `name` режется на NameFirst/NameSecond по 28 символов каждое, хвост дальше 56 символов отбрасывается; `PLUNumber` обязан попадать в 1..PLUCount (PLUCount читается после Connect), иначе товар уходит в `failed` с кодом -9
* загрузка не прерывается на первом сбойном товаре — пишутся все, ответ:
```
{
    "success": true,   // true только когда failed пуст
    "message": "Written 10 of 10",
    "data": {
        "written": 10,
        "failed": [
            { "id": 8, "plu": 500, "code": -9, "message": "PLU 500 вне диапазона 1..300" },
            { "id": 12, "plu": 4, "code": 3, "message": "текст ошибки от драйвера (ResultCodeDescription)" }
        ]
    }
}
```

`POST` `{domain}`/api/v1/libra/setSettings

* query example (required)
```
?ip=192.168.255.255
```
* body example (required)
```
{
    "labelTitle": "title of label",
    "reclameString": "lorem ipsum",
    "shopName": "Name of the shop"
}
```
* дополнительно фиксирует на весах `PrefixBCType = 2` (печатать весовой/штучный префиксы 20/21, а не групповой код и не номер весов), иначе то, что реально печатается на этикетке, зависит от ручной настройки весов 1.2.3.1

`GET` `{domain}`/api/v1/libra/getWeight

* query example (required)
```
?ip=192.168.255.255
```

## Installing the service
* Use the InstallUtil.exe tool to install the service
* The InstallUtil.exe is located in the .NET Framework directory, e.g., `C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe``.
* Run the following command (Run as administrator): 
```
    C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe MySecretServices.exe
```

## Check if the service installed properly:
* `Win+R` type `services.msc`
* Look for the service name from the list
* If it's not running, start the service

## Uninstalling the service
* If service is running, stop it.
* Run the following command (Run as administrator):
```
    C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /u MySecretServices.exe
```

## Сборка и обновление инсталлера
* Собирается только на Windows в Visual Studio, конфигурация `Release`.
* После сборки в `MySecretServices/bin/Release` берём всё, кроме `*.pdb`, `*.InstallLog`, `*.InstallState` и папки `app.publish`.
* Отобранное содержимое копируем в `tools/release-tools/bridge_service` и коммитим уже в том репозитории (bridge_service там хранится как собранный бинарник, не исходники).