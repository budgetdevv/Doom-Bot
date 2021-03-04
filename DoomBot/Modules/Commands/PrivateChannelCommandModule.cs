using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DoomBot.Modules;

namespace DiscordNetTemplate.Modules.Commands
{
    // Modules must be public and inherit from an IModuleBase
    public class PrivateChannelCommandModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        
        //Some property here
        
        public PrivateChannelModule Module { get; set; }
        
        [Command("pc")]
        public Task CreatePrivateChannel(TimeSpan TimeSpan)
        {
            _ = Module.CreatePrivateChannel(Context, TimeSpan);

            return Task.CompletedTask;
        }

        [Command("pcinvite")]
        public Task PrivateChannelInvite(SocketGuildUser User)
        {
            Module.Invite(Context, User);

            return Task.CompletedTask;
        }
        
        [Command("pcinvite")]
        public Task PrivateChannelInvite(SocketRole Role)
        {
            Module.Invite(Context, Role);

            return Task.CompletedTask;
        }
        
        [Command("pckick")]
        public Task PrivateChannelKick(SocketGuildUser User)
        {
            Module.Kick(Context, User);

            return Task.CompletedTask;
        }
        
        [Command("pckick")]
        public Task PrivateChannelKick(SocketRole Role)
        {
            Module.Kick(Context, Role);

            return Task.CompletedTask;
        }
        
        [Command("pcmute")]
        public Task PrivateChannelMute(SocketGuildUser User)
        {
            Module.Mute(Context, User);

            return Task.CompletedTask;
        }
        
        [Command("pcmute")]
        public Task PrivateChannelMute(SocketRole Role)
        {
            Module.Mute(Context, Role);

            return Task.CompletedTask;
        }
        
        [Command("pcunmute")]
        public Task PrivateChannelUnmute(SocketGuildUser User)
        {
            Module.Invite(Context, User);

            return Task.CompletedTask;
        }
        
        [Command("pcunmute")]
        public Task PrivateChannelUnmute(SocketRole Role)
        {
            Module.Invite(Context, Role);

            return Task.CompletedTask;
        }
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setprivatechannelcategory")]
        public Task SetPrivateChannel(ulong CategoryID)
        {
            Module.SetCategory(Context, CategoryID);

            return Task.CompletedTask;
        }
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("pcsclear")]
        public Task ClearPrivateChannels()
        {
            Module.DestroyAll(Context);

            return Task.CompletedTask;
        }
    }
}
