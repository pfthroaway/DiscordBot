using Discord;
using DiscordBot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class Program : IDisposable
    {
        private const string _DBPROVIDERANDSOURCE = "Data Source = DiscordBot.sqlite;Version=3";

        private string nl = Environment.NewLine;
        private DiscordClient _client;
        private List<Command> AllCommands = new List<Command>();
        private List<DiscordUser> AllUsers = new List<DiscordUser>();
        private string _botToken = "";

        #region Random Number Generation

        /// <summary>
        /// Generates a random number between min and max (inclusive).
        /// </summary>
        /// <param name="min">Inclusive minimum number</param>
        /// <param name="max">Inclusive maximum number</param>
        /// <returns>Returns randomly generated integer between min and max.</returns>
        internal static int GenerateRandomNumber(int min, int max)
        {
            return GenerateRandomNumber(min, max, int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Generates a random number between min and max (inclusive).
        /// </summary>
        /// <param name="min">Inclusive minimum number</param>
        /// <param name="max">Inclusive maximum number</param>
        /// <param name="upperLimit">Maximum limit for the method, regardless of min and max.</param>
        /// <returns>Returns randomly generated integer between min and max with an upper limit of upperLimit.</returns>
        internal static int GenerateRandomNumber(int min, int max, int lowerLimit, int upperLimit)
        {
            int result;

            if (min < max)
                result = ThreadSafeRandom.ThisThreadsRandom.Next(min, max + 1);
            else
                result = ThreadSafeRandom.ThisThreadsRandom.Next(max, min + 1);

            if (result < lowerLimit)
                return lowerLimit;
            if (result > upperLimit)
                return upperLimit;

            return result;
        }

        #endregion Random Number Generation

        private static void Main(string[] args) => new Program().Start().Wait();

        #region Load

        private async Task<bool> LoadAll()
        {
            bool success = false;
            await Task.Factory.StartNew(() =>
            {
                SQLiteConnection con = new SQLiteConnection();
                SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM BotToken", con);
                DataSet ds = new DataSet();
                con.ConnectionString = _DBPROVIDERANDSOURCE;

                try
                {
                    da.Fill(ds, "BotToken");

                    if (ds.Tables[0].Rows.Count > 0)
                        _botToken = ds.Tables[0].Rows[0]["Token"].ToString();

                    ds = new DataSet();
                    da = new SQLiteDataAdapter("SELECT * FROM Users", con);
                    da.Fill(ds, "Users");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            List<string> nicknames = new List<string>();
                            string nick = ds.Tables[0].Rows[i]["Nicknames"].ToString();
                            nicknames = nick.Split(',').Select(p => p.ToLower().Trim()).ToList();

                            DiscordUser newUser = new DiscordUser(
                                name: ds.Tables[0].Rows[i]["Name"].ToString(),
                                description: ds.Tables[0].Rows[i]["Description"].ToString(),
                                nicknames: nicknames);
                            AllUsers.Add(newUser);
                        }
                    }
                    AllUsers = AllUsers.OrderBy(user => user.Name).ToList();

                    ds = new DataSet();
                    da = new SQLiteDataAdapter("SELECT * FROM Commands", con);
                    da.Fill(ds, "Commands");

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            Commands newCommandName;
                            Enum.TryParse(ds.Tables[0].Rows[i]["Name"].ToString(), out newCommandName);
                            Command newCommand = new Command(
                                name: newCommandName,
                                description: ds.Tables[0].Rows[i]["Description"].ToString());
                            AllCommands.Add(newCommand);
                        }
                    }
                    AllCommands = AllCommands.OrderBy(command => command.Name).ToList();
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally { con.Close(); }
            });
            return success;
        }

        #endregion Load

        #region Dice Game

        /// <summary>Plays a game of dice using the #d# system.</summary>
        /// <param name="e">MessageEventArgs</param>
        /// <param name="rolledDice">Dice command</param>
        /// <returns>True if successful</returns>
        private string DiceGame(string username, string rolledDice)
        {
            string[] dice = new string[2];
            int sidesOnDice = 0;
            int numberOfDice = 0;

            if (rolledDice.Length > 0)
            {
                dice = rolledDice.Split('d', 'D');
                if (dice.Count() == 2)
                {
                    numberOfDice = Int32Helper.Parse(dice[0]);
                    sidesOnDice = Int32Helper.Parse(dice[1]);

                    try
                    {
                        checked
                        {
                            if (numberOfDice == 0 && sidesOnDice != 0)
                                numberOfDice = 1;

                            if (numberOfDice <= 0 || sidesOnDice <= 0)
                                return "Please enter a valid dice roll. (e.g. 2d10)";
                            else
                            {
                                string output = username + " rolls";
                                output += RollDice(numberOfDice, sidesOnDice);
                                return output;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return "That combination of amount of dice and sides on the dice exceeds the maximum integer value of 2,147,483,647.";
                    }
                }
                else
                    return "Please enter a valid dice roll. (e.g. 2d10)";
            }
            return "Please enter a valid dice roll. (e.g. 2d10)";
        }

        /// <summary>Rolls dice</summary>
        /// <param name="numberOfDice">Number of dice to be rolled</param>
        /// <param name="sidesOnDice">Number of sides on each die</param>
        /// <returns>Returns result string</returns>
        private string RollDice(int numberOfDice, int sidesOnDice)
        {
            string output = "";

            int[] values = new int[numberOfDice];
            int totalValue = 0;

            for (int i = 0; i < numberOfDice; i++)
            {
                values[i] = GenerateRandomNumber(1, sidesOnDice);
                totalValue += values[i];
            }

            if (numberOfDice >= 30 || sidesOnDice >= 30)
                output = " " + numberOfDice + "d" + sidesOnDice + ".";
            else
                output = ": " + string.Join(", ", values);

            return output += nl + nl + "Total Value: " + values.Sum();
        }

        #endregion Dice Game

        private List<DiscordUser> GetMatchingUsers(string username)
        {
            if (username.Contains("@"))
                username = username.Substring(username.IndexOf("@") + 1);
            return AllUsers.Where(user => user.Nicknames.Contains(username)).ToList();
        }

        private string Help()
        {
            string output = "These are all the commands I can do: " + nl + nl;
            for (int i = 0; i < AllCommands.Count; i++)
                output += AllCommands[i].NameAndDescription + nl + nl;
            return output;
        }

        private void LoadCommands()
        {
        }

        public async Task Start()
        {
            await LoadAll();
            if (_botToken.Length > 0)
            {
                _client = new DiscordClient();

                _client.MessageReceived += async (s, e) =>
                {
                    if (e.Message.Text.Trim().ToLower().StartsWith("!bot"))
                    {
                        string message = e.Message.Text.Substring(e.Message.Text.IndexOf("!bot") + 4).Trim().ToLower();
                        string command = "";
                        string parameters = "";
                        if (message.Contains(" "))
                        {
                            command = message.Substring(0, message.IndexOf(" ")).ToLower();
                            parameters = message.Substring(message.IndexOf(" ") + 1).ToLower();
                        }
                        else
                            command = message;

                        Commands currentCommand;
                        if (Enum.TryParse(command, out currentCommand))
                        {
                            switch (currentCommand)
                            {
                                case Commands.roll:
                                    await e.Channel.SendMessage(DiceGame(e.Message.User.Name, parameters));
                                    break;

                                case Commands.help:
                                    await e.Channel.SendMessage(Help());
                                    break;

                                case Commands.whois:
                                    if (parameters.Length > 0)
                                    {
                                        List<DiscordUser> whoIsUsers = GetMatchingUsers(parameters);
                                        if (whoIsUsers.Count > 0)
                                            await e.Channel.SendMessage(whoIsUsers[0].Description);
                                        else
                                            await e.Channel.SendMessage("I don't know that user.");
                                    }
                                    else
                                        await e.Channel.SendMessage("Please name someone I should dox.");
                                    break;

                                case Commands.whoareyou:
                                    await e.Channel.SendMessage("I'm a bot; bleep, bloop. I was created by the real PFthroaway.");
                                    break;

                                case Commands.shutdown:
                                    if (e.Message.User.Name.ToLower() == "pfthroaway")
                                        await _client.Disconnect();
                                    else
                                        await e.Channel.SendMessage("You're not the boss of me!");
                                    break;

                                case Commands.ping:
                                    if (parameters.Length > 0)
                                    {
                                        bool success = false;
                                        foreach (User user in e.Channel.Users)
                                        {
                                            if (parameters == user.Name.ToLower())
                                            {
                                                await e.Channel.SendMessage(user.NicknameMention);
                                                success = true;
                                                break;
                                            }
                                        }

                                        if (!success)
                                            await e.Channel.SendMessage("I don't know that user.");
                                    }
                                    else
                                        await e.Channel.SendMessage("Please name someone I should ping.");
                                    break;

                                default:
                                    await e.Channel.SendMessage("Invalid command.");
                                    break;
                            }
                        }
                        else
                            await e.Channel.SendMessage("Invalid command.");
                    }
                };

                _client.ExecuteAndWait(async () =>
                {
                    await _client.Connect(_botToken, TokenType.Bot);
                });
            }
            else
            {
                Console.WriteLine("The bot token is missing.");
                Console.ReadLine();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}