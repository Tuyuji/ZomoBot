using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Zomo.Core.Common;
using Zomo.Core.Services;

namespace Zomo.ShineBot.Modules
{
    public class ShineHelpful : ModuleBase<SocketCommandContext>
    {
        private readonly ReactionService _reaction;

        public ShineHelpful(IServiceProvider _services)
        {
            _reaction = _services.GetRequiredService<ReactionService>();
        }


        [Command("AddMoji")]
        [RequireBotPermission(GuildPermission.ManageEmojis)]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task AddMoji(string emojiName, string url = "")
        {
            var attachments = Context.Message.Attachments;
            var emojis = Context.Guild.Emotes;

            if (attachments.Count == 0 && url == "")
            {
                await ReplyAsync("Please give a link to a image or send the command with a image attached.");
                return;
            }

            if (attachments.Count != 0 && url == "")
            {
                url = attachments.ElementAt(0).Url;
            }

            bool doesEmojiExistAlready = emojis.Any(e => e.Name.ToLower() == emojiName.ToLower());
            if (doesEmojiExistAlready)
            {
                await ReplyAsync("Emoji name is taken.");
                return;
            }

            try
            {
                var message =
                    await Context.Channel.SendMessageAsync(
                        $"Do you really want to create a new emoji named \"{emojiName}\"");

                _reaction.CreateReactionEvent(message, Context, async () =>
                    {
                        var request = (HttpWebRequest) WebRequest.Create(url);
                        using (var response = (HttpWebResponse) request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            var image = new Image(stream);

                            await Context.Guild.CreateEmoteAsync(emojiName, image).ContinueWith(async e =>
                            {
                                await ReplyAsync($"Added emoji {e}");
                            });
                        }
                    },
                    () =>
                    {
                        //Do nothing
                    });
            }
            catch (Exception ex)
            {
                // Do nothing for now
            }
        }

        [Command("RemoveMoji")]
        [RequireBotPermission(GuildPermission.ManageEmojis)]
        [RequireUserPermission(GuildPermission.ManageEmojis)]
        public async Task RemoveMoji(string emojiName)
        {
            if (emojiName.StartsWith('<') || emojiName.EndsWith('>'))
            {
                //lets convert it to what we want.
                var finalname = emojiName;

                finalname = finalname.Remove(finalname.Length - 1, 1);
                finalname = finalname.Remove(0, 2);

                var eg = finalname.Split(':');
                finalname = eg[0];

                emojiName = finalname;
            }

            var emojis = Context.Guild.Emotes;
            var emoji = emojis.First(e => e.Name.ToLower() == emojiName.ToLower());

            if (emoji == null)
            {
                await ReplyAsync($"Couldn't find emoji with name \"{emojiName}\"");
                return;
            }

            var message = await Context.Channel.SendMessageAsync($"Do you really want to remove {emoji}?");

            _reaction.CreateReactionEvent(message, Context,
                async () =>
                {
                    await Context.Guild.DeleteEmoteAsync(emoji).ContinueWith(async e =>
                    {
                        await ReplyAsync($"Bye bye {emojiName}");
                    });
                }, () => { });
        }
    }
}