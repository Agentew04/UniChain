using Newtonsoft.Json;
using System.Text;
using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI.Commands;

internal class PrintCommand : ICommand {
    public string Name { get; set; } = "print";

    public List<Flag> RequiredFlags { get; set; } = new() {
        { new("file", "f", true) }
    };
    public List<Flag> OptionalFlags { get; set; } = new() {
        { new("dump", "d") },
        { new("ident", "i", true, true) }
    };

    public ReturnCode Invoke(IEnumerable<Flag> flags) {
        string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.json";
        bool dumpRequested = flags.Any(x => x.Full == "dump");
        Flag? identFlag = flags.Where(x => x.Full == "ident").FirstOrDefault();
        string path = flags.Where(x => x.Full == "file").FirstOrDefault()?.Value ?? "";
        int ident = 0;
        if (identFlag is not null) {
            // -i included
            if(identFlag.Value != ""){ 
                try {
                    ident = int.Parse(identFlag.Value ?? "");
                } catch {
                    Console.WriteLine("Invalid ident value!");
                    return ReturnCode.InvalidArgumentValue;
                }
            } else {
                ident = 4; // default value
            }
        }
        path = Utils.SanitizePath(path, "unichain", ".chain");

        if (!File.Exists(path)) {
            Console.WriteLine("Blockchain not found!");
            return ReturnCode.BlockChainNotFound;
        }

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var parser = new BlockchainParser();
        Blockchain bc = parser.DeserializeBlockchain(fs);

        TextWriter sw;
        if (dumpRequested)
            sw = File.CreateText(dumppath);
        else
            sw = Console.Out;
        
        using var jw = new JsonTextWriter(sw);
        jw.Formatting = identFlag is null ? Formatting.None : Formatting.Indented;
        jw.Indentation = ident;
        
        JsonSerializer serializer = new();
        serializer.Serialize(jw, bc);

        if (dumpRequested) {
            sw.Dispose();
        }
        return ReturnCode.Success;
    }
    
    public void Help() {
        Console.WriteLine(@"
Mandatory flags for 'print' sub-command:
-f  --file      => A path for the file containing the blockchain

Optional flags for 'print' sub-command:
-d  --dump      => Dumps the blockchain in a file
-i  --ident     => Prints the blockchain with the given identation, default is 4");
    }
}
