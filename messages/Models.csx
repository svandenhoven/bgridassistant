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


public class bGridLocations
{
    public int id { get; set; }
    public string name { get; set; }
    public int type { get; set; }
    public int floor { get; set; }
    public string building { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public int? island_id { get; set; }
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

[Serializable]
public class AssistantDevices
{
    public string Id { get; set; }
    public string RoomName { get; set; }
    public string DeviceAccount { get; set; }
}

[Serializable]
public class BGridNode
{
    public int bGridId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string RoomName { get; set; }
}

[Serializable]
public class BGridAsset
{
    public int AssetId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
}

[Serializable]
public class BGridRectangle
{
    public string Level { get; set; }
    public string Name { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

}

[Serializable]
public class Building
{
    public string bGridEndPoint { get; set; }
    public string bGridUser { get; set; }
    public string bGridPassword { get; set; }
    public string bGridDefaultIsland { get; set; }
    public string bGridDefaultRoom { get; set; }
    public List<string> AuthorizedUsers { get; set; }
    public List<BGridNode> BGridNodes { get; set; }
    public List<AssistantDevices> Assistants { get; set; }
    public List<BGridAsset> BGridAssets { get; set; }
    public List<BGridRectangle> Spots { get; set; }

}





