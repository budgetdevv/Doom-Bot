using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using _02_commands_framework.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetTemplate.Modules;
using DoomBot.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordNetTemplate
{
    class Program
    {
        static async Task Main(string[] args)
            => await new Program().MainAsync();

        public struct DiscordConf : IConfig
        {
            [JsonIgnore]
            public string Path => "Configs/DiscordConfig.json";
            
            public string Token { get; set; }
            
            public string Prefix { get; set; }
        }

        public static Config<DiscordConf> Config;

        static Program()
        {
            Config.TryLoadConfig();
        }
        
        public async Task MainAsync()
        {
            await using var services = ConfigureServices();
            
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            try
            {
                await client.LoginAsync(TokenType.Bot, Config.Conf.Token);
            }

            catch
            {
                Console.Clear();
                
                Console.WriteLine($"Make sure to input a valid token in {Config.Conf.Path} !");
                
                Config.GenerateConfig();
                
                goto End;
            }
            
            await client.StartAsync();

            // Here we initialize the logic required to register our commands.
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            services.GetRequiredService<DisboardReminderModule>();
            
            await Task.Delay(Timeout.Infinite);
            
            End:
            {
                Console.ReadKey();
            }
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<PrivateChannelModule>()
                .AddSingleton<DisboardReminderModule>()
                .BuildServiceProvider();
        }
    }
}
