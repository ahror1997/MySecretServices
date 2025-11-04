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
        "name": "Coca cola",  //string NameFirst
        "groupCode": 20,  //int GroupCode 21 or 20
        "goodType": 1,  //int  GoodsType 0 or 1
        "PLUNumber": 1,  //int  PLUNumber
    }
]
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