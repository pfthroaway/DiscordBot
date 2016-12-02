using System;
using System.Collections.Generic;

namespace DiscordBot
{
    /// <summary>Represents a user on Discord.</summary>
    internal class DiscordUser
    {
        private string _name, _description, _github, _project;
        private List<string> _nicknames = new List<string>();

        #region Properties

        /// <summary>Name of Command</summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>Description of Command</summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>Name and Description of Command</summary>
        internal List<string> Nicknames
        {
            get { return _nicknames; }
            set { _nicknames = value; }
        }

        public string GitHub
        {
            get { return _github; }
            set { _github = value; }
        }

        public string Project
        {
            get { return _project; }
            set { _project = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>Initializes a default instance of DiscordUser.</summary>
        public DiscordUser()
        {
        }

        /// <summary>Initializes an instance of DiscordUser by assinging Properties.</summary>
        /// <param name="name">Name of DiscordUser</param>
        /// <param name="description">Description of DiscordUser</param>
        /// <param name="nicknames">Nicknames used by DiscordUser</param>
        public DiscordUser(string name, string description, string github, string project, List<String> nicknames)
        {
            Name = name;
            Description = description;
            GitHub = github;
            Project = project;
            Nicknames = nicknames;
        }

        /// <summary>Replaces this instance of DiscordUser with another instance.</summary>
        /// <param name="otherUser"></param>
        public DiscordUser(DiscordUser otherUser)
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