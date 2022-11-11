using Newtonsoft.Json;
using Unichain.Core;

namespace Unichain.CLI.Commands;

internal class GetCommand : ICommand {
    public string Name { get; set; } = "get";
    public List<Flag> RequiredFlags { get; set; } = new List<Flag>() {
        { new("file", "f", true) },
        
    };
    public List<Flag> OptionalFlags { get; set; } = new List<Flag>() {
        { new("user", "u", true, true) },
        { new("dump", "d") },
        { new("all", "a") },
        { new("balance", "bal") },
        { new("currency", "cur") },
        { new("nfts", "nft") },
        { new("pools", "pool") },
        { new("messages", "msg") },
        { new("documents", "doc") }
    };

    private static List<string> ParseAddress(string addressValues) {
        var addresses = addressValues.Split(',').Select(x => x.Trim());

        // empty
        if(addresses is null || addresses.Any() ) {
            return new List<string>();
        }

        // check and remove invalid
        addresses = addresses.Where(x => {
            if (!PublicKey.IsAddressValid(x)) {
                Console.WriteLine($"Address {new string(x.Take(10).ToArray(),0,10)}... is invalid, ignoring");
                return false;
            }
            return !string.IsNullOrWhiteSpace(x);
        });
        return addresses.ToList();
    }

    public ReturnCode Invoke(IEnumerable<Flag> flags) {
        string path = RequiredFlags.Where(x => x.Full == "file").First().Value ?? "";
        Flag user = RequiredFlags.Where(x => x.Full == "user").First();
        bool queryAll = OptionalFlags.Any(x => x.Full == "all");
        bool dumpRequested = OptionalFlags.Any(x => x.Full == "dump");

        path = Utils.SanitizePath(path, "unichain", ".chain");

        if(!File.Exists(path)) {
            return ReturnCode.BlockChainNotFound;
        }

        var bc = Utils.ParseBlockchain(path);
        if(bc is null) {
            return ReturnCode.InvalidBlockchain;
        }

        string dumpPath = Path.Combine(Environment.CurrentDirectory, $"dump-{DateTime.Now:dd-MM-yyyy-HH-mm-ss}.txt");
        TextWriter sw;
        if (dumpRequested)
            sw = File.AppendText(dumpPath);
        else
            sw = Console.Out;

        var addrs = ParseAddress(OptionalFlags.Where(x => x.Full == "address").First().Value ?? "");
        if(OptionalFlags.Any(x => x.Full == "balance")) {
            List<(string, double)> output = new();
            foreach (var addr in addrs) {
                var bal = bc.GetBalance(addr);
                output.Add(new(addr, bal));
            }
            var json = JsonConvert.SerializeObject(output);
            sw.Write(json);
            if(dumpRequested) 
                sw.Close();
            return ReturnCode.Success;
        }

        if(OptionalFlags.Any(x => x.Full == "all")) {

        }

        return ReturnCode.Success;
    }

    public void Help() {
        Console.WriteLine(@"
Required flags for 'get' subcommand:
  -f  --file => The path for the blockchain file

Optional flags for 'get' subcommand:
  -u    --user    => The address of the subject to be searched for. 
                     Can be many addresses separeted by commas or be empty to search all addresses.
  -d    --dump    => Dumps all information on a json file.
  -a    --all     => Gets all types of transactions matching the query.
  -bal  --balance => Gets the current balance of the address/addresses. Not compatible with other search tags.
");

    }
}
