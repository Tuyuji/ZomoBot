using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Common;
using Zomo.Core.Interfaces;

namespace Zomo.Core.Services.Audio
{
    /*
     * Putting audio in its own folder due to this possibly getting more advanced.
     */
    
    /*
     * Audio session: A object that contains:
     *  - Current connected VC.
     *  - Music Queue.
     *  - The person that inited the session.
     *  - General session settings.
     * 
     */
    
    [BotServiceInfo("Audio", "", ServicePriority.Low)]
    public class AudioService : IBotService
    {
        private readonly IServiceProvider _services;
        private readonly DiscordSocketClient _client;

        private Dictionary<ulong, AudioSession> _sessions = new Dictionary<ulong, AudioSession>();

        private bool _canRun = true;

        public bool CanRun => _canRun;
        
        public AudioService(IServiceProvider services)
        {
            AudioServiceExt.asr = this;
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            
            _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        }

        private Task ClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState state)
        {
            var oldVCID = oldState.VoiceChannel.Id;
            var currentVCID = state.VoiceChannel.Id;
            var oldVCUsersCount = oldState.VoiceChannel.Users.Count;

            Console.WriteLine($"{user.Username}: {oldState.ToString()} -> {state.ToString()}");
            
            foreach (var audioSession in _sessions)
            {
                Console.WriteLine($"{oldState.ToString()} has {oldVCUsersCount} user/s");
                if (oldVCID == audioSession.Value.Id && oldVCUsersCount == 1)
                {
                    audioSession.Value.Dispose(DisconnectReason.NoUsers);
                    _sessions.Remove(audioSession.Key);
                }
            }
            
            return Task.CompletedTask;
        }

        public AudioSession MakeSession(IVoiceChannel channel, ISocketMessageChannel messageChannel = null)
        {
            if (channel == null)
            {
                return null;
            }

            // Not going to return the existing session,
            // this is so that way the person calling this function wont get confused.
            if (_sessions.ContainsKey(channel.GuildId))
            {
                return null;
            }
            
            AudioSession session = new AudioSession(channel, messageChannel, _client);
            _sessions.Add(channel.GuildId, session);
            return session;
        }

        public async Task<bool> KillSessionAsync(ulong guildId)
        {
            if (!_sessions.TryGetValue(guildId, out var session))
            {
                return false;
            }
            
            session.Dispose();
            _sessions.Remove(guildId);
            return true;
        }

        public AudioSession GetSession(ulong guildId)
        {
            if (!_sessions.TryGetValue(guildId, out var session))
            {
                return null;
            }

            return session;
        }

        public bool Exist(ulong guildId)
        {
            return _sessions.ContainsKey(guildId);
        }
    
        public void ServiceStart()
        {
            
        }

        public void ServicePostStart()
        {
            
        }

        public void ServiceDispose()
        {
            foreach (var audioSession in _sessions)
            {
                audioSession.Value.Dispose();
            }
        }
    }

    public static class AudioServiceExt
    {
        public static AudioService asr;

        public static bool HasAudioSession(this SocketGuild obj)
        {
            return asr.Exist(obj.Id);
        }

        public static AudioSession GetAudioSession(this SocketGuild obj)
        {
            return asr.GetSession(obj.Id);
        }
    }
}