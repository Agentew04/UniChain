using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI.Commands;

internal class CreateCommand : ICommand {
    public string Name { get; set; } = "create";

    public List<Flag> RequiredFlags { get; set; } = new();

    public List<Flag> OptionalFlags { get; set; } = new() {
        { new Flag("file", "f", true, true) },
        { new Flag("yes", "y") },
        { new Flag("reward", "r", true) },
        { new Flag("difficulty", "d", true) }
    };
   
    
    public ReturnCode Invoke(IEnumerable<Flag> flags) {
        string filePath = flags.Where(x => x.Full == "file").FirstOrDefault()?.Value ?? "";
        bool hasYesFlag = flags.Any(x => x.Full == "yes");
        string rewardStr = flags.Where(x => x.Full == "reward").FirstOrDefault()?.Value ?? "10";
        string difficultyStr = flags.Where(x => x.Full == "difficulty").FirstOrDefault()?.Value ?? "2";

        // check file path
        if (filePath == "") {
            bool createDefault = false;
            if (hasYesFlag) createDefault = true;
            else {
                Console.WriteLine("Flag -f not found, do you want to create a file in the current directory? (y/n)");
                string answer = Console.ReadLine() ?? " ";
                if (answer.ToLower()[0] == 'y')
                    createDefault = true;
                else
                    return ReturnCode.Success;
            }
            if (createDefault) {
                filePath = Path.Combine(Environment.CurrentDirectory, "\\unichain.chain");
            }
        }

        filePath = Utils.SanitizePath(filePath, "unichain", ".chain");

        if (File.Exists(filePath)) {
            Console.WriteLine("File already exists!");
            return ReturnCode.InvalidArgumentValue;
        }


        // here path is a file and doesn't exist, has .chain ext

        double reward = double.Parse(rewardStr);
        if (reward < 0) {
            Console.WriteLine("Reward must be a positive number!");
            return ReturnCode.InvalidArgumentValue;
        }
        int difficulty = int.Parse(rewardStr);
        if (difficulty < 1) {
            Console.WriteLine("Difficulty must be a positive integer!");
            return ReturnCode.InvalidArgumentValue;
        }
        var chain = new Blockchain(difficulty, reward);
        using BlockchainParser parser = new();
        using var ms = parser.SerializeBlockchain(chain);
        using var fs = File.OpenWrite(filePath);
        ms.CopyTo(fs); // await?
        return ReturnCode.Success;
    }

    public void Help() {
        Console.WriteLine(@"
Possible flags for 'create' sub-command:
  -f  --file => Path to the .chain file that will be created
  -y  --yes  => Answer yes to all questions");
    }
}
