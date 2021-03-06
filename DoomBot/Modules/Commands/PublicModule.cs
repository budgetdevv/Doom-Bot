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
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("kick")]
        public Task Kick(SocketGuildUser TargetUser, string Reason = "No reason given")
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
        
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("ban")]
        public Task Ban(SocketGuildUser TargetUser, string Reason = "No reason given")
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
    }
}
