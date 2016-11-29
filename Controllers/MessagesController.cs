using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using ContosoBank.Models;

namespace ContosoBank
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                string selectedCurr = "none";
                string finalOutput = "Hello! Welcome to the Contoso Bank Bot v1";
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                CurrencyObject.RootObject rootObject;
                HttpClient client = new HttpClient();
                if (activity.Text.ToLower().Contains("currency"))
                {
                    if (activity.Text.ToLower().Contains("gbp"))
                    {
                        selectedCurr = "gbp";
                    }
                    else if (activity.Text.ToLower().Contains("nzd"))
                    {
                        selectedCurr = "nzd";
                    }
                    else if (activity.Text.ToLower().Contains("usd"))
                    {
                        selectedCurr = "usd";
                    }
                    else if (activity.Text.ToLower().Contains("eur"))
                    {
                        selectedCurr = "eur";
                    }
                    else if (activity.Text.ToLower().Contains("aud"))
                    {
                        selectedCurr = "aud";
                    }
                    if (selectedCurr != "none")
                    {
                        string currencyurl = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + selectedCurr));
                        rootObject = JsonConvert.DeserializeObject<CurrencyObject.RootObject>(currencyurl);
                        string baseCurrency = rootObject.@base;
                        double gbp = rootObject.rates.GBP;
                        double aud = rootObject.rates.AUD;
                        double eur = rootObject.rates.EUR;
                        double usd = rootObject.rates.USD;
                        finalOutput = "Current rate for " + baseCurrency + " to GBP: " + gbp + ", AUD: " + aud + ", EURO: " + eur + ", USD: " + usd;
                        activity.CreateReply("One moment...");
                    }
                    else
                    {
                        finalOutput = "Sorry, can you please be more specific? (GBP, AUD, USD, EURO, NZD)";
                    }
                }

                // return our reply to the user
                Activity reply = activity.CreateReply(finalOutput);
                await connector.Conversations.ReplyToActivityAsync(reply);


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}