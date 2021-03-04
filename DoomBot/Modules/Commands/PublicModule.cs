using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DoomBot;

namespace DiscordNetTemplate.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        
        //Some property here
        
        [Command("ping")]
        public Task Ping()
        {
            _ = ReplyAsync($":ping_pong: | Ping-pong! `{Context.Client.Latency}` ms!");

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
