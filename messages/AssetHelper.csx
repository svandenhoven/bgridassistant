#load "Models.csx"
#load "ActionHelper.csx"

using System;
using System.Collections;

public class AssetHelper
{
    private Building _settings;

    public AssetHelper(Building settings)
    {
        _settings = settings;
    }

    public async Task<string> GetAssetLocation(string assetName)
    {
        var assets = _settings.BGridAssets.Where(n => RemoveNonCharactersAndSpace(n.Name) == RemoveNonCharactersAndSpace(assetName));
        if (assets.Count() > 0)
        {
            var assetId = assets.First().AssetId.ToString();
            var msg = await FindAsset(assetId, assetName);
            return msg;
        }
        else
        {
            var msg = $"Do not know {assetName}.";
            return msg;
        }
    }


    public async Task<string> FindAsset(string assetId, string assetName)
    {
        ////Write desk to memory for future use.
        //if (memory.ContainsKey("lastAsset"))
        //    memory["lastAsset"] = assetId;
        //else
        //    memory.Add("lastAsset", assetId);

        var msg = "";
        var asset = await new ActionHelper(_settings).ExecuteGetAction<bGridAsset>($"/api/assets/{assetId}");
        if (asset != null)
        {
            var x = Convert.ToInt32(asset.x);
            var y = Convert.ToInt32(asset.y);
            var floor = asset.floor;

            var spot = FindSpot(x, y, floor);

            if (spot != "")
                msg = $"{assetName} can be found {spot}.";
            else
                msg = $"{assetName} can be found at coordinate {x.ToString()}, {y.ToString()} on floor {floor.ToString()}.";
        }
        else
        {
            msg = $"Cannot find {assetName}. Getting random location as a demonstration. ";
            var rand = new Random();
            var x = rand.Next(-21, 0);
            var y = rand.Next(-73, -25);
            var floor = rand.Next(1, 4);
            var spot = FindSpot(x, y, floor);

            if (spot != "")
                msg += $"{assetName} can be found {spot}.";
            else
                msg += $"{assetName} can be found at coordinate {x.ToString()}, {y.ToString()}.";

        }
        return msg;
    }


    private string FindSpot(double x, double y, int floor)
    {
        var spotName = "";
        var spots = _settings.Spots;
        foreach (var s in spots)
        {
            if (s.Level == floor.ToString() && x >= s.X1 && x < s.X2 && y >= s.Y1 && y < s.Y2)
            {
                spotName = s.Name;
                break;
            }
        }

        return spotName;
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