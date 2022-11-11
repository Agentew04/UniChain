# UniChain
![license](https://img.shields.io/github/license/Agentew04/UniChain)
![build](https://img.shields.io/github/workflow/status/Agentew04/UniChain/.NET)
![issues](https://img.shields.io/github/issues/Agentew04/UniChain)

This is a custom lightweight blockchain written in C# and tries to have
as many solutions as possible, including **NFT** , **Pools**, **Stakings**,
a **document ledger** and even **private messaging**! This blockchain
uses P2P and a central server (recommended [Firebase](https://firebase.google.com/docs/database))
to organize current and available IPs in the network.


## Installation

There is a NuGet Package [here]() to develop apps that use this
blockchain solution. There is also a CLI tool that make your computer
become a node and interacts with the main network. In the future
there will be a Desktop App for creating and managing your wallet.


## Usage

### As a library in your project

After the NuGet Package has been added to your project, you can
use it as following (C#)

```cs
using UniChain.Events;
using UniChain.Core;

int difficulty = 5;
float reward = 100;
Blockchain blockchain = new(difficulty, reward); 
User currentUser = new(); // will generate a random pair of keys
string receiver = new User().Address;

double fee = 5.2f;
ITransaction tx = new CurrencyTransaction(currentUser, fee, receiver, 15); // from, fee, to and amount
tx.SignTransaction(); // signs the transaction

blockchain.AddEvent(tx); // adds a event to the PendingEvents list
blockchain.MinePendingTransactions(currentUser.Address); // currentUser will receive the reward

Console.WriteLine(blockchain.GetBalance(currentUser.Address));
// balance = reward - (fee + 15)
// prints out: 79.8
```

### Command Line Interface (CLI)

You can download the latest CLI executable version [here](https://github.com/Agentew04/Unichain/releases/latest).
After you installed it, you can add the folder containing the `.exe` to your system PATH (if you don't know how read [this](https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14)#to-add-a-path-to-the-path-environment-variable)).

Sample usage:
```
## Creating a new blockchain
unichain create -f ./unichain.json  ==> You can leave the '-f' flag to autogenerate the file!


unichain -h OR unichain --help OR unichain help  ==>  general help
unichain mine -h OR unichain --help  ==>  sub-command specific help

unichain print --base64  ==> Prints out the entire blockchain in Base-64 format
unichain print --dump  ==> Dumps all output text to a file
```

### Desktop App

Currently, the desktop is WIP.

## Support

Feel free to create an discussion [here](https://github.com/Agentew04/UniChain/discussions) or
DM me on Discord, my nick is Agentew04#4046.

## Contributing

Any help is welcome! You can either create an issue with your 
suggestion/bug and we will try to fix it in the next release 
or you can fork the repository, make your changes and create a 
descriptive pull request following the existing templates!

## License

This project is licensed under [Mozilla Public License 2.0](https://github.com/Agentew04/UniChain/blob/6c714c0e5e2ac241e2bffd99cd9453e0b99b275b/LICENSE).
