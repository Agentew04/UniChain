using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Generate
    {
        internal static void Exec(string[] args)
        {
            var foundnum = Utils.TryGetArgument(args, new()
            {
                Name = "number",
                Simplified = "n"
            }, out double number);
            var isdump = args.Contains("-d") || args.Contains("--dump");
            string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.yml";
            using (var stream = File.AppendText(dumppath))
            {
                if (!foundnum || number == 0)
                {
                    number = 1;
                }
                foreach (var chunk in Enumerable.Range(1, (int)number).Chunk(50))
                {
                    List<(User user, int index)> users = new();
                    for (int i = 0; i < number; i++)
                    {
                        User user = new();
                        if (!isdump) Utils.Print($"User nº {i+1} - \n    PrivateKey: {user.GetPrivateKey().Key}\n    PublicKey: {user.Address}");
                        users.Add((user, i));
                    }
                    if (isdump)
                    {

                        foreach (var userchunk in users.Chunk(50))
                        {
                            string textchunk = "";
                            foreach (var user in userchunk)
                            {
                                textchunk += $"{user.index}:\n privkey: {user.user.GetPrivateKey().Key}\n publkey: {user.user.Address}\n";
                            }
                            stream.Write(textchunk);
                        }
                    }
                }
            }
            Environment.Exit(0);
        }
    }
}
