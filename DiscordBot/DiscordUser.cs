using System;
using System.Collections.Generic;

namespace DiscordBot
{
    /// <summary>Represents a user on Discord.</summary>
    internal class DiscordUser
    {
        #region Properties

        /// <summary>Name of Command</summary>
        internal string Name { get; }

        /// <summary>Description of Command</summary>
        internal string Description { get; }

        /// <summary>Name and Description of Command</summary>
        internal List<string> Nicknames { get; } = new List<string>();

        internal string GitHub { get; set; }

        internal string Project { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>Initializes a default instance of DiscordUser.</summary>
        internal DiscordUser()
        {
        }

        /// <summary>Initializes an instance of DiscordUser by assinging Properties.</summary>
        /// <param name="name">Name of DiscordUser</param>
        /// <param name="description">Description of DiscordUser</param>
        /// <param name="github">GitHub link of DiscordUser</param>
        /// <param name="project">Current project of of DiscordUser</param>
        /// <param name="nicknames">Nicknames used by DiscordUser</param>
        internal DiscordUser(string name, string description, string github, string project, List<String> nicknames)
        {
            Name = name;
            Description = description;
            GitHub = github;
            Project = project;
            Nicknames = nicknames;
        }

        /// <summary>Replaces this instance of DiscordUser with another instance.</summary>
        /// <param name="otherUser"></param>
        internal DiscordUser(DiscordUser otherUser)
        {
            Name = otherUser.Name;
            Description = otherUser.Description;
            GitHub = otherUser.GitHub;
            Project = otherUser.Project;
            Nicknames = otherUser.Nicknames;
        }

        #endregion Constructors
    }
}