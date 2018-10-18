using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AlexaRequest
{
    public Session session { get; set; }
    public Request request { get; set; }
    public Context context { get; set; }
    public string version { get; set; }
}

public class Session
{
    public bool _new { get; set; }
    public string sessionId { get; set; }
    public Application application { get; set; }
    public Attributes attributes { get; set; }
    public User user { get; set; }
}

public class Application
{
    public string applicationId { get; set; }
}

public class Attributes
{
}

public class User
{
    public string userId { get; set; }
}

public class Request
{
    public string type { get; set; }
    public string requestId { get; set; }
    public Intent intent { get; set; }
    public string locale { get; set; }
    public DateTime timestamp { get; set; }
}

public class Intent
{
    public string name { get; set; }
    public Slots slots { get; set; }
}

public class Slots
{
    public RoomId roomId { get; set; }
    public AssetId assetId { get; set; }
}

public class AssetId
{
    public string name { get; set; }
    public string value { get; set; }
}

public class RoomId
{
    public string name { get; set; }
    public string value { get; set; }
}
public class Context
{
    public Audioplayer AudioPlayer { get; set; }
    public SystemEcho System { get; set; }
}

public class Audioplayer
{
    public string playerActivity { get; set; }
}

public class SystemEcho
{
    public Application application { get; set; }
    public User user { get; set; }
    public Device device { get; set; }
}

public class Device
{
    public string deviceId { get; set; }
    public Supportedinterfaces supportedInterfaces { get; set; }
}

public class Supportedinterfaces
{
}

public class AlexaResponse
{
    public string version { get; set; }
    public Response response { get; set; }
}

public class Response
{
    public Outputspeech outputSpeech { get; set; }
    public bool shouldEndSession { get; set; }
}

public class Outputspeech
{
    public string type { get; set; }
    public string text { get; set; }
}
