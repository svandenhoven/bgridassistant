using System;

//bGrid
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


//Weather

public class WeatherInfo
{
    public Coord coord { get; set; }
    public Weather[] weather { get; set; }
    public object _base { get; set; }
    public Main main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public int dt { get; set; }
    public Sys sys { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
}

public class Coord
{
    public float lon { get; set; }
    public float lat { get; set; }
}

public class Main
{
    public float temp { get; set; }
    public float pressure { get; set; }
    public float humidity { get; set; }
    public float temp_min { get; set; }
    public float temp_max { get; set; }
}

public class Wind
{
    public float speed { get; set; }
    public float deg { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class Sys
{
    public int type { get; set; }
    public int id { get; set; }
    public float message { get; set; }
    public string country { get; set; }
    public int sunrise { get; set; }
    public int sunset { get; set; }
}

public class Weather
{
    public int id { get; set; }
    public string main { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
}

//Parking
public class ParkingSpots
{
    public Parkingplace[] ParkingPlaces { get; set; }
    public float Rate { get; set; }
    public int Current { get; set; }
    public DateTime CurrentTime { get; set; }
    public int CurrentCapacity { get; set; }
    public int Trend { get; set; }
    public int RemainingMinutes { get; set; }
    public string TrendString { get; set; }
}

public class Parkingplace
{
    public DateTime CheckDate { get; set; }
    public int FreePlace { get; set; }
    public int Capacity { get; set; }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTime Timestamp { get; set; }
    public string ETag { get; set; }
}

public class B3Spots
{
    public string[] Spots
    {
        get
        {
            string[] spots = new string[37];
            spots[0] = "";
            spots[1] = "Experience room";
            spots[2] = "B3 Building lobby";
            spots[3] = "B3 Building lobby";
            spots[4] = "B3 Building lobby";
            spots[5] = "B3 Building lobby";
            spots[6] = "B3 Building lobby";
            spots[7] = "B3 Building lobby";
            spots[8] = "B3 Building lobby";
            spots[9] = "B3 Building lobby";
            spots[10] = "B3 Building lobby";
            spots[11] = "B3 Building lobby";
            spots[12] = "Seating area";
            spots[13] = "Ground floor desk 1";
            spots[14] = "Ground floor desk 2";
            spots[15] = "Ground floor desk 3";
            spots[16] = "Ground floor desk 4";
            spots[17] = "Hospitality desk";
            spots[18] = "Hospitality desk";
            spots[19] = "Ground floor meeting room";
            spots[20] = "Ground floor meeting room";
            spots[21] = "Ground floor meeting room";
            spots[22] = "Ground floor meeting room";
            spots[23] = "Experience room";
            spots[24] = "Ground floor meeting room";
            spots[25] = "Ground floor meeting room";
            spots[26] = "Experience Room";
            spots[27] = "Experience Room";
            spots[28] = "Microsoft 1st floor meeting room 1";
            spots[29] = "Microsoft 1st floor meeting room 2";
            spots[30] = "Microsoft 1st floor seating area";
            spots[31] = "Experience room";
            spots[32] = "First floor desk 1";
            spots[33] = "First floor desk 2";
            spots[34] = "First floor desk 3";
            spots[35] = "First floor desk 4";
            spots[36] = "First floor desk 5";

            return spots;
        }
    }
}



