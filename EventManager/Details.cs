namespace EventManager
{
    public class DetailsAttribute : System.Attribute
    {
        public readonly string author;
        public readonly int EVENT_MAJOR;
        public readonly int EVENT_MINOR;
        public readonly int SMOD_MAJOR;
        public readonly int SMOD_MINOR;
        public readonly string version;

        public DetailsAttribute(string author, int EVENT_MAJOR, int EVENT_MINOR, int SMOD_MAJOR, int SMOD_MINOR, string version)
        {
            this.author = author;
            this.EVENT_MAJOR = EVENT_MAJOR;
            this.EVENT_MINOR = EVENT_MINOR;
            this.SMOD_MAJOR = SMOD_MAJOR;
            this.SMOD_MINOR = SMOD_MINOR;
            this.version = version;
        }
    }
}
