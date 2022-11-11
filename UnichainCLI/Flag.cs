using System.Runtime.Serialization.Formatters;

namespace Unichain.CLI
{
    public class Flag
    {
        /// <summary>
        /// The simplified flag name
        /// </summary>
        public string Simplified { get; init; }
        
        /// <summary>
        /// The full flag name
        /// </summary>
        public string Full { get; init; }

        /// <summary>
        /// If the flag has a value associated with it
        /// </summary>
        public bool HasValue { get; init; }

        /// <summary>
        /// When the Flag has value, if the value can be empty(<see cref="string.Empty"></see>)
        /// </summary>
        public bool CanBeEmpty { get; init; }

        /// <summary>
        /// The value of the flag, null if <see cref="HasValue"/> is false
        /// </summary>
        public string? Value { get; set; }

        public Flag(string fullname, string simpleName, bool hasValue = false, bool canBeEmpty = false, string? value = null) {
            Simplified = simpleName;
            Full = fullname;
            HasValue = hasValue;
            CanBeEmpty = canBeEmpty;
            Value = value;
        }

        public static bool TryGetFlagValue(string[] args, Flag flag, out string value) {
            value = "";
            if (!flag.HasValue)
                return false;
            if (args == null || args.Length == 0) 
                return false;
            if (!args.Any(x => flag.Full==x || flag.Simplified==x))
                return false;

            int flagindex = -1;
            for (int i = 0; i < args.Length; i++) {
                if (args[i] == flag.Simplified || args[i] == flag.Full) {
                    flagindex = i;
                    break;
                }
            }
            if (flagindex == args.Length - 1)
                return false;

            // flag not found
            if (flagindex == -1)
                return false;
            
            value = args[flagindex + 1];

            // check if value is not other flag( has no value)
            if(value.StartsWith("-") || value.StartsWith("--"))
                return false;
            
            return true;
        }

        public static bool HasFlag(string[] args, Flag flag) {
            if (args == null || args.Length == 0)
                return false;
            return args.Any(x => x == flag.Full || x == flag.Simplified);
        }

        public Flag Clone() {
            return new Flag(Full, Simplified, HasValue, CanBeEmpty, Value);
        }
    }
}
