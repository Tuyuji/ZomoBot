using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Zomo.Core.Common;

namespace Zomo.ShineBot.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private readonly Config _headpatConfig = new Config("headpat.cfg");

        [Command("HeadPat")]
        public async Task Headpat(SocketUser user = null)
        {
            _headpatConfig.Load();
            if (!_headpatConfig.HasVar("images") || !_headpatConfig.HasVar("lines"))
            {
                this.Log("Missing images and lines!");
                return;
            }

            string[] images = _headpatConfig.Get<string[]>("images");
            string[] lines = _headpatConfig.Get<string[]>("lines");

            Random random = new Random();

            string image = images[random.Next(images.Length)];
            string line = lines[random.Next(lines.Length)];

            var builder = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Headpats!")
                .WithDescription(line)
                .WithImageUrl(image);

            if (user != null)
                builder.Description += $" for {user.Mention}";

            await ReplyAsync("", false, builder.Build());
        }
    }
}