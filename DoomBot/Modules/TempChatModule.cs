using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetTemplate.Modules;

namespace DoomBot.Modules
{
    public class TempChatModule: IDisposable
    {
        private readonly DiscordSocketClient Client;

        private readonly Dictionary<ulong, DateTime> UserToExpiry;
        
        public TempChatModule(DiscordSocketClient client)
        {
            Client = client;
            
            Client.MessageReceived += OnMsgReceived;

            UserToExpiry = new Dictionary<ulong, DateTime>(10);
        }

        private Task OnMsgReceived(SocketMessage Msg)
        {
            if (Msg.Channel is not SocketTextChannel)
            {
                return Task.CompletedTask;
            }
            
            var Author = Msg.Author.Id;

            if (!UserToExpiry.TryGetValue(Author, out var Exp) || Exp == DateTime.MinValue)
            {
                return Task.CompletedTask;
            }

            _ = CountDown();
            
            return Task.CompletedTask;
            
            async Task CountDown()
            {
                var Diff = Exp - DateTime.UtcNow;
            
                await Task.Delay(Diff);

                Exp = UserToExpiry[Author];
            
                Diff = Exp - DateTime.UtcNow;
            
                while (Diff > TimeSpan.Zero)
                {
                    await Task.Delay(Diff);

                    Exp = UserToExpiry[Author];
                
                    Diff = Exp - DateTime.UtcNow;
                }

                _ = Msg.DeleteAsync();
            }
        }

        public void TempChat<UserT>(SocketCommandContext Context, UserT User, TimeSpan TS) where UserT: IUser
        {
            var UserID = User.Id;
            
            if (UserToExpiry.TryGetValue(UserID, out var Exp) && Exp != DateTime.MinValue)
            {
                _ = Context.Channel.SendMessageAsync($":negative_squared_cross_mark: | Temp chat is currently active for {User.Mention} ! [ Expires in `{TS.TotalMinutes}` mins! ]");

                return;
            }

            Exp = DateTime.UtcNow + TS;

            UserToExpiry[UserID] = Exp;

            _ = Countdown(UserID, Exp);
            
            _ = Context.Channel.SendMessageAsync($":white_check_mark: | Deleting all messages sent from this point, by {User.Mention} in `{TS.TotalMinutes}` mins!");
        }

        private async Task Countdown(ulong UserID, DateTime Exp)
        {
            var Diff = Exp - DateTime.UtcNow;

            await Task.Delay(Diff);

            var UpdatedExp = UserToExpiry[UserID];
            
            while (UpdatedExp != Exp)
            {
                Diff = Exp - DateTime.UtcNow;
                
                await Task.Delay(Diff);
                
                UpdatedExp = UserToExpiry[UserID];
            }
            
            UserToExpiry[UserID] = DateTime.MinValue;
        }

        // private void TempChatExtend(SocketCommandContext Context, SocketGuildUser User, TimeSpan TS)
        // {
        //     
        //     
        //     _ = Context.Channel.SendMessageAsync($":white_check_mark: | Temp Chat extended! Now deleting all messages sent from this point, by {User.Mention} in `{TS.TotalMinutes}` mins!");
        // }
        
        public void Dispose()
        {
            Client.MessageReceived -= OnMsgReceived;
        }
    }
}