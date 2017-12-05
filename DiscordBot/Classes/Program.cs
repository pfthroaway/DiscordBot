using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Classes
{
    internal class Program
    {
        private static DiscordSocketClient Client;
        private CommandService Commands;

        public static bool IsExit = false;

        public static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        private async Task Start()
        {
            using (Client = new DiscordSocketClient())
            {
                Commands = new CommandService();

                Client.Connected += async () =>
                {
                    Console.WriteLine("Connected!");
                    await Task.CompletedTask;
                };

                Client.Ready += async () =>
                {
                    AppState.LoadAll(Client);

                    Console.WriteLine("Ready!");
                    await Task.CompletedTask;
                };

                await Client.LoginAsync(TokenType.Bot, await AppState.DatabaseInteraction.LoadBotToken());
                await Client.StartAsync();

                await InstallCommands();

                //Wait in 1 second intervals to check if IsExit changed
                while (!IsExit) await Task.Delay(TimeSpan.FromSeconds(1));

                await Client.LogoutAsync();
            }

            async Task InstallCommands()
            {
                Client.MessageReceived += MessageReceived;
                await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
            }

            async Task MessageReceived(SocketMessage msg)
            {
                if (msg is SocketUserMessage message)
                {
                    int argPos = 0;
                    if (!message.HasStringPrefix("!bot ", ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos)
                       || message.Author.IsBot) return;

                    var context = new CommandContext(Client, message);

                    var result = await Commands.ExecuteAsync(context, argPos);

                    if (!result.IsSuccess)
                        Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}