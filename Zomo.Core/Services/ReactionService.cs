using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Common;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Services
{
    [BotServiceInfo("Reaction", "Allows commands to use reactions as buttons", ServicePriority.Mid)]
    public class ReactionService : IBotService
    {
        private static ReactionService _instance;

        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;

        private readonly List<ReactionEvent> _reactionEvents = new List<ReactionEvent>();

        public static ReactionService Instance => _instance;

        public ReactionService(IServiceProvider services)
        {
            _instance = this;
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _client.ReactionAdded += OnReactionAdded;
        }

        public void AddReactionEvent(ReactionEvent reactionEvent)
        {
            _reactionEvents.Add(reactionEvent);
        }

        public void RemoveReactionEvent(ReactionEvent reactionEvent)
        {
            _reactionEvents.Remove(reactionEvent);
        }

        public void CreateReactionEvent(RestUserMessage message, SocketCommandContext context, Action onConfirm,
            Action onCancel, bool senderOnly = true, bool shouldDeleteAfter = true)
        {
            ReactionDialog dialog = new ReactionDialog(message, context, onConfirm, onCancel, senderOnly,
                shouldDeleteAfter, _client.CurrentUser.Id);
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2,
            SocketReaction arg3)
        {
            try
            {
                var reactionEvent = _reactionEvents.First(e => e.Message == arg1.Id);
                reactionEvent.Event.Invoke(arg1, arg2, arg3);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                //Do nothing for now
            }

            return Task.CompletedTask;
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