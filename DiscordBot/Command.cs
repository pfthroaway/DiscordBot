namespace DiscordBot
{
    /// <summary>Represents a command issued to the bot.</summary>
    internal class Command
    {
        private Commands _name;
        private string _description;

        #region Properties

        /// <summary>Name of Command</summary>
        public Commands Name
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
        internal string NameAndDescription
        {
            get { return Name.ToString() + "  -  " + Description; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>Initializes a default instance of Command.</summary>
        public Command()
        {
        }

        /// <summary>Initializes an instance of Command by assinging Properties.</summary>
        /// <param name="name">Name of Command</param>
        /// <param name="description">Description of Command</param>
        public Command(Commands name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>Replaces this instance of Command with another instance.</summary>
        /// <param name="otherCommand">Instance of Command to replace this instance</param>
        public Command(Command otherCommand)
        {
            Name = otherCommand.Name;
            Description = otherCommand.Description;
        }

        #endregion Constructors
    }
}