using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DoomBot.Modules.Commands
{
    // Modules must be public and inherit from an IModuleBase
    public class TempChatCommandModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us

        public TempChatModule Module { get; set; }
        
        [Command("tempchat")]
        public Task TempChat(TimeSpan DeleteIn)
        {
            Module.TempChat(Context, Context.User, DeleteIn);

            return Task.CompletedTask;
        }
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("tempchat")]
        public Task TempChat(SocketGuildUser User, TimeSpan DeleteIn)
        {
            Module.TempChat(Context, User, DeleteIn);

            return Task.CompletedTask;
        }
    }
}
