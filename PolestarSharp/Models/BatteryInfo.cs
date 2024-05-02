namespace PolestarSharp.Models;

public class BatteryData
{
    public bData? data { get; set; }
}

public class bData
{
    public GetBatteryData getBatteryData { get; set; }
}

public class GetBatteryData
{
    public double? averageEnergyConsumptionKwhPer100Km { get; set; }
    public double? batteryChargeLevelPercentage { get; set; }
    public string chargerConnectionStatus { get; set; }
    public int? chargingCurrentAmps { get; set; }
    public int? chargingPowerWatts { get; set; }
    public string chargingStatus { get; set; }
    public string estimatedChargingTimeMinutesToTargetDistance { get; set; }
    public int? estimatedChargingTimeToFullMinutes { get; set; }
    public int? estimatedDistanceToEmptyKm { get; set; }
    public int? estimatedDistanceToEmptyMiles { get; set; }
    public EventUpdatedTimestamp eventUpdatedTimestamp { get; set; }
    public string __typename { get; set; }
}

public class EventUpdatedTimestamp
{
    public string iso { get; set; }
    public string unix { get; set; }
    public string __typename { get; set; }
}