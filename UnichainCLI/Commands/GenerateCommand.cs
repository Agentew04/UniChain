using System.IO;
using Unichain.Core;

namespace Unichain.CLI.Commands;

internal class GenerateCommand : ICommand {
    public string Name { get; set; } = "generate";
    public List<Flag> RequiredFlags { get; set; } = new() {

    };
    public List<Flag> OptionalFlags { get; set; } = new() {
        { new("number", "n", true) },
        { new("dump", "d") }
    };

    public void Help() {
        Console.WriteLine(@"
Possible flags for 'generate' sub-command:
  -n  --number => Sets the number of addresses and users to generate, defaults to 1
  -d  --dump   => Dumps all information to a text file");
    }

    public ReturnCode Invoke(IEnumerable<Flag> flags) {
        bool dumpRequested = flags.Any(x => x.Full == "dump");
        bool numberSpecified = flags.Any(x => x.Full == "number");
        int number = 1;
        if(numberSpecified) {
            string numStr = flags.Where(x => x.Full == "number").FirstOrDefault()?.Value ?? "";
            try {
                number = int.Parse(numStr);
            } catch {
                Console.WriteLine("Invalid number specified!");
                return ReturnCode.InvalidArgumentValue;
            }
        }

        if(number <= 0) {
            Console.WriteLine("Number must be greater than 0!");
            return ReturnCode.InvalidArgumentValue;
        }
        
        // todo convert this to json
        string dumpPath = Path.Combine(Environment.CurrentDirectory, $"dump-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.json");
        TextWriter sw;
        if (dumpRequested)
            sw = File.AppendText(dumpPath);
        else
            sw = Console.Out;

        //sw.WriteLine($"address, publicKey, privateKey");
        sw.Write("[\n");
        for (int i = 0; i < number; i++) {
            User user = new();
            sw.Write("\t{\n");
            sw.Write($"\t\t\"address\": \"{user.Address}\",\n");
            sw.Write($"\t\t\"publicKey\": \"{user.PublicKey}\",\n");
            sw.Write($"\t\t\"privateKey\": \"{user.PrivateKey}\"\n");
            sw.Write("\t}");
            if (i != number - 1)
                sw.Write(',');
            sw.Write('\n');
        }
        sw.Write("]\n");

        if (dumpRequested) // don't dispose Console.Out (it should not throw an error but just in case)
            sw.Dispose();

        return ReturnCode.Success;
    }
}
