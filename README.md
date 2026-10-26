# Dsync

<p align="center">
  <img src="assets/logo_danny.png" alt="Dsync-logo" width="180">
</p>

Dsync er et enkelt Windows-program som hjelper deg med å synkronisere klokken.

Programmet kan:

- synkronisere tiden med Windows sine gjeldende innstillinger
- starte Windows Time-tjenesten på nytt og synkronisere
- bruke en valgt tidsserver, for eksempel `time.windows.com,0x8`

Programmet ber om administratorrettigheter når det startes.

## Bygg programmet

Krever .NET 10 SDK. Kjør denne kommandoen fra prosjektmappen:

```powershell
dotnet publish Dsync.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Den ferdige programfilen ligger i `publish/Dsync.exe`.

Laget av danny · [dvnny.no](https://dvnny.no)
