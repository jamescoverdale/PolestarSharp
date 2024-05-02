namespace PolestarSharp.Models;

public class OdometerData
{
    public oData data { get; set; }
}

public class oData
{
    public GetOdometerData getOdometerData { get; set; }
}

public class GetOdometerData
{
    public int? averageSpeedKmPerHour { get; set; }
    public EventUpdatedTimestamp eventUpdatedTimestamp { get; set; }
    public int? odometerMeters { get; set; }
    public double tripMeterAutomaticKm { get; set; }
    public double tripMeterManualKm { get; set; }
    public string __typename { get; set; }
}

