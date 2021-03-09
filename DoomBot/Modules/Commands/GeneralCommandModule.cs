using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DoomBot.Modules.Commands
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us

        public TempChatModule Module { get; set; }
        
        [Command("ping")]
        public Task Ping()
        {
            _ = ReplyAsync($":ping_pong: | Ping-pong! `{Context.Client.Latency}` ms!");

            return Task.CompletedTask;
        }
    }
}
