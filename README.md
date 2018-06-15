# Neo blockchain integration with Bluzelle

<a href="https://bluzelle.com/"><img src='https://raw.githubusercontent.com/bluzelle/api/master/source/images/Bluzelle%20-%20Logo%20-%20Big%20-%20Colour.png' alt="Bluzelle" style="width: 100%"/></a>

## Status
[![Build Status](https://travis-ci.org/Relfos/neo-bluzelle.png)](https://travis-ci.org/Relfos/neo-bluzelle)

## Instalation / Compilation

First add Microsoft feed to package manager
```console
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```

Install dotnet and required deps
```console
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

Compile and run
```console
dotnet build
dotnet run Bluzelle.NEO.Bridge.dll
```