namespace Unichain.CLI
{
    public struct Flag
    {

        public string Simplified { get; init; }
        public string Full { get; init; }

        public Flag(string fullname, string simpleName)
        {
            Simplified = "-" + simpleName;
            Full = "--" + fullname;
        }
    }
}
