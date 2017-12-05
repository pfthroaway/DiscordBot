using DiscordBot.Classes.Entities;
using ExtensionsCore;
using ExtensionsCore.DatabaseHelp;
using ExtensionsCore.DataTypeHelpers;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Classes.Database
{
    /// <summary>Represents SQLite interactions between the application and database.</summary>
    internal class SQLiteDatabaseInteraction
    {
        private const string _DATABASENAME = "DiscordBot.sqlite";
        private readonly string _con = $"Data Source = {_DATABASENAME}; foreign keys = true; Version = 3";

        /// <summary>Verifies that the requested database exists and that its file size is greater than zero. If not, it extracts the embedded database file to the local output folder.</summary>
        public void VerifyDatabaseIntegrity() => Functions.VerifyFileIntegrity(Assembly.GetExecutingAssembly().GetManifestResourceStream($"Assassin.{_DATABASENAME}"),
                _DATABASENAME);

        #region Insert User

        /// <summary>Adds a User to the database.</summary>
        /// <param name="id">User ID</param>
        /// <param name="name">Name</param>
        /// <returns>True if successful</returns>
        public async Task<bool> AddUser(ulong id, string name)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "INSERT INTO Users([ID], [Name])VALUES(@id, @name)"
            };

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Insert User

        #region Load User

        /// <summary>Turns a DataRow from the Users table in the database into a <see cref="DiscordUser"/>.</summary>
        /// <param name="dr">DataRow</param>
        /// <returns><see cref="DiscordUser"/></returns>
        private DiscordUser AssignUserFromDataRow(DataRow dr) => new DiscordUser(ULongHelper.Parse(dr["ID"]), dr["Name"].ToString(), dr["Info"].ToString(), dr["GitHub"].ToString(), dr["Project"].ToString());

        /// <summary>Loads a specific User from the database.</summary>
        /// <param name="id">User's ID</param>
        /// <returns><see cref="DiscordUser"/></returns>
        internal async Task<DiscordUser> LoadUser(ulong id)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "SELECT * FROM Users WHERE [ID] = @id"
            };
            cmd.Parameters.AddWithValue("@id", id);
            DataSet ds = await SQLite.FillDataSet(_con, cmd);
            return (ds.Tables[0].Rows.Count > 0) ? AssignUserFromDataRow(ds.Tables[0].Rows[0]) : new DiscordUser();
        }

        /// <summary>Loads all Users from the database.</summary>
        /// <returns>All Users</returns>
        internal async Task<List<DiscordUser>> LoadAllUsers()
        {
            List<DiscordUser> allUsers = new List<DiscordUser>();

            DataSet ds = await SQLite.FillDataSet(_con, "SELECT * FROM Users");
            if (ds.Tables[0].Rows.Count > 0)
                allUsers.AddRange(from DataRow dr in ds.Tables[0].Rows select AssignUserFromDataRow(dr));

            return allUsers;
        }

        #endregion Load User

        #region Load BotToken

        /// <summary>Loads the bot token from the database.</summary>
        /// <returns>Bot token</returns>
        internal async Task<string> LoadBotToken()
        {
            string token = "";
            DataSet ds = await SQLite.FillDataSet(_con, "SELECT * FROM BotToken");
            if (ds.Tables[0].Rows.Count > 0)
                token = ds.Tables[0].Rows[0]["Token"].ToString();
            return token;
        }

        #endregion Load BotToken

        #region Update User

        /// <summary>Updates the database with a user's GitHub link.</summary>
        /// <param name="id">User ID</param>
        /// <param name="github">Link to GitHub</param>
        /// <returns>True if successful</returns>
        internal async Task<bool> SetGitHub(ulong id, string github)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Users SET [GitHub] = @github WHERE [ID] = @id"
            };

            cmd.Parameters.AddWithValue("@github", github);
            cmd.Parameters.AddWithValue("@id", id);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Updates the database with a user's information.</summary>
        /// <param name="id">User ID</param>
        /// <param name="info">Information</param>
        /// <returns>True if successful</returns>
        internal async Task<bool> SetInfo(ulong id, string info)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Users SET [Info] = @info WHERE [ID] = @id"
            };

            cmd.Parameters.AddWithValue("@info", info);
            cmd.Parameters.AddWithValue("@id", id);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        /// <summary>Updates the database with a user's current project.</summary>
        /// <param name="id">User ID</param>
        /// <param name="project">Current Project</param>
        /// <returns>True if successful</returns>
        internal async Task<bool> SetProject(ulong id, string project)
        {
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "UPDATE Users SET [Project] = @project WHERE [ID] = @id"
            };

            cmd.Parameters.AddWithValue("@project", project);
            cmd.Parameters.AddWithValue("@id", id);

            return await SQLite.ExecuteCommand(_con, cmd);
        }

        #endregion Update User
    }
}