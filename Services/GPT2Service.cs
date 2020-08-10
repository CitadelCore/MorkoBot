using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Amazon.SQS;
using Amazon.SQS.Model;

using Microsoft.Extensions.DependencyInjection;

namespace MorkoBotRavenEdition.Services
{
    public class GPT2Service
    {
        private const string QUEUE_TO_GPT = "https://sqs.eu-west-2.amazonaws.com/622474503675/morkobot-to-gpt";
        private const string QUEUE_FROM_GPT = "https://sqs.eu-west-2.amazonaws.com/622474503675/morkobot-from-gpt";
        private readonly AmazonSQSClient _sqsClient;

        private IDictionary<string, IUserMessage> _messages = new Dictionary<string, IUserMessage>();
        private IDiscordClient _discordClient;

        public GPT2Service(IServiceProvider serviceProvider)
        {
            _sqsClient = serviceProvider.GetService<AmazonSQSClient>();
            _discordClient = serviceProvider.GetService<IDiscordClient>();

            var pollThread = new Thread(async () => await PollMessagesAsync());
            pollThread.Start();
        }

        public async Task QueueRequestAsync(IUserMessage message, string prefix)
        {
            var result = await _sqsClient.SendMessageAsync(QUEUE_TO_GPT, prefix);

            var channel = message.Channel;
            var builder = new EmbedBuilder();
            builder.WithColor(Color.Orange);
            builder.WithTitle("Processing...");
            builder.WithDescription("MorkoBot is running the model. Please wait!");
            builder.WithFooter("Request has been sent to the worker machine for processing");

            var sent = await channel.SendMessageAsync(null, false, builder.Build());
            _messages.Add(result.MessageId, sent);
        }

        private async Task PollMessagesAsync() {
            while(true) {
                var messages = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest(QUEUE_FROM_GPT) {
                    MessageAttributeNames = { "OriginalId", "TimeElapsed" }
                });

                foreach (var message in messages.Messages)
                {
                    string mid = null;
                    string time = null;
                    foreach (var attribute in message.MessageAttributes)
                    {
                        switch(attribute.Key)
                        {
                            case "OriginalId":
                                mid = attribute.Value.StringValue;
                                break;
                            case "TimeElapsed":
                                time = attribute.Value.StringValue;
                                break;
                        }
                    }

                    if (mid == null || !_messages.ContainsKey(mid)) continue;
                    var resolved = _messages[mid];
                    _messages.Remove(mid);

                    // truncate, max body length is 2048
                    var bodyText = message.Body;
                    bodyText = bodyText.Substring(0, Math.Min(bodyText.Length, 2000));

                    var footer = $"Processed in {time}ms";
                    if (bodyText.Contains("[BLACKSTAR REDACTED"))
                        footer += " | Sensitive content was detected and redacted.";

                    var builder = new EmbedBuilder();
                    builder.WithColor(Color.Green);
                    builder.WithTitle("Generation complete!");
                    builder.WithDescription($"```\n{bodyText}\n```");
                    builder.WithFooter(footer);

                    var embed = builder.Build();

                    try {
                        await resolved.ModifyAsync(m => {
                            m.Embed = builder.Build();
                        });
                    } 
                    catch (Exception)
                    {
                        // do nothing, who cares?
                    }

                    await _sqsClient.DeleteMessageAsync(QUEUE_FROM_GPT, message.ReceiptHandle);
                }

                Thread.Sleep(3000);
            }
        }
    }
}
