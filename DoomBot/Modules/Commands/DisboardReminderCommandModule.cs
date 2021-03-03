using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordNetTemplate.Modules.Commands
{
    // Modules must be public and inherit from an IModuleBase
    public class DisboardReminderCommandModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        
        //Some property here
            
        public DisboardReminderModule Module { get; set; }

        [Command("bumptime")]
        public Task BumpTime()
        {
            Module.BumpTime(Context);
            
            return Task.CompletedTask;
        }
        
        [Command("bumprole")]
        public Task BumpRole()
        {
            Module.ViewBumpRole(Context);
            
            return Task.CompletedTask;
        }
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setbumprole")]
        public Task SetBumpRole(SocketRole Role)
        {
            Module.SetMentionRole(Context, Role);
            
            return Task.CompletedTask;
        }
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("mentionbumprole")]
        public Task MentionBumpRole()
        {
            Module.ViewBumpRole(Context, true);
            
            return Task.CompletedTask;
        }
    }
}
