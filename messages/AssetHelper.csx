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
            var msg = await FindAsset(assetId);
            return msg;
        }
        else
        {
            var msg = $"Do not know {assetName}.";
            return msg;
        }
    }


    public async Task<string> FindAsset(string assetId)
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

            var spot = FindSpot(x, y);

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


    private string FindSpot(double x, double y)
    {
        var spotName = "";
        var spots = _settings.Spots;
        foreach (var s in spots)
        {
            if (x >= s.X1 && x < s.X2 && y >= s.Y1 && y < s.Y2)
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