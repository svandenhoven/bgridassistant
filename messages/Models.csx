using System;
using System.Collections;

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
    public string SpotKey(string name)
    {
        foreach(var k in Spots.Keys)
        {
            if(Spots[k].ToString().ToLower() == name.ToLower())
            {
                return k.ToString();
            }
        }
        return name;
    }
    public Hashtable Spots
    {
        get
        {         
            Hashtable spots = new Hashtable();
            //Edge Olympic
            /*
            spots.Add(135, "Existence");
            spots.Add(136, "Existence");
            spots.Add(138, "Existence");
            spots.Add(143, "Existence");
            spots.Add(145, "Existence");
            spots.Add(146, "Existence");
            */

            //B3 Building
            spots.Add(0,"");
            spots.Add(1,"experience room");
            spots.Add(2,"b3 building lobby");
            spots.Add(3,"b3 building lobby");
            spots.Add(4,"b3 building lobby");
            spots.Add(5,"b3 building lobby");
            spots.Add(6,"b3 building lobby");
            spots.Add(7,"b3 building lobby");
            spots.Add(8,"b3 building lobby");
            spots.Add(9,"b3 building lobby");
            spots.Add(10,"b3 building lobby");
            spots.Add(11,"b3 building lobby");
            spots.Add(12,"seating area");
            spots.Add(13,"ground floor desk 1");
            spots.Add(14,"ground floor desk 2");
            spots.Add(15,"ground floor desk 3");
            spots.Add(16,"ground floor desk 4");
            spots.Add(17,"hospitality desk");
            spots.Add(18,"hospitality desk");
            spots.Add(19,"ground floor meeting room");
            spots.Add(20,"ground floor meeting room");
            spots.Add(21,"ground floor meeting room");
            spots.Add(22,"ground floor meeting room");
            spots.Add(23,"experience room");
            spots.Add(24,"ground floor meeting room");
            spots.Add(25,"ground floor meeting room");
            spots.Add(26,"experience Room");
            spots.Add(27,"experience Room");
            spots.Add(28,"microsoft 1st floor meeting room 1");
            spots.Add(29,"microsoft 1st floor meeting room 2");
            spots.Add(30,"microsoft 1st floor seating area");
            spots.Add(31,"experience room");
            spots.Add(32,"first floor desk 1");
            spots.Add(33,"first floor desk 2");
            spots.Add(34,"first floor desk 3");
            spots.Add(35,"first floor desk 4");
            spots.Add(36,"first floor desk 5");
            return spots;
        }
    }
}



