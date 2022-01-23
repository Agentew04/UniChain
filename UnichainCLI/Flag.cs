namespace Unichain.CLI
{
    public struct Flag
    {
        private string _Simplified;

        public string Simplified
        {
            get { return _Simplified; }
            set { _Simplified = "-" + value; }
        }


        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = "--" + value; }
        }

        public Flag(string name, string simple)
        {
            _Simplified = simple;
            _Name = name;
        }
    }
}
