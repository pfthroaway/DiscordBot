namespace DiscordBot.Classes.Entities
{
    /// <summary>Represents an instance of a User from Discord in my database.</summary>
    internal class DiscordUser
    {
        /// <summary>User ID</summary>
        public ulong Id { get; set; }

        /// <summary>User's name</summary>
        public string Name { get; set; }

        /// <summary>User's info</summary>
        public string Info { get; set; }

        /// <summary>User's GitHub link</summary>
        public string GitHub { get; set; }

        /// <summary>User's current project</summary>
        public string Project { get; set; }

        #region Constructors

        /// <summary>Initializes a default instance of <see cref="DiscordUser"/>.</summary>
        public DiscordUser()
        {
        }

        /// <summary>Initializes an instance of <see cref="DiscordUser"/> by assigning Property values.</summary>
        /// <param name="id">User ID</param>
        /// <param name="name">User's name</param>
        /// <param name="info">User's info</param>
        /// <param name="github">User's GitHub link</param>
        /// <param name="project">User's current project</param>
        public DiscordUser(ulong id, string name, string info, string github, string project)
        {
            Id = id;
            Name = name;
            Info = info;
            GitHub = github;
            Project = project;
        }

        /// <summary>Replaces this instance of <see cref="DiscordUser"/> with another instance.</summary>
        /// <param name="other">Instance to replace this instance</param>
        public DiscordUser(DiscordUser other) : this(other.Id, other.Name, other.Info, other.GitHub, other.Project)
        {
        }

        #endregion Constructors
    }
}