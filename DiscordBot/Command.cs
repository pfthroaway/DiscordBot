namespace DiscordBot
{
    /// <summary>Represents a command issued to the bot.</summary>
    internal class Command
    {
        #region Properties

        /// <summary>Name of Command</summary>
        internal Commands Name { get; }

        /// <summary>Description of Command</summary>
        private string Description { get; }

        /// <summary>Name and Description of Command</summary>
        internal string NameAndDescription => "**" + Name + "**  -  " + Description;

        #endregion Properties

        #region Constructors

        /// <summary>Initializes a default instance of Command.</summary>
        internal Command()
        {
        }

        /// <summary>Initializes an instance of Command by assinging Properties.</summary>
        /// <param name="name">Name of Command</param>
        /// <param name="description">Description of Command</param>
        internal Command(Commands name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>Replaces this instance of Command with another instance.</summary>
        /// <param name="otherCommand">Instance of Command to replace this instance</param>
        internal Command(Command otherCommand)
        {
            Name = otherCommand.Name;
            Description = otherCommand.Description;
        }

        #endregion Constructors
    }
}