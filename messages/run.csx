#r "Newtonsoft.Json"
#load "BuildingDialog.csx"

using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Configuration;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// Bot Storage: Register the optional private state storage for your bot. 

// (Default) For Azure Table Storage, set the following environment variables in your bot app:
// -UseTableStorageForConversationState set to 'true'
// -AzureWebJobsStorage set to your table connection string

// For CosmosDb, set the following environment variables in your bot app:
// -UseCosmosDbForConversationState set to 'true'
// -CosmosDbEndpoint set to your cosmos db endpoint
// -CosmosDbKey set to your cosmos db key

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    // Initialize the azure bot
    using (BotService.Initialize())
    {
        // Deserialize the incoming activity
        string jsonContent = await req.Content.ReadAsStringAsync();
        var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
        
        // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
        // if request is authenticated
        if (!await BotService.Authenticator.TryAuthenticateAsync(req, new [] {activity}, CancellationToken.None))
        {
            return BotAuthenticator.GenerateUnauthorizedResponse(req);
        }

       
        if (activity != null)
        {
            var activityTxt = JsonConvert.SerializeObject(activity);
            log.Info($"Activity: {activityTxt}.");

            var channeldata = activity.ChannelData;
            var channeldatatxt = JsonConvert.SerializeObject(channeldata);
            log.Info($"ChannelData: {channeldatatxt}.");

            var userInfo = activity.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));

            //Authorize any allowed users
            var allowedUsers = ConfigurationManager.AppSettings["AuthorizedUsers"].Split(',');
            var email = "unknown";
            var authorized = false;
            if (userInfo != null)
            {
                var userInfoTxt = JsonConvert.SerializeObject(userInfo);
                log.Info($"UserInfo: {userInfoTxt}.");

                email = userInfo.Properties.Value<string>("email");
                log.Info($"Email User: {email}.");
                if (!string.IsNullOrEmpty(email))
                {
                    if (allowedUsers.Contains(email))
                    {
                        authorized = true;
                    }
                }
            }
            if(!authorized)
            {
                var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                var reply = activity.CreateReply();
                reply.Text = $"You are not authorized to use this skill. Your user name is {email}.";
                await client.Conversations.ReplyToActivityAsync(reply);
                return req.CreateResponse(HttpStatusCode.Accepted);
            }

            // one of these will have an interface and process it
            switch (activity.GetActivityType())
            {
                case ActivityTypes.Message:
                    await Conversation.SendAsync(activity, () => new BuildingDialog());
                    break;
                case ActivityTypes.ConversationUpdate:
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                    IConversationUpdateActivity update = activity;
                    if (update.MembersAdded.Any())
                    {
                        var reply = activity.CreateReply();
                        var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                        foreach (var newMember in newMembers)
                        {
                            reply.Text = "Welcome";
                            if (!string.IsNullOrEmpty(newMember.Name))
                            {
                                reply.Text += $" {newMember.Name}";
                            }
                            reply.Text += "!";
                            await client.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                    break;
                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:
                case ActivityTypes.Ping:
                default:
                    log.Error($"Unknown activity type ignored: {activity.GetActivityType()}");
                    break;
            }
        }
        return req.CreateResponse(HttpStatusCode.Accepted);
    }    
}
