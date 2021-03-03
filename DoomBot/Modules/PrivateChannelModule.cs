using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordNetTemplate.Modules
{
    public class PrivateChannelModule: IDisposable
    {
        private struct PrivateChannelConfig: IConfig
        {
            [JsonIgnore]
            public string Path => "Configs/PrivateChannelConf.json";

            public ulong Category { get; set; }
        }

        private Dictionary<ulong, SocketTextChannel> ActiveUsers;

        private Dictionary<ulong, ulong> ChannelToUserID;

        private Config<PrivateChannelConfig> Config;

        private DiscordSocketClient Client;

        private TimeSpan MaxDuration;
        
        public PrivateChannelModule(DiscordSocketClient client)
        {
            //DI
            
            Client = client;
            
            Client.ChannelDestroyed += OnChannelDestroyed;

            const int InitSize = 10;
            
            ActiveUsers = new Dictionary<ulong, SocketTextChannel>(InitSize);

            ChannelToUserID = new Dictionary<ulong, ulong>(InitSize);
            
            Config.TryLoadConfig();
            
            MaxDuration = TimeSpan.FromDays(1);
        }

        public async Task CreatePrivateChannel(SocketCommandContext Context, TimeSpan DestroyIn)
        {
            if (DestroyIn > MaxDuration)
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | Duration may not be greater than a day!");

                return;
            }
            
            var Category = Context.Guild.GetCategoryChannel(Config.Conf.Category);

            if (Category == null)
            {
                _ = Context.Channel.SendMessageAsync($":negative_squared_cross_mark: | Private Category doesn't exist! Contact a staff member for assistance!");

                return;
            }
            
            var User = Context.User;

            if (ActiveUsers.TryGetValue(User.Id, out var ExistingTC))
            {
                _ = Context.Channel.SendMessageAsync($":negative_squared_cross_mark: | You've already created a private channel! [ {ExistingTC.Mention} ]");

                return;
            }

            var Msg = await Context.Channel.SendMessageAsync("<a:Party:816675948250398762> | Creating channel...!");

            var TC = await Context.Guild.CreateTextChannelAsync($"{User}'s Private Channel");

            if (TC == null)
            {
                _ = Msg.ModifyAsync(x => x.Content = ":negative_squared_cross_mark: | Failed to create channel! Contact a staff member for assistance!");

                return;
            }
            
            await TC.ModifyAsync(x => x.CategoryId = Category.Id);

            var SocketTC = Context.Guild.GetTextChannel(TC.Id);

            await SocketTC.AddPermissionOverwriteAsync(User, new OverwritePermissions(manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, manageMessages: PermValue.Allow));

            await SocketTC.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
            
            AddActiveUser(User, SocketTC);

            _ = Msg.ModifyAsync(x => x.Content = $":white_check_mark: | Success! [ {TC.Mention} ]");

            _ = DestroyChannel(User.Id, DestroyIn);
        }

        private void AddActiveUser<UserT>(UserT User, SocketTextChannel TC) where UserT: IUser
        {
            ActiveUsers.Add(User.Id, TC);
            
            ChannelToUserID.Add(TC.Id, User.Id);
        }
        
        private void RemoveActiveUser<ChannelT>(ChannelT DeletedChannel) where ChannelT: IChannel
        {
            ChannelToUserID.Remove(DeletedChannel.Id, out var UserID);
                
            ActiveUsers.Remove(UserID, out _);
        }
        
        private void RemoveActiveUser(ulong UserID, out SocketTextChannel TC)
        {
            ActiveUsers.Remove(UserID, out TC);

            if (TC == null)
            {
                return;
            }

            ChannelToUserID.Remove(TC.Id, out _);
        }
        
        private async Task DestroyChannel(ulong UserID, TimeSpan DestroyIn)
        {
            await Task.Delay(DestroyIn);
            
            RemoveActiveUser(UserID, out var Channel);

            _ = Channel?.DeleteAsync();
        }

        public void SetCategory(SocketCommandContext Context, ulong CategoryID)
        {
            var Category = Context.Guild.GetCategoryChannel(CategoryID);

            if (Category == null)
            {
                Context.Channel.SendMessageAsync($":negative_squared_cross_mark: | No such Category with an ID of `{CategoryID}` !");

                return;
            }

            _ = Context.Channel.SendMessageAsync($":white_check_mark: | Success! [ {Category} ]");
            
            Config.Conf.Category = CategoryID;
            
            Config.UpdateConfig();
        }
        
        private Task OnChannelDestroyed(SocketChannel DeletedChannel)
        {
            RemoveActiveUser(DeletedChannel);

            return Task.CompletedTask;
        }

        public void Invite(SocketCommandContext Context, SocketGuildUser InvitedUser)
        {
            var User = Context.User;

            if (InvitedUser == User)
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You can't invite yourself!");
                
                return;
            }
            
            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.AddPermissionOverwriteAsync(InvitedUser, new OverwritePermissions(viewChannel: PermValue.Allow));

            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success!");
        }
        
        public void Invite(SocketCommandContext Context, SocketRole Role)
        {
            var User = Context.User;

            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.AddPermissionOverwriteAsync(Role, new OverwritePermissions(viewChannel: PermValue.Allow));
            
            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success! !");
        }
        
        public void Kick(SocketCommandContext Context, SocketGuildUser InvitedUser)
        {
            var User = Context.User;

            if (InvitedUser == User)
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You can't kick yourself!");
                
                return;
            }
            
            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.RemovePermissionOverwriteAsync(InvitedUser);
            
            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success! !");
        }
        
        public void Kick(SocketCommandContext Context, SocketRole Role)
        {
            var User = Context.User;

            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.RemovePermissionOverwriteAsync(Role);
            
            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success! !");
        }
        
        public void Mute(SocketCommandContext Context, SocketGuildUser InvitedUser)
        {
            var User = Context.User;

            if (InvitedUser == User)
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You can't mute yourself!");
                
                return;
            }
            
            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.AddPermissionOverwriteAsync(InvitedUser, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny));
            
            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success! !");
        }
        
        public void Mute(SocketCommandContext Context, SocketRole Role)
        {
            var User = Context.User;

            if (!ActiveUsers.TryGetValue(User.Id, out var Channel))
            {
                _ = Context.Channel.SendMessageAsync(":negative_squared_cross_mark: | You do not have an active `Private Channel` !");
                
                return;
            }

            _ = Channel.AddPermissionOverwriteAsync(Role, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny));
            
            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success!");
        }

        public void DestroyAll(SocketCommandContext Context)
        {
            var CategoryID = Config.Conf.Category;
            
            var Category = Context.Guild.GetCategoryChannel(CategoryID);

            if (Category == null)
            {
                Context.Channel.SendMessageAsync($":negative_squared_cross_mark: | No such Category with an ID of `{CategoryID}` !");

                return;
            }

            foreach (var Channel in Category.Channels)
            {
                _ = Channel.DeleteAsync();
            }

            _ = Context.Channel.SendMessageAsync(":white_check_mark: | Success!");
        }
        
        public void Dispose()
        {
            Client.ChannelDestroyed -= OnChannelDestroyed;
        }
    }
}
