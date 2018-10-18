#r "Newtonsoft.Json"
#load "Models.csx"
#load "ActionHelper.csx"


using System;
using System.Text;
using System.Collections;
using Newtonsoft.Json;

public class LightsHelper
{
    private Building _settings;

    public LightsHelper(Building settings)
    {
        _settings = settings;
    }

    public async Task<string> SetAllLights(string roomName, string lightState)
    {
        var isLands = _settings.BGridNodes.Where(n => n.RoomName == roomName && n.Type == "island");
        foreach (var island in isLands)
        {
            await SetLight(island.bGridId.ToString(), lightState);
        }
        return $"Switch all lights {lightState}.";
    }

    public async Task<string> SetLight(string islandId, string lightState)
    {
        var bGridClient = new ActionHelper(_settings).GetHttpClient();
        var json = "{ \"status\" : \"" + lightState + "\", \"return_address\":\"localhost\" }";

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var lightResponse = await bGridClient.PostAsync($"/api/islands/{islandId}/light/status", httpContent);
        if (lightResponse.IsSuccessStatusCode)
        {
            var roomName = islandId;
            var room = _settings.BGridNodes.Where(n => n.bGridId.ToString() == islandId);
            if (roomName != null)
                roomName = room.First().RoomName;

            return $"Lights of {roomName} are {lightState}.";

        }
        else
        {
            return $"Could not switch light for {islandId}";
        }
    }

    public async Task<string> SwitchLights(string roomname, string lightState)
    {
        if (RemoveNonCharactersAndSpace(roomname) == RemoveNonCharactersAndSpace("all lights"))
        {
            var msg = await SetAllLights("room1", lightState);
            return msg;
        }
        else
        {
            var lights = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.RoomName) == RemoveNonCharactersAndSpace(roomname) && n.Type == "island");
            if (lights.Count() > 0)
            {
                var lightId = lights.First().bGridId.ToString();
                var msg = await SetLight(lightId, lightState);
                return msg;
            }
            else
            {
                var msg = $"I do not know {roomname}. Please use correct name for lights.";
                return msg;
            }
        }
    }

    public async Task<string> SetlightIntensity(string roomName, string lightIntensity)
    {
        var islandId = _settings.bGridDefaultIsland;
        var lights = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.RoomName) == RemoveNonCharactersAndSpace(roomName) && n.Type == "island");
        if (lights.Count() > 0)
        {
            islandId = lights.First().bGridId.ToString();
        }

        var bGridClient = new ActionHelper(_settings).GetHttpClient();
        var obj = new
        {
            intensity = int.Parse(lightIntensity),
            return_address = "localhost"
        };

        var json = JsonConvert.SerializeObject(obj);

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var lightResponse = await bGridClient.PostAsync($"/api/islands/{islandId}/light/intensity", httpContent);
        if (lightResponse.IsSuccessStatusCode)
        {
            return $"Dimmed lights of {roomName} to {lightIntensity} procent.";
        }
        else
        {
            return $"Could not set lightIntensity for {roomName}";
        }
    }

    private string RemoveNonCharactersAndSpace(string input)
    {
        //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        //return rgx.Replace(input, "");
        if (input[input.Length - 1] == '.')
        {
            return input.Replace(" ", "").Replace(".", "").ToLower();
        }
        else
        {
            return input.Replace(" ", "").ToLower();
        }
    }
}