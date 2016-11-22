using Discord;
using DiscordBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().Start();

        private DiscordClient _client;

        public void Start()
        {
            _client = new DiscordClient();

            _client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                    await e.Channel.SendMessage(e.Message.Text);
            };

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect("", TokenType.Bot);
            });
        }
    }
}