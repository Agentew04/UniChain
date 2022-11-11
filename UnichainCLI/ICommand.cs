using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.CLI; 

internal interface ICommand {

    /// <summary>
    /// Invokes this command.
    /// </summary>
    /// <param name="flags">A collection of flags and parameters </param>
    /// <param name="parameters">A collection of orphan parameters</param>
    /// <returns>The return code</returns>
    ReturnCode Invoke(IEnumerable<Flag> flags);

    /// <summary>
    /// Prints this command help
    /// </summary>
    void Help();

    /// <summary>
    /// The command name, must be the first argument
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// A list of necessary flags, without these the command cannot run.
    /// </summary>
    List<Flag> RequiredFlags { get; set; }

    /// <summary>
    /// A list with the flags that can be passed to the command
    /// </summary>
    List<Flag> OptionalFlags { get; set; }

}
