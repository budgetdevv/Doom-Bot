using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DoomBot;
using DoomBot.Modules;

namespace DiscordNetTemplate.Modules
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
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("prefix")]
        public Task Prefix(string NewPrefix)
        {
            Program.Config.Conf.Prefix = NewPrefix;
            
            Program.Config.UpdateConfig();
            
            _ = ReplyAsync($":white_check_mark: | Success!");

            return Task.CompletedTask;
        }
    }
}
