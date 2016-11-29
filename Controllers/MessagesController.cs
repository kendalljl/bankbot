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
using System.Collections.Generic;

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
                string userInput = activity.Text.ToLower();

                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                if (userData.GetProperty<bool>("GreetingSent"))
                {
                    finalOutput = "I didn't understand that one sorry!";
                }
                else
                {
                    userData.SetProperty<bool>("GreetingSent", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }
                CurrencyObject.RootObject rootObject;
                HttpClient client = new HttpClient();
                //if the user wants to look at currency rates
                if (userInput.Contains("currency"))
                {
                    bool askedCurrency = false;
                    if (userInput.Contains("gbp"))
                    {
                        selectedCurr = "gbp";
                    }
                    else if (userInput.Contains("nzd"))
                    {
                        selectedCurr = "nzd";
                    }
                    else if (userInput.Contains("usd"))
                    {
                        selectedCurr = "usd";
                    }
                    else if (userInput.Contains("eur"))
                    {
                        selectedCurr = "eur";
                    }
                    else if (userInput.Contains("aud"))
                    {
                        selectedCurr = "aud";
                    }
                    if (selectedCurr != "none")
                    {
                        if (askedCurrency == false)
                        {
                            string currencyurl = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + selectedCurr));
                            rootObject = JsonConvert.DeserializeObject<CurrencyObject.RootObject>(currencyurl);
                            string baseCurrency = rootObject.@base;
                            double gbp = rootObject.rates.GBP;
                            double aud = rootObject.rates.AUD;
                            double eur = rootObject.rates.EUR;
                            double usd = rootObject.rates.USD;
                            finalOutput = "Current rate for " + baseCurrency + " to GBP: " + gbp + ", AUD: " + aud + ", EURO: " + eur + ", USD: " + usd;
                            Activity replyToConversation = activity.CreateReply("One moment...");
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            askedCurrency = true;
                        }
                        else
                        {
                            finalOutput = "You've already asked for currency rates, ask again if you are sure";
                        }
                    }
                    else
                    {
                        finalOutput = "Sorry, could you please be more specific? (3 digit currency code)";
                    }
                }
                //if the user wants to clear user data
                if (userInput.Contains("clear") || userInput.Contains("delete"))
                {
                    finalOutput = "Your user data has been cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                }



                // return our reply to the user
                Activity reply = activity.CreateReply(finalOutput);
                await connector.Conversations.ReplyToActivityAsync(reply);

                //display bank info item card 
                if (userInput.Contains("info"))
                {
                    finalOutput = "Contoso Bank Information";
                    Activity replyToConversation = activity.CreateReply();
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://bit.ly/2gRFx1i"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "https://github.com/troniik/bankbot",
                        Type = "openUrl",
                        Title = "Contoso Bank Ltd"
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit our website!",
                        //Subtitle = "(Actually my github)",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

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