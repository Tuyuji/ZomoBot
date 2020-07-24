using System;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Zomo.Core.Services;

namespace Zomo.Core.Common
{
    public struct ReactionEvent
    {
        public Action<Cacheable<IUserMessage, ulong>, ISocketMessageChannel, SocketReaction> Event;
        public ulong Message;
    }

    public class ReactionDialog
    {
        private RestUserMessage _message;
        private ReactionEvent _event;
        private SocketCommandContext _context;
        private bool _senderOnly;
        private bool _shouldDelete;
        private Action _onConfirm;
        private Action _onCancel;
        private ulong _botID;

        public ReactionDialog(RestUserMessage message, SocketCommandContext context, Action onConfirm, Action onCancel,
            bool senderOnly, bool shouldDeleteAfter, ulong botId)
        {
            _message = message;
            _event = new ReactionEvent {Message = message.Id, Event = OnReactionClicked};
            _context = context;
            _senderOnly = senderOnly;
            _shouldDelete = shouldDeleteAfter;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _botID = botId;

            //Setup emojis
            message.AddReactionsAsync(new IEmote[]
            {
                new Emoji("✅"),
                new Emoji("❌")
            }).ContinueWith(e => { ReactionService.Instance.AddReactionEvent(_event); });
        }

        private void OnReactionClicked(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            try
            {
                //Only continue if the person who gave the reaction is the sender of the og command.
                if (_senderOnly)
                    if (reaction.User.Value.Id != _context.User.Id)
                        return;

                if (reaction.User.Value.Id == _botID) return;


                switch (reaction.Emote.Name)
                {
                    case "❌":
                        _onCancel.Invoke();
                        break;
                    case "✅":
                        _onConfirm.Invoke();
                        break;
                    default:
                        return;
                }

                if (_shouldDelete)
                    message.GetOrDownloadAsync().Result.DeleteAsync();

                ReactionService.Instance.RemoveReactionEvent(_event);
            }
            catch (Exception ex)
            {
                //Logger.Log(ex.ToString(), this);
                throw;
            }
        }
    }
}