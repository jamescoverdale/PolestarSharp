# PolestarSharp
A small dotnet library to help access the Polestar web api.

### Usage

- Install the nuget package: `nuget install PolestarSharp`
- Perform a login and data request:
```c#
var api = new PolestarApi("email", "password", "vin");
await api.Login();

var battery = await api.GetBatteryInfo();
var odometer = await api.GetOdometerData();
```
