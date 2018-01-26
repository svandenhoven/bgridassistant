#r "Newtonsoft.Json"
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

    public BuildingDialog() : base(new LuisService(new LuisModelAttribute(
    "bd1b92d3-076a-46c7-a1bb-2e10d9962f40",
    "8248c94c11e743c58e29411c0219734d",
    domain: "westeurope.api.cognitive.microsoft.com")))
    {
    }

    [LuisIntent("GetTemperature")]
    public async Task GetTemperature(IDialogContext context, LuisResult result)
    {
        EntityRecommendation roomEntity;

        var gotRoom = result.TryFindEntity("Room", out roomEntity);

        var roomId = "0";
        if (gotRoom)
        {
            roomId = roomEntity.Entity;
            var msg = await GetTemperature(roomId);
            await context.SayAsync(msg, msg);

        }
        else
        {
            var promptText = "For which room do you want to know temperature?";
            var promptOption = new PromptOptions<string>(promptText, null, speak: promptText);
            var prompt = new PromptDialog.PromptString(promptOption);
            context.Call<string>(prompt, this.ResumeAfterOrderRoomClarification);
        }
    }

    private async Task<string> GetTemperature(string roomId)
    {
        var bGridClient = GetHttpClient();

        var tempResponse = await bGridClient.GetAsync($"api/locations/{roomId}/temperature");
        if (tempResponse.IsSuccessStatusCode)
        {
            var json = await tempResponse.Content.ReadAsStringAsync();
            var tempInfo = JsonConvert.DeserializeObject<List<bGridTemperature>>(json);
            var temp = tempInfo.Last();
            return $"The temperature in {roomId} is {Math.Round(temp.value, 0, MidpointRounding.AwayFromZero)} degrees celcius.";         
        }
        else
        {
            return $"Could not retrieve temperature.";
        }
    }

    private async Task ResumeAfterOrderRoomClarification(IDialogContext context, IAwaitable<string> result)
    {
        var roomId = await result;
        var msg = await GetTemperature(roomId);
        await context.SayAsync(msg, msg);
    }

    [LuisIntent("None")]
    public async Task NoneIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    // Go to https://luis.ai and create a new intent, then train/publish your luis app.
    // Finally replace "Greeting" with the name of your newly created intent in the following handler
    [LuisIntent("Greeting")]
    public async Task GreetingIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    [LuisIntent("Cancel")]
    public async Task CancelIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
    }

    [LuisIntent("Help")]
    public async Task HelpIntent(IDialogContext context, LuisResult result)
    {
        await this.ShowLuisResult(context, result);
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
}