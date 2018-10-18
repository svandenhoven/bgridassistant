#load "AlexaRequest.csx"
#load "Models.csx"
#load "ActionHelper.csx"
#load "LightsHelper.csx"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;
using System.Net.Http.Headers;

public class AlexaProcessor
{
    protected Building _settings;

    public AlexaProcessor(Building settings)
    {
        _settings = settings;
    }

    public async Task<AlexaResponse> ProcessAlexaRequest(AlexaRequest request)
    {
        var msg = "";

        switch (request.request.intent.name)
        {
            case "LightOn":
                msg = await new LightsHelper(_settings).SwitchLights("2301", "on");
                break;
            case "LightOff":
                msg = await new LightsHelper(_settings).SwitchLights("2301", "off");
                break;
            case "DimLight":
                msg = await SetlightIntensity("2301", "25");
                break;
            default:
                break;
        }

        return CreateAlexaResponse(msg);
    }

    private AlexaResponse CreateAlexaResponse(string message)
    {
        return new AlexaResponse
        {
            version = "1.0",
            response = new Response
            {
                outputSpeech = new Outputspeech
                {
                    type = "PlainText",
                    text = message
                },
                shouldEndSession = true
            }
        };
    }

    private string SwitchLights(string islandId, string state)
    {
        var bGridClient = new ActionHelper(_settings).GetHttpClient();
        var json = "{ \"status\" : \"" + state + "\", \"return_address\":\"localhost\" }";
        var ob = new
        {
            status = "on",
            return_address = "localhost"
        };

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var lightResponse = bGridClient.PostAsync($"/api/islands/{islandId}/light/status", httpContent).Result;
        if (lightResponse.IsSuccessStatusCode)
        {
            return $"The light is {state}.";

        }
        else
        {
            return $"Could not switch light";
        }
    }

    private async Task<string> SetlightIntensity(string islandId, string lightIntensity)
    {
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
            return $"Dimmed lights of {islandId} to {lightIntensity} procent.";
        }
        else
        {
            return $"Could not set lightIntensity for {islandId}";
        }
    }
}
   