#load "Models.csx"
#load "ActionHelper.csx"

using System;
using System.Collections;

public class RoomHelper
{
    private Building _settings;

    public RoomHelper(Building settings)
    {
        _settings = settings;
    }

    public async Task<string> GetOfficeOccupancy(string actionType)
    {
        var nodeType = actionType == "work" ? "desk" : "room";
        var msg = "";
        var locations = await new ActionHelper(_settings).ExecuteGetAction<List<bGridLocations>>("/api/locations");
        var desks = await new ActionHelper(_settings).ExecuteGetAction<List<bGridOccpancy>>("/api/occupancy/office/");

        if (desks == null)
            msg = "I could not retrieve availability information.";
        else
        {
            if (desks.Count == 0)
            {
                msg = "I could not find any recent information on desks";
            }
            else
            {
                var LocationOccupancies = from loc in locations
                                          join desk in desks on loc.id equals desk.location_id
                                          where loc.building == "Microsoft"
                                          select new { Id = loc.id, Occupancy = desk.value, Floor = loc.floor, Island = loc.island_id, X = loc.x, Y = loc.y, Z = loc.z };

                var Nodes = from node in _settings.BGridNodes
                            join loc in LocationOccupancies on node.bGridId equals loc.Id
                            where node.Type == nodeType
                            select new { RoomName = node.RoomName, Name = node.Name, Occupancy = loc.Occupancy, Id = node.bGridId, Floor = loc.Floor, Island = loc.Island, X = loc.X, Y = loc.Y, Z = loc.Z };

                var FreeRooms = from room in Nodes
                                group room by room.RoomName into roomNodes
                                where Nodes.Where(n => n.Occupancy == 2).Count() == 0
                                select roomNodes;

                var uniqueRoomNames = FreeRooms.Distinct();
                if (uniqueRoomNames.Count() > 0)
                {
                    int i = 1;
                    msg += actionType == "work" ? "The workplace" : "The room";
                    msg += (uniqueRoomNames.Count() > 1) ? "s " : " ";
                    foreach (var spot in uniqueRoomNames)
                    {
                        msg += spot.Key;
                        msg += (i == uniqueRoomNames.Count() - 1) ? " and " : ((i == 1) ? " " : ", ");
                        i++;
                        if (i > 3 && uniqueRoomNames.Count() > 4)
                        {
                            msg += $" and {uniqueRoomNames.Count() - i} more ";
                            break;
                        }
                    }
                    msg += (uniqueRoomNames.Count() > 1) ? "are " : "is";
                    msg += " available.";
                }
                else
                    msg = "No places are available.";
            }
        }
        return msg;

    }

    public async Task<string> GetTemperature(string deskId)
    {
        var deskNum = Convert.ToInt32(deskId);
        var deskName = _settings.BGridNodes.Where(n => n.bGridId == deskNum).First().Name;

        var tempInfo = await new ActionHelper(_settings).ExecuteGetAction<List<bGridTemperature>>($"api/locations/{deskId}/temperature");
        if (tempInfo.Count == 0)
        {

            return $"I do not have information on {deskName} for you.";
        }
        else
        {
            var temp = tempInfo.Last();
            var msg = $"The temperature in {deskName} is {Math.Round(temp.value, 0, MidpointRounding.AwayFromZero)} degrees celcius.";
            return msg;
        }
    }
}