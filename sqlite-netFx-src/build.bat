msbuild /p:Configuration=Release /p:UseInteropDll=false /p:UseSqliteStandard=true System.Data.SQLite/System.Data.SQLite.2013.csproj
mkdir ..\sqlite-netFx-bin
for %%I in (bin\2013\Release\bin\System.Data.SQLite.dll bin\2013\Release\bin\System.Data.SQLite.dll.config bin\2013\Release\bin\System.Data.SQLite.xml) do copy /Y %%I ..\sqlite-netFx-bin