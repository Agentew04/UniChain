using Unichain.CLI.Commands;
using Unichain.Core;
using Unichain.Events;

namespace Unichain.CLI;

public class Program
{
    public static Dictionary<string, bool> Commands { get; set; } = new()
    {
        // defines commands and if they need file argument or not
        { "create", false },
        { "connect", true },
        { "add", true },
        { "mine", true },
        { "generate", false },
        { "get", true },
        { "print", true },
    };
    public static int ExitCode { get; set; } = 0;
    public static bool IsCommand { get; set; }

    static CommandProvider Provider { get; set; } = new();

    public static int Main(string[] args) {
        Provider.RegisterCommand<AddCommand>()
            .RegisterCommand<CreateCommand>()
            .RegisterCommand<GenerateCommand>()
            .RegisterCommand<GetCommand>()
            .RegisterCommand<PrintCommand>()
            .RegisterCommand<MineCommand>();

        int exitCode = (int)Provider.Invoke(args);

        return exitCode;
    }

//    public static void ShowAddHelp(ITransaction type)
//    {
//        switch (typeof(Type))
//        {
//            case type is CurrencyTransaction:
//                Utils.Print(@"
//Flags for Transaction event:
//      --amount   => It's the amount of money that will be sent
//  -r  --receiver => The address that will receive the money");
//                break;
//            case nameof(EventType.NFTTransfer):
//                Utils.Print(@"
//Flags for NFT Transfer event:
//  -i  --id       => The Id(Guid) of the NFT that will be transferred
//      --receiver => The address that will receive the NFT");
//                break;
//            case nameof(EventType.NFTBurn):
//                Utils.Print(@"
//Flags for NFT Burn event:
//  -i  --id => The Id(Guid) of the NFT that will be burned");
//                break;
//            case nameof(EventType.NFTMint):
//                Utils.Print(@"
//Flags for NFT Mint event:
//  -m  --meta => The path to the .json file containing the metadata");
//                break;
//            case nameof(EventType.PoolOpen):
//                Utils.Print(@"
//Flags for Pool Open event:
//  -m  --meta => The path to the .json file containing the metadata");
//                break;
//            case nameof(EventType.PoolVote):
//                Utils.Print(@"
//Flags for the Pool Vote event:
//  -i  --id   => The Id(Guid) of the Pool that will receive the vote
//  -v  --vote => The index of the vote cast");
//                break;
//            case nameof(EventType.DocumentSubmit):
//                break;
//            case nameof(EventType.MessageSendUser):
//                break;
//            default:
//                break;
//        }
//    }

//    public static void ShowHelp(string subcommand = "", EventType? type = null)
//    {
//        if (subcommand == "add" && type != null)
//        {
//            ShowAddHelp(type);
//            return;
//        }
//        Utils.Print("Welcome to the UniChain CLI Helper!");
//        switch (subcommand)
//        {
//            case "add":
//                Utils.Print();
//                if (type != null) ShowAddHelp(type);
//                break;
//            case "print":
//                Utils.Print(@"
//Possible flags for 'Utils.Print' sub-command:
//  -f  --file    => Path to the json file that the blockchain is stored
//  -d  --dump    => Dumps the output to a file
//  -x  --hex     => Utils.Prints out the chain in hexadecimal(JSON text bytes)
//  -b  --base64  => Utils.Prints out the chain in base 64(JSON text bytes)");
//                break;
//            case "mine":
//                Utils.Print();
//                break;
//            case "generate":
//                Utils.Print();
//                break;
//            case "get":
//                Utils.Print(@"
//Possible flags and options for 'get' sub-command:
//  -f  --file    => Path to the .chain file that the blockchain is stored
//  -a  --address => The address to be searched, works with '--nft' and '--balance'
//      --balance => Gets the token balance of a address
//      --nft     => Gets all NFTs a address has");
//                break;
//            case "cache":
//                Utils.Print(@"
//Possible flags for 'cache' sub-command:
//  -m  --meta    => Calculate and cache just NFTs and Pools metadata 
//  -b  --balance => Calculate and cache just the balance of all addresses");
//                break;
//        }

}

