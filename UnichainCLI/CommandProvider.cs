using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.CLI; 

internal class CommandProvider {

    private readonly List<ICommand> commands = new();

    public CommandProvider RegisterCommand<T>() where T : ICommand, new() {
        var command = new T();
        if (!commands.Contains(command))
            commands.Add(command);
        return this;
    }

    public CommandProvider UnregisterCommand<T>() where T : ICommand {
        commands.RemoveAll(x => x.GetType() is T);
        return this;
    }

    public ReturnCode Invoke(string[] args) {
        if (args is null || args.Length == 0) {
            Console.WriteLine("No command detected! Use -h to see the help page.");
            return ReturnCode.MissingCommand;
        }
        for(int i = 0; i < args.Length; i++) {
            if (args[i].StartsWith('-'))
                args[i] = args[i][1..];
            if (args[i].StartsWith('-'))
                args[i] = args[i][1..];
        }

        var command = commands.Where(x => x.Name == args[0])
            .FirstOrDefault();
        var helpRequested = Flag.TryGetFlagValue(args, new("help", "h"), out _);
        if (command is null) {
            if (helpRequested) {
                Console.WriteLine($"Unichain Help. CLI Version {Utils.GetVersion()}");
                Console.WriteLine();
                Console.WriteLine(@"
Possible arguments and sub-commands:
      add      => Add a event to the pending transactions
      cache    => Calculates and caches and blockchain information
      create   => Creates a new blockchain in a file
      connect  => Connects the current pc to the network
      mine     => Mine all pending transactions
      generate => Generate a number of key pairs (Users and Addresses)
      get      => Search for something inside the blockchain
      mine     => Mines a block
      print    => prints a schema of the current state of the blockchain.
  -h  --help   => Display this help menu
  -f  --file   => Path to the json file that the blockchain is stored");
            } else Console.WriteLine("Invalid command! Use -h to see the help page.");

            return ReturnCode.InvalidCommand;
        }

        if(helpRequested) {
            Console.WriteLine($"Unichain Help. CLI Version {Utils.GetVersion()}");
            Console.WriteLine();
            command.Help();
            return ReturnCode.Success;
        }

        // get necessary flags, without them there is no need to run the command
        var required = command.RequiredFlags.Clone();
        foreach(var flag in required) {
            var has = Flag.TryGetFlagValue(args, flag, out var val);

            if (!has)
                return ReturnCode.MissingArgumentValue;
            flag.Value = val;
        }

        var optional = new List<Flag>();
        foreach (var flag in command.OptionalFlags) {
            Flag newFlag = flag.Clone();
            var has = Flag.HasFlag(args, flag);
            if (!has)
                continue;
            if (flag.HasValue) {
                var valPresent = Flag.TryGetFlagValue(args, flag, out string val);
                if (!flag.CanBeEmpty && !valPresent)
                    return ReturnCode.MissingArgumentValue;
                newFlag.Value = val;
            }
            optional.Add(newFlag);
        }

        // join flags
        var flags = required.Concat(optional);

        ReturnCode exitCode = command.Invoke(flags);
        return exitCode;
    }
}
