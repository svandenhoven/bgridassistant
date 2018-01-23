#r "Newtonsoft.Json"
#load "Models.csx"

using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
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

    private async Task<string> SwitchLights(string state)
    {
        var bGridClient = GetHttpClient();
        var json = "{ \"status\" : \"" + state + "\", \"return_address\":\"localhost\" }";
        var ob = new
        {
            status = "on",
            return_address = "localhost"
        };

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var lightResponse = await bGridClient.PostAsync("/api/islands/101/light/status", httpContent);
        if (lightResponse.IsSuccessStatusCode)
        {
            return $"The light is {state}.";
            
        }
        else
        {
            return $"Could not switch light";
        }
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        if (message.Text.ToLower() == "reset")
        {

            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else
        {
            var msg = "";
            switch (message.Text.ToLower())
            {
                case "what is temperature":
                    var bGridClient = GetHttpClient();

                    var tempResponse = await bGridClient.GetAsync("api/locations/416/temperature");
                    if (tempResponse.IsSuccessStatusCode)
                    {
                        var json = await tempResponse.Content.ReadAsStringAsync();
                        var tempInfo = JsonConvert.DeserializeObject<List<bGridTemperature>>(json);
                        var temp = tempInfo.Last();
                        msg = $"The temperature is {Math.Round(temp.value,0, MidpointRounding.AwayFromZero)} degrees celcius.";
                        await context.SayAsync(msg, msg);
                    }
                    else
                    {
                        msg = $"Could not retrieve temperature.";
                        await context.SayAsync(msg, msg);
                    }
                    break;

                case "light on":
                    msg = await SwitchLights("on");
                    await context.SayAsync(msg, msg);

                    break;
                case "light off":
                    msg = await SwitchLights("off");
                    await context.SayAsync(msg, msg);
                    break;
                case "can i park":
                    var parkInfo = new ParkingStatus();

                    var client = new HttpClient()
                    {
                        BaseAddress = new Uri("http://mindparkfacilityapi.azurewebsites.net")
                    };
                    var response = await client.GetAsync($"api/parking/1");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        parkInfo = JsonConvert.DeserializeObject<ParkingStatus>(json);
                    }

                    if (parkInfo.Current > 0)
                    {
                        msg = $"You can park, there are {parkInfo.Current} places available.";
                        await context.SayAsync(msg, msg);
                    }
                    else
                    {
                        msg = $"Parking is full.";
                        await context.SayAsync(msg, msg);
                    }
                    break;
                default:
                    await context.PostAsync($"{this.count++}: You said {message.Text}");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }
}

