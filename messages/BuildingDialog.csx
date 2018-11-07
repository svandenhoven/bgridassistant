#r "Newtonsoft.Json"
#r "System.Drawing"
#load "Models.csx"
#load "AssetHelper.csx"
#load "LightsHelper.csx"
#load "RoomHelper.csx"

using System;
using System.Net;
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

            msg = await new RoomHelper(_settings).GetOfficeOccupancy(action);
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
            var assetName = entity.Entity;
            var assetHelper = new AssetHelper(_settings);
            var msg = await assetHelper.GetAssetLocation(assetName);
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
            var desks = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(deskId));
            if (desks.Count() > 0)
            {
                deskId = desks.First().bGridId.ToString();
                var msg = await new RoomHelper(_settings).GetTemperature(deskId);
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
            //var promptText = "For which room do you want to know temperature?";
            //var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
            //var prompt = new PromptDialog.PromptString(promptOption);
            //context.Call<string>(prompt, this.ResumeGetTemperatureAfterOrderDeskClarification);
            var deskId = _settings.bGridDefaultRoom;
            var desk = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.RoomName) == RemoveNonCharactersAndSpace(deskId));
            if (desk.Count() > 0)
            {
                deskId = desk.First().bGridId.ToString();
                var msg = await new RoomHelper(_settings).GetTemperature(deskId);
                await context.SayAsync(msg, msg);
            }
        }
    }


    [LuisIntent("LightSwitch")]
    public async Task LightSwitch(IDialogContext context, LuisResult result)
    {
        EntityRecommendation lightEntity;
        EntityRecommendation lightStateEntity;

        var gotDevice = result.TryFindEntity("Device", out lightEntity);
        var gotLightState = result.TryFindEntity("LightStates", out lightStateEntity);

        var lightId = _settings.bGridDefaultRoom;

        #region
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
        #endregion

        if (!gotLightState)
        {
            var msg = "Please mention to switch light on or off";
            await context.SayAsync(msg, msg);
        }
        else
        {
            var lightState = lightStateEntity.Entity;
            if (gotDevice)
            {
                var roomname = lightEntity.Entity;
                var msg = await new LightsHelper(_settings).SwitchLights(roomname, lightState);
                await context.SayAsync(msg, msg);
            }
            else
            {
                _lightSwitchState = lightStateEntity.Entity;
                var promptText = $"In which room do you want to switch light {_lightSwitchState}?";
                var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
                var prompt = new PromptDialog.PromptString(promptOption);
                context.Call<string>(prompt, this.ResumeLightSwitchAfterOrderDeskClarification);
            }
        }
    }

    [LuisIntent("DimLight")]
    public async Task DimLight(IDialogContext context, LuisResult result)
    {
        EntityRecommendation nodeEntity;
        EntityRecommendation lightIntensityEntity;

        var gotDevice = result.TryFindEntity("Device", out nodeEntity);
        var gotlightIntensity = result.TryFindEntity("LightIntensity", out lightIntensityEntity);

        var roomname = _settings.bGridDefaultRoom; 
        if (gotDevice)
        {
            roomname = nodeEntity.Entity;
        }

        var lightIntensity = "25";
        var msg = await new LightsHelper(_settings).SetlightIntensity(roomname, lightIntensity);
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

    private async Task ResumeGetTemperatureAfterMoreInfoConfirmation(IDialogContext context, IAwaitable<string> result)
    {
        var confirm = await result;
        confirm = confirm.Replace(".", "");
        
        string[] answers = { "ok", "yes", "sure", "yeah" };
        if (answers.Contains(confirm.ToLower()))
        {
            var deskId = memory["lastDevice"].ToString();

            var msg = await new RoomHelper(_settings).GetTemperature(deskId);
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
        var desk = _settings.BGridNodes.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(deskId));
        if (desk.Count() > 0)
        {
            deskId = desk.First().bGridId.ToString();

            var msg = await new RoomHelper(_settings).GetTemperature(deskId);
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
            var msg = $"I do not know {deskId}.";
            await context.SayAsync(msg, msg);
        }

    }

    private async Task ResumeLightSwitchAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var roomname = await result;
        var msg = await new LightsHelper(_settings).SwitchLights(roomname, _lightSwitchState);
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeDimLightAfterOrderDeskClarification(IDialogContext context, IAwaitable<string> result)
    {
        var roomname = await result;
        var msg = await new LightsHelper(_settings).SetlightIntensity(roomname, _lightIntensity);
        await context.SayAsync(msg, msg);
    }

    private async Task ResumeFindAfterAssetClarification(IDialogContext context, IAwaitable<string> result)
    {
        var assetName = await result;
        var assetHelper = new AssetHelper(_settings);
        var msg = await assetHelper.GetAssetLocation(assetName);
        await context.SayAsync(msg, msg);
    }

    private async Task GetDeskOccupancy(IDialogContext context, string deskId)
    {
        //Write desk to memory for future use.
        if (memory.ContainsKey("lastDevice"))
            memory["lastDevice"] = deskId;
        else
            memory.Add("lastDevice", deskId);

        var occupancyInfo = await new ActionHelper(_settings).ExecuteGetAction<bGridOccpancy>($"/api/occupancy/office/{deskId}");
        if (occupancyInfo != null)
        {
            var deskNum = Convert.ToInt32(deskId);
            var deskName = _settings.BGridNodes.Where(n => n.bGridId == deskNum).First().Name;

            var msg = (occupancyInfo.value != 1) ? $"Yes, {deskName} is available." : $"No, {deskName} is not available.";
            //await context.SayAsync(msg, msg);
            var promptText = msg + $" Do you want more information of {deskName}?";
            var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
            var prompt = new PromptDialog.PromptString(promptOption);
            context.Call<string>(prompt, this.ResumeGetTemperatureAfterMoreInfoConfirmation);
        }
        else
        {
            var msg = $"Could not retrieve occupancy for {deskId}.";
            await context.SayAsync(msg, msg);
        }
    }

    public string RemoveNonCharactersAndSpace(string input)
    {
        //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
        //return rgx.Replace(input, "");
        if(input[input.Length-1] == '.')
        {
            return input.Replace(" ", "").Replace(".","").ToLower();
        }
        else
        {
            return input.Replace(" ","").ToLower();
        }
    }

}