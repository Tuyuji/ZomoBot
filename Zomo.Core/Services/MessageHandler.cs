using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Services
{
    [BotServiceInfo("Message Handler", "Checks for commands and logging.", ServicePriority.High)]
    public class MessageHandler : IBotService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;

        public MessageHandler(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _command = _services.GetRequiredService<CommandService>();

            _client.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            if (messageParam.Author.IsBot) return;

            var message = messageParam as SocketUserMessage;
            string prefix = "!"; //TODO: Get a guilds config and set the current prefix
            var context = new SocketCommandContext(_client, message);
            int argPos = 0;

            if (message.HasStringPrefix(prefix, ref argPos)
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _command.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.UnknownCommand:
                        {
                            await HandleUnknownCommand(context);
                            break;
                        }
                    }
                }
            }
        }

        private async Task HandleUnknownCommand(SocketCommandContext context)
        {
            //Could do something where you could check a users script for commands 
        }

        public void ServiceStart()
        {
        }

        public void ServicePostStart()
        {
        }

        public void ServiceDispose()
        {
        }
    }
}