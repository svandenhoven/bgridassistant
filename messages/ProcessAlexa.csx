#load "AlexaRequest.csx"
#load "Models.csx"
#load "ActionHelper.csx"
#load "LightsHelper.csx"
#load "AssetHelper.csx"

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
        var lightId = DefineLightId(request);
        var assetId = DefineAssetId(request);

        switch (request.request.intent.name)
        {
            case "LightOn":
                msg = await new LightsHelper(_settings).SwitchLights(lightId, "on");
                break;
            case "LightOff":
                msg = await new LightsHelper(_settings).SwitchLights(lightId, "off");
                break;
            case "DimLight":
                msg = await new LightsHelper(_settings).SetlightIntensity(lightId, "25");
                break;
            case "FindAsset":
                msg = await new AssetHelper(_settings).GetAssetLocation(assetId);
                break;
            default:
                break;
        }

        return CreateAlexaResponse(msg);
    }

    private string DefineLightId(AlexaRequest alexa)
    {
        if (alexa.request.intent.slots.roomId != null)
        {
            var roomId = alexa.request.intent.slots.roomId.value;
            if (roomId != null)
                return roomId;
            else
                return _settings.bGridDefaultRoom;
        }
        else
        {
            return "";
        }
    }

    private string DefineAssetId(AlexaRequest alexa)
    {
        if (alexa.request.intent.slots.assetId != null)
        {
            var assetId = alexa.request.intent.slots.assetId.value;
            if (assetId != null)
                return assetId;
            else
                return "";
        }
        else
        {
            return "";
        }
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


}
   