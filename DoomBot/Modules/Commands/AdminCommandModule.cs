using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DoomBot.Modules.Commands
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("prefix")]
        public Task Prefix(string NewPrefix)
        {
            Program.Config.Conf.Prefix = NewPrefix;
            
            Program.Config.UpdateConfig();
            
            _ = ReplyAsync($":white_check_mark: | Success!");

            return Task.CompletedTask;
        }
        
        [Command("kick")]
        public Task Kick(SocketGuildUser TargetUser, [Remainder]string Reason = "No reason given")
        {
            var ContextUser = TargetUser.Guild.GetUser(Context.User.Id);

            if (ContextUser.Hierarchy <= TargetUser.Hierarchy)
            {
                _ = ReplyAsync($":negative_squared_cross_mark: | You may only `kick` someone of lower hierarchy than you're!");

                return Task.CompletedTask;
            }
            
            _ = TargetUser.SendMessageAsync($"You've been kicked from {TargetUser.Guild}! - {Reason}");

            _ = TargetUser.KickAsync(Reason);

            _ = ReplyAsync($":white_check_mark: | Successfully `kicked` {TargetUser.Mention} !");
            
            return Task.CompletedTask;
        }
        
        [Command("ban")]
        public Task Ban(SocketGuildUser TargetUser, [Remainder]string Reason = "No reason given")
        {
            var ContextUser = TargetUser.Guild.GetUser(Context.User.Id);

            if (ContextUser.Hierarchy <= TargetUser.Hierarchy)
            {
                _ = ReplyAsync($":negative_squared_cross_mark: | You may only `ban` someone of lower hierarchy than you're!");

                return Task.CompletedTask;
            }
            
            _ = TargetUser.SendMessageAsync($"You've been banned from {TargetUser.Guild}! - {Reason}");

            _ = TargetUser.BanAsync(0, Reason);
            
            _ = ReplyAsync($":white_check_mark: | Successfully `banned` {TargetUser.Mention} !");

            return Task.CompletedTask;
        }
        
        [Command("addemoji")]
        public async Task AddEmoji([Remainder]string MsgLink)
        {
            var ParsedMsg = await DiscordHelpers.TryParseMessageLinkAsync(Context, MsgLink);
            
            if (ParsedMsg == null)
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | Invalid Channel or Message ID!");

                return;
            }

            var Attachments = ParsedMsg.Attachments;
            
            var Msg = await Context.Channel.SendMessageAsync($"<a:Party:816675948250398762> | Adding Emoji(s)... [ `{Attachments.Count}` detected! ]");

            var Guild = Context.Guild;

            using var HC = new HttpClient();

            var Count = 0;

            try
            {
                foreach (var Attachment in Attachments)
                {
                    var Img = new Image(await HC.GetStreamAsync(Attachment.Url));

                    var Name = Attachment.Filename;

                    DiscordHelpers.RemoveExtensionFromString(ref Name);

                    var Emoji = await Guild.CreateEmoteAsync(Name, Img);

                    if (Emoji != null)
                    {
                        Count++;
                    }
                }
            }

            finally
            {
                _ = Msg.ModifyAsync(x => x.Content = $"<a:Party:816675948250398762> | Successfully added `{Count}` Emoji(s) !");
            }
        }
    }
}
