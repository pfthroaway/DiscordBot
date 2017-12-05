using Discord;
using Discord.Commands;
using ExtensionsCore;
using ExtensionsCore.DataTypeHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Classes.Commands
{
    public class MyCommands : ModuleBase
    {
        /// <summary>Displays all commands.</summary>
        [Command("help")]
        [Summary("Lists all the commands I can do.")]
        [Alias("?")]
        public async Task Help()
        {
            EmbedBuilder eb = new EmbedBuilder();

            MethodInfo[] mi = GetType().GetMethods();
            for (int o = 0; o < mi.Length; o++)
            {
                CommandAttribute myAttribute1 = mi[o].GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault();
                SummaryAttribute myAttribute2 = mi[o].GetCustomAttributes(true).OfType<SummaryAttribute>().FirstOrDefault();
                if (myAttribute1 != null && myAttribute2 != null)
                    eb.AddField(myAttribute1.Text, myAttribute2.Text);
            }

            await ReplyAsync("", false, eb);
        }

        #region Dice Game

        /// <summary>Rolls dice.</summary>
        /// <param name="rolledDice">Number of dice and sides of dice to be rolled.</param>
        [Command("roll")]
        [Summary("Rolls a set of dice in the fashion of \"2d10\".")]
        public async Task Roll([Remainder]string rolledDice)
        {
            if (rolledDice.Length > 0)
            {
                string[] dice = rolledDice.ToLower().Split('d');
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

                            if (numberOfDice > 0 && sidesOnDice > 0)
                            {
                                string output = Context.User.Username + " rolls";
                                output += RollDice(numberOfDice, sidesOnDice);
                                await ReplyAsync(output);
                            }
                            else
                                await ReplyAsync("Please enter a valid dice roll. (e.g. 2d10)");
                        }
                    }
                    catch (Exception)
                    {
                        await ReplyAsync("That combination of amount of dice and sides on the dice exceeds the maximum integer value of 2,147,483,647.");
                    }
                }
                else
                    await ReplyAsync("Please enter a valid dice roll. (e.g. 2d10)");
            }
            else
                await ReplyAsync("Please enter a valid dice roll. (e.g. 2d10)");
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

        #region GitHub

        /// <summary>Displays the GitHub link for the selected user.</summary>
        /// <param name="u">User</param>
        [Command("github")]
        [Summary("Displays the GitHub link for the selected user.")]
        [Alias("git")]
        public async Task GitHub(IUser u) => await ReplyAsync(AppState.AllUsers.Find(user => user.Id == u.Id).GitHub);

        /// <summary>Sets GitHub link for the selected User</summary>
        /// <param name="github">GitHub link for the selected User</param>
        [Command("setgithub")]
        [Summary("Sets GitHub link for the selected User.")]
        [Alias("setgit")]
        public async Task SetGitHub([Remainder]string github)
        {
            if (await AppState.DatabaseInteraction.SetGitHub(Context.User.Id, github))
            {
                await ReplyAsync("GitHub set successfully.");
                AppState.AllUsers.Find(user => user.Id == Context.User.Id).GitHub = github;
            }
            else await ReplyAsync("GitHub not set.");
        }

        #endregion GitHub

        #region Info

        /// <summary>Displays information about the selected user.</summary>
        /// <param name="u">User</param>
        [Command("info")]
        [Summary("Displays information about the selected user.")]
        public async Task Info(IUser u) => await ReplyAsync(AppState.AllUsers.Find(user => user.Id == u.Id).Info);

        /// <summary>Sets information about the selected user.</summary>
        /// <param name="info">Information about the selected user</param>
        [Command("setinfo")]
        [Summary("Sets information about User.")]
        public async Task SetInfo([Remainder]string info)
        {
            if (await AppState.DatabaseInteraction.SetInfo(Context.User.Id, info))
            {
                await ReplyAsync("Information set successfully.");
                AppState.AllUsers.Find(user => user.Id == Context.User.Id).Info = info;
            }
            else await ReplyAsync("Information not set.");
        }

        #endregion Info

        #region Ping

        [Command("ping")]
        [Summary("Pings a user.")]
        [Alias("p")]
        public async Task Ping(IUser u) => await ReplyAsync(MentionUtils.MentionUser(u.Id));

        #endregion Ping

        #region Project

        /// <summary>Displays the current project for the selected user.</summary>
        /// <param name="u">User</param>
        [Command("project")]
        [Summary("Displays the current project for the selected user.")]
        [Alias("proj")]
        public async Task Project(IUser u) => await ReplyAsync(AppState.AllUsers.Find(user => user.Id == u.Id).Project);

        /// <summary>Sets current project for the selected User</summary>
        /// <param name="project">current project for the selected User</param>
        [Command("setproject")]
        [Summary("Sets current project for the selected User.")]
        [Alias("setproj")]
        public async Task SetProject([Remainder]string project)
        {
            if (await AppState.DatabaseInteraction.SetProject(Context.User.Id, project))
            {
                await ReplyAsync("Project set successfully.");
                AppState.AllUsers.Find(user => user.Id == Context.User.Id).Project = project;
            }
            else await ReplyAsync("Project not set.");
        }

        #endregion Project

        #region Shutdown

        /// <summary>Allows the owner to shutdown the bot.</summary>
        [Command("shutdown")]
        [Summary("Allows the owner to shutdown the bot.")]
        [Alias("sd")]
        public async Task Shutdown()
        {
            if (Context.User.Id == AppState.PF.Id)
            {
                await ReplyAsync("Shutting down...");
                Program.IsExit = true;
            }
            else
                await ReplyAsync("You're not the boss of me!");
        }

        #endregion Shutdown

        #region Time

        /// <summary>Displays the current time.</summary>
        [Command("time")]
        [Summary("Displays the current time.")]
        public async Task Time() => await ReplyAsync($"The date and time in UTC is: {DateTime.UtcNow:dddd, yyyy/MM/dd hh:mm:ss tt}\n\nMy creator's local time is: {DateTime.Now:dddd, yyyy/MM/dd hh:mm:ss tt}");

        #endregion Time
    }
}