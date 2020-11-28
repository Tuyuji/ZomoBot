using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Services.Audio;

namespace Zomo.Core.Modules
{
    [Group("Audio")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServiceProvider _services;
        private readonly AudioService _audio;

        public AudioModule(IServiceProvider services)
        {
            this._services = services;
            _audio = _services.GetRequiredService<AudioService>();
        }

        [Command("Join", RunMode = RunMode.Async)]
        public async Task Join(string songUrl = "")
        {
            if (_audio.Exist(Context.Guild.Id))
            {
                await ReplyAsync("Already joined.");
                return;
            }
            
            IVoiceChannel vc = (Context.User as IGuildUser)?.VoiceChannel;
            if (vc == null)
            {
                await ReplyAsync("Your not in a voice channel, please join one and try again.");
                return;
            }

            AudioSession session = _audio.MakeSession(vc, Context.Channel);

            var bgw = new BackgroundWorker();

            bgw.DoWork += (sender, args) => { session.ConnectAsync(); };
            
            bgw.RunWorkerAsync();

            if (songUrl != "")
                await session.AddSong(new Uri(songUrl));

            //This is a test
            //session.AddSong("youtube", "P5MpLQ8TQcQ");
            //var song = session.AddSong("https://www.youtube.com/watch?v=2zmfP9pi2cI"); The class will figure out the URL
            //session.Play();
            //session.Play(song); Instantly push first in queue and play
        }

        [Command("Leave")]
        public async Task Leave()
        {
            if (!_audio.Exist(Context.Guild.Id))
                await ReplyAsync("Session doesn't exist for guild.");
            
            await _audio.KillSessionAsync(Context.Guild.Id);
        }

        [Command("Add", RunMode = RunMode.Async)]
        public async Task AddSong(string url)
        {
            if (!Context.Guild.HasAudioSession())
            {
                await Join(url);
                return;
            }
            
            var session = _audio.GetSession(Context.Guild.Id);
            await session.AddSong(new Uri(url));
        }

        [Command("List", RunMode = RunMode.Async)]
        public async Task List()
        {
            if (!Context.Guild.HasAudioSession())
            {
                await ReplyAsync("Guild currently dosnt have a session, please create one first");
                return;
            }

            Song? currentlyPlaying = Context.Guild.GetAudioSession().CurrentlyPlaying;
            
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"Songs in {Context.Guild.Name}");

            if (currentlyPlaying.HasValue)
                builder.AddField($"▶️: {currentlyPlaying.Value.Title}", $"by {currentlyPlaying.Value.Author}");
            
            int i = 0;
            foreach (Song song in Context.Guild.GetAudioSession().Songs)
            {
                builder.AddField($"#{i}: {song.Title}", $"by {song.Author}", false);
                i++;
            }

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}