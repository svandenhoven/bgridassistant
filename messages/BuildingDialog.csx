#r "Newtonsoft.Json"
#r "System.Drawing"
#load "Models.csx"

using System;
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
    //public BuildingDialog() : base(new LuisService(new LuisModelAttribute(
    //    ConfigurationManager.AppSettings["LuisAppId"], 
    //    ConfigurationManager.AppSettings["LuisAPIKey"], 
    //    domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
    //{
    //}

    // todo: make smarter memory,  wih short & longterm memory
    protected Hashtable memory = new Hashtable();
    protected string _lightSwitchState = "";
    protected string _lightIntensity = "";

    public BuildingDialog() : base(new LuisService(new LuisModelAttribute(
    "bd1b92d3-076a-46c7-a1bb-2e10d9962f40",
    "8248c94c11e743c58e29411c0219734d",
    domain: "westeurope.api.cognitive.microsoft.com")))
    {
        memory.Add("lightState", "");
        memory.Add("lightIntensity", "");
        memory.Add("lastDevice", "");
        memory.Add("lastAsset", "");
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
            await GetDeskOccupancy(context, deskId);
        }
        else
        {
            msg = await GetOfficeOccupancy();
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
                var promptText = "Which asset do you want to find?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeFindAfterAssetClarification);
            }
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
            var msg = await GetTemperature(deskId);
            await context.SayAsync(msg, msg);

        }
        else
        {
            if (memory["lastDevice"].ToString() != "")
            {
                var msg = await GetTemperature(memory["lastDevice"].ToString());
                await context.SayAsync(msg, msg);
            }
            else
            {
                var promptText = "For which desk do you want to know temperature?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeGetTemperatureAfterOrderDeskClarification);
            }
        }
    }


    [LuisIntent("LightSwitch")]
    public async Task LightSwitch(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        EntityRecommendation lightStateEntity;

        var gotDevice = result.TryFindEntity("Device", out deskEntity);
        var gotLightState = result.TryFindEntity("LightStates", out lightStateEntity);

        if (gotDevice && gotLightState)
        {
            var lightId = deskEntity.Entity;
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

        }
    }

    [LuisIntent("DimLight")]
    public async Task DimLight(IDialogContext context, LuisResult result)
    {
        EntityRecommendation deskEntity;
        EntityRecommendation lightIntensityEntity;

        var gotDevice = result.TryFindEntity("Device", out deskEntity);
        var gotlightIntensity = result.TryFindEntity("LightIntensity", out lightIntensityEntity);

        if (gotDevice && gotlightIntensity)
        {
            var lightId = deskEntity.Entity;
            var lightIntensity = lightIntensityEntity.Entity;
            var msg = await SetlightIntensity(lightId, lightIntensity);
            await context.SayAsync(msg, msg);

        }
        else
        {
            if (gotlightIntensity)
            {
                _lightIntensity = lightIntensityEntity.Entity;
                var promptText = $"For which light do you want to switch intensity to {_lightIntensity} procent?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeDimLightAfterOrderDeskClarification);
            }

        }
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
        var msg = "Hi. This is building bot. You can ask temperature, availability and switch lights.";
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
        var msg = "This is building bot. You can ask temperature, availability and switch lights.";
        await context.SayAsync(msg, msg);
    }

    private async Task ShowLuisResult(IDialogContext context, LuisResult result)
    {
        await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
        context.Wait(MessageReceived);
    }

    private HttpClient GetHttpClient()
    {
        var bGridClient = new HttpClient()
        {
            BaseAddress = new Uri("https://wsn-demo.evalan.com:8443")
        };
        var byteArray = Encoding.ASCII.GetBytes("demo_set_1:Hp3B9E71b44DbJ2G9kxE");
        bGridClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        return bGridClient;
    }

    private List<bGridRectangles> CreateSpots()
    {
        var scale = 1; //Todo: Add to config
        var spots = new List<bGridRectangles>();
        for(var x = -1; x<=10; x++)
        {
            for(var y=-1; y<=20; y++)
            {
                spots.Add(new bGridRectangles {
                    Name = ((char)(65+x)).ToString() + y.ToString(),
                    Spot = new bGridRectangle(x* scale, y* scale, (x+1)* scale, (y+1)* scale)
                });
            }
        }    

        return spots;
    }

    private string FindSpot(Point pt)
    {
        var spotName = "";
        var spots = CreateSpots();
        foreach(var s in spots)
        {
            if (pt.X >= s.Spot.X1 && pt.X < s.Spot.X2 && pt.Y >= s.Spot.Y1 && pt.Y < s.Spot.Y2)
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
            Point pt = new Point(Convert.ToInt32(asset.x), Convert.ToInt32(asset.y));
            var spot = FindSpot(pt);

            if (spot != "")
                msg = $"Asset {assetId} can be found at in square {spot} ({asset.x.ToString()},{asset.y.ToString()}).";
            else
                msg = $"Asset {assetId} can be found at {asset.x.ToString()},{asset.y.ToString()}.";
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
            memory["lastDevice"] = deskId;
        else
            memory.Add("lastDevice", deskId);

        var bGridClient = GetHttpClient();
        var tempResponse = await bGridClient.GetAsync($"api/locations/{deskId}/temperature");
        if (tempResponse.IsSuccessStatusCode)
        {
            var json = await tempResponse.Content.ReadAsStringAsync();
            var tempInfo = JsonConvert.DeserializeObject<List<bGridTemperature>>(json);
            if (tempInfo.Count == 0)
            {
                return $"I do not have information on desk {deskId} for you.";
            }
            else
            {
                var temp = tempInfo.Last();
                return $"The temperature in {deskId} is {Math.Round(temp.value, 0, MidpointRounding.AwayFromZero)} degrees celcius.";
            }
        }
        else
        {
            return $"Could not retrieve temperature for {deskId}.";
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
            var json = await occupancyResponse.Content.ReadAsStringAsync();
            var occupancyInfo = JsonConvert.DeserializeObject<bGridOccpancy>(json);
            if (occupancyInfo == null)
            {
                var msg = $"I do not have information on desk {deskId} for you.";
                await context.SayAsync(msg, msg); 
            }
            else
            {
                var msg = (occupancyInfo.value == 0) ? $"Yes, desk {deskId} is available." : $"No, desk {deskId} is not available.";
                //await context.SayAsync(msg, msg);
                var promptText = msg + $" Do you want more information of desk {deskId}?";
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

    private async Task<string> GetOfficeOccupancy()
    {
        var msg = "";
        var desks = await ExecuteAction<List<bGridOccpancy>>($"/api/occupancy/office/");

        if (desks == null)
            msg = "I could not retrieve available desks.";
        else
        {
            if (desks.Count == 0)
            {
                msg = "I could not find any recent information on desks";
            }
            else
            {
                int[] workplaces = { 416, 417, 1007, 1008, 1009, 1010, 1011 };
                var availableDesks = desks.Where(d => d.value == 0 && workplaces.Contains(d.location_id));
                if (availableDesks.Count() > 0)
                {
                    int i = 1;
                    msg += "Desk";
                    msg += (availableDesks.Count() > 1) ? "s " : " ";
                    foreach (var desk in availableDesks)
                    {
                        msg += desk.location_id;
                        msg += (i < availableDesks.Count()) ? " and " : " ";
                        i++;
                    }
                    msg += (availableDesks.Count() > 1) ? "are " : "is";
                    msg += " available.";

                    //msg += await GetTemperature(availableDesks.Last().location_id.ToString());
                }
                else
                    msg = "No desks are available.";
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
            return $"The light of {islandId} is {gotLightState}.";

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
            return $"Set lightIntensity of {islandId} to {lightIntensity} procent.";

        }
        else
        {
            return $"Could not set lightIntensity for {islandId}";
        }
    }



    private async Task ResumeGetTemperatureAfterMoreInfoConfirmation(IDialogContext context, IAwaitable<string> result)
    {
        var confirm = await result;
        string[] answers = { "ok", "yes", "sure" };
        if (answers.Contains(confirm.ToLower()))
        {
            var deskId = memory["lastDevice"].ToString();

            var msg = await GetTemperature(deskId);
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
        deskId = RemoveNonCharacters(deskId);

        var msg = await GetTemperature(deskId);
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeLightSwitchAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var islandId = await result;
        islandId = RemoveNonCharacters(islandId);
        var msg = await SetLight(islandId, _lightSwitchState);
        await context.SayAsync(msg, msg);
    }

    //
    private async Task ResumeDimLightAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var islandId = await result;
        islandId = RemoveNonCharacters(islandId);
        var msg = await SetlightIntensity(islandId, _lightIntensity);
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeFindAfterAssetClarification(IDialogContext context, IAwaitable<string> result)
    {
        var assetId = await result;
        assetId = RemoveNonCharacters(assetId);
        var msg = await FindAsset(assetId);
        await context.SayAsync(msg, msg);
    }

    private string RemoveNonCharacters(string input)
    {
        Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        return rgx.Replace(input, "");
    }

}