namespace PolestarSharp.Models;

public class CarTelematicsData
{
    public Data data { get; set; }
}

public class Data
{
    public CarTelematicsV2 carTelematicsV2 { get; set; }
}

public class CarTelematicsV2
{
    public Health[] health { get; set; }
    public Battery[] battery { get; set; }
    public Odometer[] odometer { get; set; }
}

public class Health
{
    public string brakeFluidLevelWarning { get; set; }
    public int? daysToService { get; set; }
    public int? distanceToServiceKm { get; set; }
    public string engineCoolantLevelWarning { get; set; }
    public EventUpdatedTimestamp timestamp { get; set; }
    public string oilLevelWarning { get; set; }
    public string serviceWarning { get; set; }
}

public class Battery
{
    public double averageEnergyConsumptionKwhPer100Km { get; set; }
    public int batteryChargeLevelPercentage { get; set; }
    public string chargingStatus { get; set; }
    public int? estimatedChargingTimeToFullMinutes { get; set; }
    public int? estimatedDistanceToEmptyKm { get; set; }
    public EventUpdatedTimestamp timestamp { get; set; }
}
public class Odometer
{
    public EventUpdatedTimestamp timestamp { get; set; }
    public long odometerMeters { get; set; }
}

public class EventUpdatedTimestamp
{
    public string seconds { get; set; }
    public long nanos { get; set; }
    public string __typename { get; set; }
}