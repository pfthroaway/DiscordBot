using Discord.WebSocket;
using DiscordBot.Classes.Database;
using DiscordBot.Classes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBot.Classes
{
    /// <summary>Represents the current state of the application.</summary>
    internal static class AppState
    {
        ///<summary>Bot owner</summary>
        internal static SocketUser PF;

        internal static List<DiscordUser> AllUsers = new List<DiscordUser>();
        internal static SQLiteDatabaseInteraction DatabaseInteraction = new SQLiteDatabaseInteraction();

        /// <summary>Loads everything from the database.</summary>
        /// <param name="client">Discord client</param>
        internal static async void LoadAll(DiscordSocketClient client)
        {
            DatabaseInteraction.VerifyDatabaseIntegrity();
            PF = client.GetUser(227367559861633025);
            AllUsers = await DatabaseInteraction.LoadAllUsers();

            //checks for new Users on bot loading
            foreach (SocketGuild guild in client.Guilds)
            {
                foreach (SocketUser user in guild.Users)
                {
                    if (!AllUsers.Any(usr => usr.Id == user.Id))
                        await DatabaseInteraction.AddUser(user.Id, user.Username);
                }
            }
        }

        private static void CompareUsers()
        {
        }
    }
}