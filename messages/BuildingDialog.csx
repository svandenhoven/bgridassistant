#r "Newtonsoft.Json"
#r "System.Drawing"
#load "Models.csx"

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;


// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BuildingDialog : LuisDialog<object>
{
    // todo: make smarter memory,  wih short & longterm memory
    protected Hashtable memory = new Hashtable();
    protected string _lightSwitchState = "";
    protected string _lightIntensity = "";
    protected Building _settings;

    public BuildingDialog() : base(new LuisService(new LuisModelAttribute(
        ConfigurationManager.AppSettings["LuisAppId"],
        ConfigurationManager.AppSettings["LuisAPIKey"],
        domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
    {
        memory.Add("lightState", "");
        memory.Add("lightIntensity", "");
        memory.Add("lastDevice", "");
        memory.Add("lastAsset", "");

        //Read Settings from Azure Blob
        var settingFileName = ConfigurationManager.AppSettings["SettingsFile"];
        var webClient = new WebClient();
        var settingJson = webClient.DownloadString(settingFileName);
        _settings = JsonConvert.DeserializeObject<Building>(settingJson);
    }

    [LuisIntent("GetDeskInfo")]
    public async Task GetDeskInfo(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        var gotDesk = result.TryFindEntity("Device", out deskEntity);
        var msg = "";

        if (gotDesk)
        { 
            var deskId = deskEntity.Entity;
            deskId = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(deskId)).First().bGridId.ToString();
            await GetDeskOccupancy(context, deskId);
        }
        else
        {
            EntityRecommendation entity;
            var gotEntity = result.TryFindEntity("Action", out entity);
            var action = "";
            if (gotEntity)
            {
                action = entity.Entity;
            }
            else
            {
                action = "work";
            }

            msg = await GetOfficeOccupancy(action);
            await context.SayAsync(msg, msg);
        }
    }

    [LuisIntent("FindAsset")]
    public async Task FindAsset(IDialogContext context, LuisResult result)
    {
        EntityRecommendation entity;
        var gotEntity = result.TryFindEntity("Asset", out entity);

        if (gotEntity)
        {
            var entityValue = entity.Entity;
            var msg = await FindAsset(entityValue);
            await context.SayAsync(msg, msg);

        }
        else
        {
            if (memory["lastAsset"].ToString() != "")
            {
                var msg = await FindAsset(memory["lastAsset"].ToString());
                await context.SayAsync(msg, msg);
            }
            else
            {
                var promptText = "Did not heard a know asset, which asset do you want to find?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeFindAfterAssetClarification);
            }
        }
    }

    [LuisIntent("Hospitality")]
    public async Task HospitalityIntent(IDialogContext context, LuisResult result)
    {
        EntityRecommendation entity;
        var hasProduct = result.TryFindEntity("Products", out entity);
        if(hasProduct)
        {
            var product = entity.Entity;
            var msg = $"I cannot order {product} for you yet. This could be implemented by sending message to hospitality to deliver {product} in your location.";
            await context.SayAsync(msg, msg);
        }
        else
        {
            var msg = $"Did not understand what you want to order. Can you please order again?";
            await context.SayAsync(msg, msg);
        }
    }

    [LuisIntent("GetTemperature")]
    public async Task GetTemperature(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        var gotDesk = result.TryFindEntity("Device", out deskEntity);

        if (gotDesk)
        {
            var deskId = deskEntity.Entity;
            var desk = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(deskId));
            if (desk.Count() > 0)
            {
                deskId = desk.First().bGridId.ToString();
                var msg = await GetTemperature(deskId);
                await context.SayAsync(msg, msg);
            }
            else
            {
                var promptText = "Did not understand the location, For which room do you want to know temperature?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeGetTemperatureAfterOrderDeskClarification);
            }
        }
        else
        {
            var promptText = "For which room do you want to know temperature?";
            var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
            var prompt = new PromptDialog.PromptString(promptOption);
            context.Call<string>(prompt, this.ResumeGetTemperatureAfterOrderDeskClarification);
        }
    }


    [LuisIntent("LightSwitch")]
    public async Task LightSwitch(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        EntityRecommendation lightStateEntity;

        var gotDevice = result.TryFindEntity("Device", out deskEntity);
        var gotLightState = result.TryFindEntity("LightStates", out lightStateEntity);

        var lightId = _settings.bGridDefaultIsland;

        //if (GetUserEmail(context) != "")
        //{
        //    var assistant = _settings.Assistants.Where(a => a.DeviceAccount == GetUserEmail(context)).First();
        //    if (assistant != null)
        //    {
        //        var node = _settings.BGridNodes.Where(n => n.Type == "island" && n.Name == assistant.RoomName).First();
        //        if(node != null)
        //        {
        //            lightId = node.bGridId.ToString();
        //        }
        //    }
        //}


        var activity = context.Activity;

        if(gotDevice)
        {
            lightId = deskEntity.Entity;
        }

        if (gotLightState)
        {
            var lightState = lightStateEntity.Entity;
            var msg = await SetLight(lightId, lightState);
            await context.SayAsync(msg, msg);

        }
        else
        {
            if (gotLightState)
            {
                _lightSwitchState = lightStateEntity.Entity;
                var promptText = $"Which light do you want to switch {_lightSwitchState}?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeLightSwitchAfterOrderDeskClarification);
            }
            else
            {
                var msg = "Please mention to switch light on or off";
                await context.SayAsync(msg, msg);
            }

        }
    }

    [LuisIntent("DimLight")]
    public async Task DimLight(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        EntityRecommendation lightIntensityEntity;

        var gotDevice = result.TryFindEntity("Device", out deskEntity);
        var gotlightIntensity = result.TryFindEntity("LightIntensity", out lightIntensityEntity);

        var lightId = _settings.bGridDefaultIsland; 
        if (gotDevice)
        {
            lightId = deskEntity.Entity;
        }

        var lightIntensity = "25";
        var msg = await SetlightIntensity(lightId, lightIntensity);
        await context.SayAsync(msg, msg);
    }

    [LuisIntent("Parking")]
    public async Task ParkingIntent(IDialogContext context, LuisResult result)
    {
        var msg = await GetParking();
        await context.SayAsync(msg, msg);
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        var msg = "No clear what you mean. Can you rephrase?";
        await context.SayAsync(msg, msg);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "Greeting" with the name of your newly created intent in the following handler
    [LuisIntent("Greeting")]
    public async Task GreetingIntent(IDialogContext context, LuisResult result)
    {
        var msg = "Hi. This is building bot. You can ask temperature, occupancy and switch lights. For Example say 'Ask Building Where can I work?'";
        await context.SayAsync(msg, msg);
    }

    [LuisIntent("Cancel")]
    public async Task CancelIntent(IDialogContext context, LuisResult result)
    {
        var msg = "This cancel action is not implemented";
        await context.SayAsync(msg, msg);
    }

    [LuisIntent("Help")]
    public async Task HelpIntent(IDialogContext context, LuisResult result)
    {
        var msg = "This is building bot. You can ask temperature, occupancy and switch lights.";
        await context.SayAsync(msg, msg);
    }


    //private string GetUserEmail(IDialogContext context)
    //{
    //    var userInfo = context.Activity.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
    //    var userEmail = "";
    //    if (userInfo != null)
    //    {
    //        var userInfoTxt = JsonConvert.SerializeObject(userInfo);
    //        userEmail = userInfo.Properties.Value<string>("email");
    //    }
    //    return userEmail;
    //}

    private async Task ShowLuisResult(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
        context.Wait(MessageReceived);
    }

    private HttpClient GetHttpClient()
    {
        var endpoint = _settings.bGridEndPoint;
        var user = _settings.bGridUser;
        var pw = _settings.bGridPassword;

        var bGridClient = new HttpClient()
        {
            BaseAddress = new Uri(endpoint)
        };
        var byteArray = Encoding.ASCII.GetBytes($"{user}:{pw}");
        bGridClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        return bGridClient;
    }

    private List<bGridRectangles> CreateSpots()
    {
        var scale = 1; //Todo: Add to config
        var spots = new List<bGridRectangles>();
        for(var x = -1; x<=11; x++)
        {
            for(var y=-1; y<=26; y++)
            {
                spots.Add(new bGridRectangles {
                    Name = x.ToString() + ", " + y.ToString(),
                    Spot = new bGridRectangle(x* scale, y* scale, (x+1)* scale, (y+1)* scale)
                });
            }
        }    

        return spots;
    }

    private string FindSpot(double x, double y)
    {
        var spotName = "";
        var spots = CreateSpots();
        foreach(var s in spots)
        {
            if (x >= s.Spot.X1 && x < s.Spot.X2 && y >= s.Spot.Y1 && y < s.Spot.Y2)
            {
                spotName = s.Name;
                break;
            }
        }

        return spotName;
    }

    private async Task<string> FindAsset(string assetId)
    {
        //Write desk to memory for future use.
        if (memory.ContainsKey("lastAsset"))
            memory["lastAsset"] = assetId;
        else
            memory.Add("lastAsset", assetId);

        var msg = "";
        var asset = await ExecuteAction<bGridAsset>($"/api/assets/{assetId}");
        if(asset != null)
        {
            var x = Convert.ToInt32(asset.x) + 91.5;
            var y = 36.5 - Convert.ToInt32(asset.y);
            
            var spot = FindSpot(x,y);

            if (spot != "")
                msg = $"Asset {assetId} can be found at square {spot}.";
            else
                msg = $"Asset {assetId} can be found at coordinate {x.ToString()}, {y.ToString()}.";
        }
        else
        {
            msg = $"Cannot find location of asset {assetId}";
        }
        return msg;
    }

    private async Task<string> GetTemperature(string deskId)
    {
        //Write desk to memory for future use.
        if (memory.ContainsKey("lastDevice"))
            memory.Remove("lastDevice");
        else
            memory.Add("lastDevice", deskId);

        var deskNum = Convert.ToInt32(deskId);
        var deskName = _settings.BGridNodes.Where(n => n.bGridId == deskNum).First().Name;

        var bGridClient = GetHttpClient();
        var tempResponse = await bGridClient.GetAsync($"api/locations/{deskId}/temperature");
        if (tempResponse.IsSuccessStatusCode)
        {
            var json = await tempResponse.Content.ReadAsStringAsync();
            var tempInfo = JsonConvert.DeserializeObject<List<bGridTemperature>>(json);
            if (tempInfo.Count == 0)
            {

                return $"I do not have information on {deskName} for you.";
            }
            else
            {
                var temp = tempInfo.Last();
                var msg =  $"The temperature in {deskName} is {Math.Round(temp.value, 0, MidpointRounding.AwayFromZero)} degrees celcius.";
                return msg;
            }
        }
        else
        {
            return $"Could not retrieve temperature for {deskName}.";
        }
    }

    private async Task<int> GetWeather()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://mindparkfacilityapi.azurewebsites.net/")
        };

        var response = await client.GetAsync("Api/weather?city=amsterdam");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(json);
            return weatherInfo.clouds.all;

        }
        else
        {
            return 100;
        }

    }

    private async Task<string> GetParking()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("http://mindparkfacilityapi.azurewebsites.net/")
        };

        var response = await client.GetAsync("api/parking/1");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var parkingInfo = JsonConvert.DeserializeObject<ParkingSpots>(json);
            var msg = $"There are {parkingInfo.Current} spots available and that is {parkingInfo.TrendString}.";
            msg += (parkingInfo.RemainingMinutes > 60) ? " It takes more than one hour to fill." : $" It will take {parkingInfo.RemainingMinutes} minutes to fill.";
            return msg;

        }
        else
        {
            return "I am not able to get parking information.";
        }

    }

    private async Task GetDeskOccupancy(IDialogContext context, string deskId)
    {
        //Write desk to memory for future use.
        if (memory.ContainsKey("lastDevice"))
            memory["lastDevice"] = deskId;
        else
            memory.Add("lastDevice", deskId);

        var bGridClient = GetHttpClient();
        var occupancyResponse = await bGridClient.GetAsync($"/api/occupancy/office/{deskId}");
        if (occupancyResponse.IsSuccessStatusCode)
        {
            var deskNum = Convert.ToInt32(deskId);
            var deskName = _settings.BGridNodes.Where(n => n.bGridId == deskNum).First().Name;

            var json = await occupancyResponse.Content.ReadAsStringAsync();
            var occupancyInfo = JsonConvert.DeserializeObject<bGridOccpancy>(json);
            if (occupancyInfo == null)
            {
                var msg = $"I do not have information on {deskName} for you.";
                await context.SayAsync(msg, msg); 
            }
            else
            {
                var msg = (occupancyInfo.value != 1) ? $"Yes, {deskName} is available." : $"No, {deskName} is not available.";
                //await context.SayAsync(msg, msg);
                var promptText = msg + $" Do you want more information of {deskName}?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeGetTemperatureAfterMoreInfoConfirmation);
            }
        }
        else
        {
            var msg = $"Could not retrieve occupancy for {deskId}.";
            await context.SayAsync(msg, msg);
        }
    }

    private async Task<string> GetOfficeOccupancy(string actionType)
    {
        var nodetype = actionType == "work" ? "desk" : "room";

        var workplaces = _settings.BGridNodes.Where(n => n.Type == nodetype).Select(n => n.bGridId).ToArray();
        var msg = "";
        var desks = await ExecuteAction<List<bGridOccpancy>>($"/api/occupancy/office/");

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
                var availableNodes = desks.Where(d => workplaces.Contains(d.location_id)).ToList();
                var availableRoomNames = _settings.BGridNodes; //.Where(n => availableNodes.Contains(n.bGridId)).ToList();
                var Nodes = from node in availableRoomNames
                            join desk in desks on node.bGridId equals desk.location_id
                            select new { Name = node.Name, Available = desk.value, Id = node.bGridId };

                var FreeRooms = from room in Nodes
                                group room by room.Name into roomNodes
                                where roomNodes.Where(n => n.Available == 2).Count() == 0
                                select roomNodes.Key;

                var uniqueRoomNames = FreeRooms.ToList().Distinct();
                if (uniqueRoomNames.Count() > 0)
                {
                    int i = 1;
                    msg += actionType == "work" ? "The workplace" : "The room";
                    msg += (uniqueRoomNames.Count() > 1) ? "s " : " ";
                    foreach (var spot in uniqueRoomNames)
                    {
                        msg += spot;
                        msg += (i == uniqueRoomNames.Count()-1) ? " and " : ", ";
                        i++;
                        if(i > 3 && uniqueRoomNames.Count() > 4)
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

    private async Task<T> ExecuteAction<T>(string action)
    {
        var bGridClient = GetHttpClient();
        var response = await bGridClient.GetAsync(action);

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonConvert.DeserializeObject<T>(jsonString);
            return jsonObject;
        }
        else
        {
            return default(T);
        }
    }

    private async Task<string> SetLight(string islandId, string gotLightState)
    {
        var bGridClient = GetHttpClient();
        var json = "{ \"status\" : \"" + gotLightState + "\", \"return_address\":\"localhost\" }";

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var lightResponse = await bGridClient.PostAsync($"/api/islands/{islandId}/light/status", httpContent);
        if (lightResponse.IsSuccessStatusCode)
        {
            var roomName = islandId;
            var room = _settings.BGridNodes.Where(n => n.bGridId.ToString() == islandId);
            if(roomName != null)
                roomName = room.First().Name;

            return $"Lights of {roomName} are {gotLightState}.";

        }
        else
        {
            return $"Could not switch light for {islandId}";
        }
    }


    private async Task<string> SetlightIntensity(string islandId, string lightIntensity)
    {
        var bGridClient = GetHttpClient();
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
            var roomName = islandId;
            var room = _settings.BGridNodes.Where(n => n.bGridId.ToString() == islandId);
            if (roomName != null)
                roomName = room.First().Name;

            return $"Dimmed lights of {roomName} to {lightIntensity} procent.";
        }
        else
        {
            return $"Could not set lightIntensity for {islandId}";
        }
    }


        private async Task ResumeGetTemperatureAfterMoreInfoConfirmation(IDialogContext context, IAwaitable<string> result)
    {
        var confirm = await result;
        confirm = confirm.Replace(".", "");
        
        string[] answers = { "ok", "yes", "sure", "yeah" };
        if (answers.Contains(confirm.ToLower()))
        {
            var deskId = memory["lastDevice"].ToString();

            var msg = await GetTemperature(deskId);
            await context.SayAsync(msg, msg);

            await context.SayAsync("Getting weather info for you.", "Getting weather info for you.");
            var cloudLevel = await GetWeather();
            if (cloudLevel < 40)
                msg = $" It will be sunny today, so might get warm in afternoon.";
            else
                msg = $" It will not be very sunny today, so will stay cool.";
            await context.SayAsync(msg, msg);
        }
        else
        {
            var msg = "Ok, just ask if you need more info.";
            await context.SayAsync(msg, msg);
        }
    }

    private async Task ResumeGetTemperatureAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var deskId = await result;
        deskId = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(deskId)).First().bGridId.ToString();

        var msg = await GetTemperature(deskId);
        await context.SayAsync(msg, msg);

        await context.SayAsync("Getting weather info for you.", "Getting weather info for you.");
        var cloudLevel = await GetWeather();
        if (cloudLevel < 40)
            msg = $" It will be sunny today, so might get warm in afternoon.";
        else
            msg = $" It will not be very sunny today, so will stay cool.";
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeLightSwitchAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var islandId = await result;
        islandId = RemoveNonCharactersAndSpace(islandId);
        var msg = await SetLight(islandId, _lightSwitchState);
        await context.SayAsync(msg, msg);
    }

    //
    private async Task ResumeDimLightAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var islandId = await result;
        islandId = RemoveNonCharactersAndSpace(islandId);
        var msg = await SetlightIntensity(islandId, _lightIntensity);
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeFindAfterAssetClarification(IDialogContext context, IAwaitable<string> result)
    {
        var assetId = await result;
        assetId = RemoveNonCharactersAndSpace(assetId);
        var msg = await FindAsset(assetId);
        await context.SayAsync(msg, msg);
    }

    private string RemoveNonCharactersAndSpace(string input)
    {
        //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        //return rgx.Replace(input, "");
        if(input[input.Length-1] == '.')
        {
            return input.Substring(0, input.Length - 2).Replace(" ", "").ToLower();
        }
        else
        {
            return input.Replace(" ","").ToLower();
        }
    }

}