using System;

public enum ParkTrend
{
    Stable,
    FillingSlow,
    FillingFast,
    EmptyingSlow,
    EmptyingFast
}
public class ParkingStatus
{
    public double Rate { get; set; }
    public int Current { get; set; }
    public DateTime CurrentTime { get; set; }
    public int CurrentCapacity { get; set; }
    public ParkTrend Trend { get; set; }
    public int RemainingMinutes { get; set; }
    public string TrendString
    {
        get
        {
            return Trend.ToString();
        }
    }
    public IEnumerable<ParkingPlace> ParkingPlaces;

}

public class ParkingPlace
{
    public DateTime CheckDate { get; set; }
    public int FreePlace { get; set; }
    public int Capacity { get; set; }
}


public class bGridTemperature
{
    public int timestamp { get; set; }
    public float value { get; set; }
}
