using Discord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Extensions;

namespace DiscordBot
{
    internal class Program : IDisposable
    {
        // ReSharper disable once InconsistentNaming
        private const string _DBPROVIDERANDSOURCE = "Data Source = DiscordBot.sqlite;Version=3";

        private DiscordClient _client;
        private List<Command> _allCommands = new List<Command>();
        private List<DiscordUser> _allUsers = new List<DiscordUser>();
        private string _botToken = "";
        private string[] _briahna, _briahnansfw, _molly, _woo;

        private static void Main(string[] args) => new Program().Start().Wait();

        #region Load

        /// <summary>Loads everything from the database and from disk.</summary>
        /// <returns>Returns true if completed successfully</returns>
        private async Task<bool> LoadAll()
        {
            Console.Title = "Discord Bot";
            bool success = false;
            await Task.Factory.StartNew(() =>
            {
                SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
                SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM BotToken", con);
                DataSet ds = new DataSet();

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
                            string nick = ds.Tables[0].Rows[i]["Nicknames"].ToString();
                            List<string> nicknames = nick.Split(',').Select(p => p.ToLower().Trim()).ToList();

                            DiscordUser newUser = new DiscordUser(
                                name: ds.Tables[0].Rows[i]["Name"].ToString(),
                                description: ds.Tables[0].Rows[i]["Description"].ToString(),
                                github: ds.Tables[0].Rows[i]["GitHub"].ToString(),
                                project: ds.Tables[0].Rows[i]["Project"].ToString(),
                                nicknames: nicknames);
                            _allUsers.Add(newUser);
                        }
                    }
                    _allUsers = _allUsers.OrderBy(user => user.Name).ToList();

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
                            _allCommands.Add(newCommand);
                        }
                    }
                    _allCommands = _allCommands.OrderBy(command => command.Name.ToString()).ToList();

                    _briahna = Directory.GetFiles("Briahna", "*.*");
                    _briahnansfw = Directory.GetFiles("Briahna\\NSFW", "*.*");
                    _molly = Directory.GetFiles("Molly", "*.*");
                    _woo = Directory.GetFiles("Woo", "*.*");
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
        /// <param name="username">Username of player roller dice</param>
        /// <param name="rolledDice">Dice command</param>
        /// <returns>True if successful</returns>
        private static string DiceGame(string username, string rolledDice)
        {
            if (rolledDice.Length > 0)
            {
                string[] dice = rolledDice.Split('d', 'D');
                if (dice.Length == 2)
                {
                    int numberOfDice = Int32Helper.Parse(dice[0]);
                    int sidesOnDice = Int32Helper.Parse(dice[1]);

                    try
                    {
                        checked
                        {
                            if (numberOfDice == 0 && sidesOnDice != 0)
                                numberOfDice = 1;

                            if (numberOfDice <= 0 || sidesOnDice <= 0)
                                return "Please enter a valid dice roll. (e.g. 2d10)";

                            string output = username + " rolls";
                            output += RollDice(numberOfDice, sidesOnDice);
                            return output;
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
        private static string RollDice(int numberOfDice, int sidesOnDice)
        {
            int[] values = new int[numberOfDice];

            for (int i = 0; i < numberOfDice; i++)
                values[i] = Functions.GenerateRandomNumber(1, sidesOnDice);

            string output = numberOfDice >= 30 || sidesOnDice >= 30
                ? " " + numberOfDice + "d" + sidesOnDice + "."
                : ": " + string.Join(", ", values);

            return output + ("\n\nTotal Value: " + values.Sum().ToString("N0"));
        }

        #endregion Dice Game

        /// <summary>Returns a List of users matching that nickname.</summary>
        /// <param name="username">Name to be matched</param>
        /// <returns>Returns a List of matching users</returns>
        private List<DiscordUser> GetMatchingUsers(string username)
        {
            if (username.Contains("@"))
                username = username.Substring(username.IndexOf("@") + 1);
            return _allUsers.Where(user => user.Nicknames.Contains(username)).ToList();
        }

        /// <summary>Displays all commands in the database.</summary>
        /// <returns>Return text regarding all commands in the database.</returns>
        private string Help()
        {
            string output = "These are all the commands I can do: \n\n";
            foreach (Command command in _allCommands)
                output += command.NameAndDescription + "\n\n";
            return output;
        }

        /// <summary>Allows the caller to change their current project in the database.</summary>
        /// <param name="user">User whose project wants to be changed</param>
        /// <param name="project">New project</param>
        /// <returns>Returns true if database set successful</returns>
        private async Task<bool> SetProject(User user, string project)
        {
            bool success = false;

            List<DiscordUser> matchingUsers = GetMatchingUsers(user.Name.ToLower());

            if (matchingUsers.Count > 0)
            {
                matchingUsers[0].Project = project;
                SQLiteCommand cmd = new SQLiteCommand();
                SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
                cmd.CommandText = "UPDATE Users SET [Project] = @project WHERE [Name] = @name";
                cmd.Parameters.AddWithValue("@project", project);
                cmd.Parameters.AddWithValue("@name", user.ToString());
                await Task.Factory.StartNew(() =>
              {
                  try
                  {
                      cmd.Connection = con;
                      con.Open();
                      cmd.ExecuteNonQuery();
                      success = true;
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine(ex.Message);
                  }
                  finally { con.Close(); }
              });
            }

            return success;
        }

        /// <summary>Allows the caller to change their current GitHub link in the database.</summary>
        /// <param name="user">User whose GitHub link wants to be changed</param>
        /// <param name="github">New GitHub link</param>
        /// <returns>Returns true if database set successful</returns>
        private async Task<bool> SetGitHub(User user, string github)
        {
            bool success = false;

            List<DiscordUser> matchingUsers = GetMatchingUsers(user.Name.ToLower());

            if (matchingUsers.Count > 0)
            {
                matchingUsers[0].GitHub = github;
                SQLiteCommand cmd = new SQLiteCommand();
                SQLiteConnection con = new SQLiteConnection { ConnectionString = _DBPROVIDERANDSOURCE };
                cmd.CommandText = "UPDATE Users SET [GitHub] = @github WHERE [Name] = @name";
                cmd.Parameters.AddWithValue("@github", github);
                cmd.Parameters.AddWithValue("@name", user.ToString());
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally { con.Close(); }
                });
            }

            return success;
        }

        /// <summary>Allows the caller to issue a command to the bot.</summary>
        /// <param name="e">Information about the original message</param>
        /// <param name="selectedCommand">Command to be issued</param>
        /// <param name="parameters">Parameters regarding command</param>
        /// <returns>Returns true if command is successful.</returns>
        private async Task<bool> IssueCommand(MessageEventArgs e, Commands selectedCommand, string parameters)
        {
            bool me = parameters == "me";

            switch (selectedCommand)
            {
                case Commands.briahna:
                    await e.Channel.SendFile(_briahna[Functions.GenerateRandomNumber(0, _briahna.Length - 1)]);
                    break;

                case Commands.briahnansfw:
                    await e.Channel.SendFile(_briahnansfw[Functions.GenerateRandomNumber(0, _briahnansfw.Length - 1)]);
                    break;

                case Commands.dox:
                case Commands.whois:
                    if (parameters.Length > 0)
                    {
                        List<DiscordUser> whoIsUsers = GetMatchingUsers(!me ? parameters : e.Message.User.Name.ToLower());

                        if (whoIsUsers.Count > 0)
                            await e.Channel.SendMessage(whoIsUsers[0].Description);
                        else
                            await e.Channel.SendMessage("I don't know that user.");
                    }
                    else
                        await e.Channel.SendMessage("Please name someone I should dox.");
                    break;

                case Commands.help:
                    await e.Channel.SendMessage(Help());
                    break;

                case Commands.git:
                case Commands.github:
                    if (parameters.Length > 0)
                    {
                        List<DiscordUser> gitHubUsers = GetMatchingUsers(!me ? parameters : e.Message.User.Name.ToLower());

                        if (gitHubUsers.Count > 0)
                        {
                            string githubUsername = gitHubUsers[0].Name.Substring(0, gitHubUsers[0].Name.IndexOf("#"));
                            await e.Channel.SendMessage(githubUsername + "'s GitHub link is: " + gitHubUsers[0].GitHub);
                        }
                        else
                            await e.Channel.SendMessage("I don't know that user.");
                    }
                    else
                        await e.Channel.SendMessage("Please name someone whose GitHub I should link.");
                    break;

                case Commands.mention:
                case Commands.ping:
                    if (parameters.Length > 0)
                    {
                        bool success = false;

                        if (!me)
                        {
                            List<DiscordUser> pingUsers = GetMatchingUsers(parameters);
                            if (pingUsers.Count > 0)
                            {
                                string pingUsername = pingUsers[0].Name.Substring(0, pingUsers[0].Name.IndexOf("#"));
                                foreach (User user in e.Channel.Users)
                                {
                                    if (pingUsername.ToLower() == user.Name.ToLower())
                                    {
                                        await e.Channel.SendMessage(user.NicknameMention);
                                        success = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessage(e.Message.User.NicknameMention);
                            success = true;
                        }

                        if (!success)
                            await e.Channel.SendMessage("I don't know that user.");
                    }
                    else
                        await e.Channel.SendMessage("Please name someone I should ping.");
                    break;

                case Commands.proj:
                case Commands.project:
                    if (parameters.Length > 0)
                    {
                        List<DiscordUser> projectUsers = GetMatchingUsers(!me ? parameters : e.Message.User.Name.ToLower());

                        if (projectUsers.Count > 0)
                        {
                            string projUsername = projectUsers[0].Name.Substring(0, projectUsers[0].Name.IndexOf("#"));
                            await e.Channel.SendMessage(projUsername + "'s current project is: " + projectUsers[0].Project);
                        }
                        else
                            await e.Channel.SendMessage("I don't know that user.");
                    }
                    else
                        await e.Channel.SendMessage("Please name someone whose current project I should tell you.");
                    break;

                case Commands.molly:
                    await e.Channel.SendFile(_molly[Functions.GenerateRandomNumber(0, _molly.Length - 1)]);
                    break;

                case Commands.roll:
                    await e.Channel.SendMessage(DiceGame(e.Message.User.Name, parameters));
                    break;

                case Commands.sayhi:
                    if (parameters.Length > 0)
                    {
                        if (!me)
                        {
                            List<DiscordUser> sayHiUsers = GetMatchingUsers(parameters);

                            if (sayHiUsers.Count > 0)
                            {
                                string pingUsername = sayHiUsers[0].Name.Substring(0, sayHiUsers[0].Name.IndexOf("#"));
                                foreach (User user in e.Channel.Users)
                                {
                                    if (pingUsername.ToLower() == user.Name.ToLower())
                                    {
                                        await e.Channel.SendMessage("Hi, " + user.NicknameMention + ".");
                                        break;
                                    }
                                }
                            }
                            else
                                await e.Channel.SendMessage("Hi.");
                        }
                        else
                            await e.Channel.SendMessage("Hi, " + e.Message.User.NicknameMention + ".");
                    }
                    else
                        await e.Channel.SendMessage("Hi.");
                    break;

                case Commands.setgit:
                case Commands.setgithub:
                    if (await SetGitHub(e.Message.User, parameters))
                        await e.Channel.SendMessage("GitHub set successful.");
                    else
                        await e.Channel.SendMessage("Failure! GitHub set not successful.");
                    break;

                case Commands.setproj:
                case Commands.setproject:
                    if (await SetProject(e.Message.User, parameters))
                        await e.Channel.SendMessage("Project set successful.");
                    else
                        await e.Channel.SendMessage("Failure! Project set not successful.");
                    break;

                case Commands.shutdown:
                    if (e.Message.User.Name.ToLower() == "pfthroaway")
                        await _client.Disconnect();
                    else
                        await e.Channel.SendMessage("You're not the boss of me!");
                    break;

                case Commands.time:
                    await e.Channel.SendMessage("The date and time in UTC is: " + DateTime.UtcNow.ToString("dddd, yyyy/MM/dd hh:mm:ss tt") + "\n\nMy creator's local time is: " + DateTime.Now.ToString("dddd, yyyy/MM/dd hh:mm:ss tt"));
                    break;

                case Commands.whoami:
                    List<DiscordUser> whoAmIUsers = GetMatchingUsers(e.Message.User.Name.ToLower());
                    if (whoAmIUsers.Count > 0)
                        await e.Channel.SendMessage(whoAmIUsers[0].Description);
                    else
                        await e.Channel.SendMessage("I don't know you.");
                    break;

                default:
                    await e.Channel.SendMessage("Invalid command. Please type \"!bot help\" for a list of valid commands.");
                    break;

                case Commands.whoareyou:
                    await e.Channel.SendMessage("I'm a bot; bleep, bloop. I was created by the real PFthroaway.");
                    break;

                case Commands.woo:
                    await e.Channel.SendFile(_woo[Functions.GenerateRandomNumber(0, _woo.Length - 1)]);
                    break;
            }
            return true;
        }

        /// <summary>Starts the Discord bot.</summary>
        /// <returns>Returns Task</returns>
        private async Task Start()
        {
            await LoadAll();
            if (_botToken.Length > 0)
            {
                _client = new DiscordClient();

                _client.MessageReceived += async (s, e) =>
                {
                    if (e.Message.Text.Trim().ToLower().StartsWith("!bot"))
                    {
                        string message = e.Message.Text.Substring(e.Message.Text.ToLower().IndexOf("!bot") + 4).Trim();
                        string command;
                        string parameters = "";
                        if (message.Contains(" "))
                        {
                            command = message.Substring(0, message.IndexOf(" ")).ToLower();
                            if (!command.Contains("setproj") && !command.Contains("setgit"))
                                parameters = message.Substring(message.IndexOf(" ") + 1).ToLower();
                            else
                                parameters = message.Substring(message.IndexOf(" ") + 1);
                        }
                        else
                            command = message.ToLower();

                        Commands currentCommand;
                        if (Enum.TryParse(command, out currentCommand))
                            await IssueCommand(e, currentCommand, parameters);
                        else
                            await e.Channel.SendMessage("Invalid command. Please type \"!bot help\" for a list of valid commands.");
                    }
                };

                _client.ExecuteAndWait(async () =>
                {
                    await _client.Connect(_botToken, TokenType.Bot);
                    _client.SetGame("Type '!bot help' for help.");
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