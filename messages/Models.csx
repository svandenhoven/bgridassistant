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
    public IEnumerable<ParkingPlace> ParkingPlaces { get; set; }

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

public class bGridMovement
{
    public int location_id { get; set; }
    public int timestamp { get; set; }
    public float value { get; set; }
}


public class bGridOccpancy
{
    public int id { get; set; }
    public int location_id { get; set; }
    public int timestamp { get; set; }
    public int value { get; set; }
}


public class bGridAsset
{
    public int id { get; set; }
    public double x { get; set; }
    public double y { get; set; }
    public int lastSeen { get; set; }
    public int floor { get; set; }
    public string building { get; set; }
}

public class bGridRectangles
{
    public string Name { get; set; }
    public bGridRectangle Spot { get; set; }
}

public class bGridRectangle
{
    public bGridRectangle(int x1, int y1, int x2, int y2)
    {
        X1 = x1; X2 = x2; Y1 = y1; Y2 = y2;
    }

    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

}


