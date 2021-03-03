using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetTemplate.Modules;
using Newtonsoft.Json;

namespace DoomBot.Modules
{
    public class DisboardReminderModule: IDisposable
    {
        private static readonly Color SuccessColor = new Color(36, 183, 183);

        private static readonly TimeSpan BumpDelay = TimeSpan.FromHours(2);
        
        private readonly DiscordSocketClient Client;

        private DateTime NextBump;

        private struct DisboardReminderConfig: IConfig
        {
            [JsonIgnore]
            public string Path => "Configs/DiscordReminderConf.json";

            public ulong MentionRoleID { get; set; }
        }

        private Config<DisboardReminderConfig> Config;
        
        public DisboardReminderModule(DiscordSocketClient client)
        {
            Config.TryLoadConfig();
            
            Client = client;
            
            Client.MessageReceived += OnMsgReceived;
            
            NextBump = DateTime.MinValue;
        }

        private Task OnMsgReceived(SocketMessage Msg)
        {
            if (Msg.Author.Id != 302050872383242240)
            {
                return Task.CompletedTask;
            }

            var EM = Msg.Embeds.First();

            if (EM.Color == SuccessColor)
            {
                _ = Countdown(Msg.Channel);
                
                Msg.Channel.SendMessageAsync($"Thanks for bumping! [ {Msg.MentionedUsers.First()} ]");

                return Task.CompletedTask;
            }

            var Str = Msg.Embeds.First().Description;

            var Match = Regex.Match(Str, @"(\d+)\sminutes");

            if (!Match.Success)
            {
                return Task.CompletedTask;
            }

            var Mins = int.Parse(Match.Groups[1].Value);

            var TheoreticalNextBump = DateTime.UtcNow + TimeSpan.FromMinutes(Mins);

            if (TheoreticalNextBump < NextBump) //If someone had a global-cooldown of 30 mins, and their timing got registered
            {
                NextBump = TheoreticalNextBump;
            }

            return Task.CompletedTask;
        }

        private async Task Countdown<ChannelT>(ChannelT Channel) where ChannelT: ISocketMessageChannel
        {
            NextBump = DateTime.UtcNow + BumpDelay;

            await Task.Delay(BumpDelay);

            if (NextBump > DateTime.UtcNow)
            {
                return;
            }

            _ = Channel.SendMessageAsync($"Time to bump, kids! [ <@&{Config.Conf.MentionRoleID}> ]");
        }

        public void SetMentionRole(SocketCommandContext Context, SocketRole Role)
        {
            Config.Conf.MentionRoleID = Role.Id;
            
            Config.UpdateConfig();

            Context.Channel.SendMessageAsync($":white_check_mark: | Success! [ {Role.Mention} ]", false, null, null, AllowedMentions.None);
        }

        public void Dispose()
        {
            Client.MessageReceived -= OnMsgReceived;
        }

        public void BumpTime(SocketCommandContext Context)
        {
            if (NextBump == DateTime.MinValue)
            {
                _ = Context.Channel.SendMessageAsync(":warning: | Bump Time unknown! Do !d bump to register Bump Time!");
                
                return;
            }

            var TS = NextBump - DateTime.UtcNow;

            if (TS <= TimeSpan.Zero)
            {
                _ = Context.Channel.SendMessageAsync(":clock: | You may bump now!");

                return;
            }
            
            _ = Context.Channel.SendMessageAsync($":clock: | You may bump in `{TS.TotalMinutes}` mins!");
        }

        public void ViewBumpRole(SocketCommandContext Context, bool Mention = false)
        {
            Context.Channel.SendMessageAsync($"[ <@&{Config.Conf.MentionRoleID}> ]", false, null, null, (Mention) ? AllowedMentions.All : AllowedMentions.None);
        }
    }
}