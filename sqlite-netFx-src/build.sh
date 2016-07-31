#!/bin/bash
xbuild /p:Configuration=Release /p:UseInteropDll=false /p:UseSqliteStandard=true System.Data.SQLite/System.Data.SQLite.2015.csproj
mkdir -p ../sqlite-netFx-bin && cp -f bin/2015/Release/bin/{System.Data.SQLite.dll,System.Data.SQLite.dll.config,System.Data.SQLite.xml} ../sqlite-netFx-bin