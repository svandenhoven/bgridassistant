#load "Models.csx"

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

public class ActionHelper
{
    protected Building _settings;

    public ActionHelper(Building settings)
    {
        _settings = settings;
    }

    public  HttpClient GetHttpClient()
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

    public async Task<T> ExecuteGetAction<T>(string action)
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
}