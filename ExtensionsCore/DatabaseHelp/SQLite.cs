using System;
using System.Data.SQLite;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ExtensionsCore.DatabaseHelp
{
    /// <summary>Provides an extension into SQLite commands.</summary>
    public static class SQLite
    {
        /// <summary>This method fills a DataSet with data from a table.</summary>
        /// <param name="sql">SQL query to be executed</param>
        /// <param name="con">Connection information</param>
        /// <returns>Returns DataSet with queried results</returns>
        public static async Task<DataSet> FillDataSet(string con, string sql) => await FillDataSet(con, new SQLiteCommand { CommandText = sql });

        /// <summary>This method fills a DataSet with data from a table.</summary>
        /// <param name="cmd">SQLite command to be executed</param>
        /// <param name="con">Connection information</param>
        /// <returns>Returns DataSet with queried results</returns>
        public static async Task<DataSet> FillDataSet(string con, SQLiteCommand cmd)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrWhiteSpace(con))
            {
                SQLiteConnection connection = new SQLiteConnection(con);
                cmd.Connection = connection;
                await Task.Run(async () =>
                {
                    try
                    {
                        SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                        da.Fill(ds);
                    }
                    catch (Exception ex)
                    {
                        await LogError(con, ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                });
            }
            else
                await LogError(con, "Connection string cannot be empty!");

            return ds;
        }

        /// <summary>Executes commands.</summary>
        /// <param name="con">Connection information</param>
        /// <param name="commands">Commands to be executed</param>
        /// <returns>Returns true if command(s) executed successfully</returns>
        public static async Task<bool> ExecuteCommand(string con, params SQLiteCommand[] commands)
        {
            bool success = false;
            if (!string.IsNullOrWhiteSpace(con))
            {
                SQLiteConnection connection = new SQLiteConnection(con);

                await Task.Run(async () =>
                {
                    try
                    {
                        connection.Open();
                        foreach (SQLiteCommand command in commands)
                        {
                            command.Connection = connection;
                            command.Prepare();
                            command.ExecuteNonQuery();
                        }
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        await LogError(con, ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                });
            }
            else
                await LogError(con, "Connection string cannot be empty!");

            return success;
        }

        /// <summary>Logs an error to the database.</summary>
        /// <param name="con">Database connection string</param>
        /// <param name="error">Error to be logged</param>
        internal static async Task LogError(string con, string error)
        {
            SQLiteConnection connection = new SQLiteConnection(con);
            SQLiteCommand cmd = new SQLiteCommand
            {
                CommandText = "INSERT INTO Errors (Error, Time)VALUES(@error, @time)"
            };
            cmd.Parameters.AddWithValue("@error", error);
            cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt"));
            await ExecuteCommand(con, cmd);
        }
    }
}