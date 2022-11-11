using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.CLI; 

public enum ReturnCode {
    Success = 0,
    BlockChainNotFound,
    MissingArgumentValue,
    MissingCommand,
    InvalidKey,
    InvalidAddress,
    InvalidArgumentValue,
    InvalidCommand,
    InvalidBlockchain
        
}

