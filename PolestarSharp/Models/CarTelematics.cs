namespace PolestarSharp.Models;

public class CarTelematicsData
{
    public Data data { get; set; }
}

public class Data
{
    public CarTelematics carTelematics { get; set; }
}

public class CarTelematics
{
    public Health health { get; set; }
    public Battery battery { get; set; }
    public Odometer odometer { get; set; }
}

public class Health
{
    public string brakeFluidLevelWarning { get; set; }
    public int daysToService { get; set; }
    public int distanceToServiceKm { get; set; }
    public string engineCoolantLevelWarning { get; set; }
    public EventUpdatedTimestamp eventUpdatedTimestamp { get; set; }
    public string oilLevelWarning { get; set; }
    public string serviceWarning { get; set; }
}

public class Battery
{
    public double averageEnergyConsumptionKwhPer100Km { get; set; }
    public int batteryChargeLevelPercentage { get; set; }
    public string chargerConnectionStatus { get; set; }
    public int chargingCurrentAmps { get; set; }
    public int chargingPowerWatts { get; set; }
    public string chargingStatus { get; set; }
    public object estimatedChargingTimeMinutesToTargetDistance { get; set; }
    public int estimatedChargingTimeToFullMinutes { get; set; }
    public int estimatedDistanceToEmptyKm { get; set; }
    public int estimatedDistanceToEmptyMiles { get; set; }
    public EventUpdatedTimestamp eventUpdatedTimestamp { get; set; }
}
public class Odometer
{
    public int averageSpeedKmPerHour { get; set; }
    public EventUpdatedTimestamp eventUpdatedTimestamp { get; set; }
    public int odometerMeters { get; set; }
    public int tripMeterAutomaticKm { get; set; }
    public double tripMeterManualKm { get; set; }
}

public class EventUpdatedTimestamp
{
    public string iso { get; set; }
    public string unix { get; set; }
    public string __typename { get; set; }
}