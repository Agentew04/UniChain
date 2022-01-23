namespace Unichain.CLI.Commands
{
    internal class Create
    {
        internal static void Exec(string path)
        {
            if (path == "") // it doesn't work using ispathinput
            {
                //path not found
                Console.WriteLine("No parameter for the file location found, do you want to create one" +
                    " in the current directory?[y/N]");
                var input = Console.ReadLine();
                if (input?.ToUpper() != "Y")
                {
                    Utils.Print("Exitting...");
                    Environment.Exit(0);
                }
                path = Environment.CurrentDirectory + "\\unichain.chain";
                Utils.CreateChain(path);
                Utils.Print($"Blockchain created in {path}");
                Environment.Exit(0);
            }
            //found path
            Utils.CreateChain(path);
            Utils.Print($"Blockchain created in {path}");
            Environment.Exit(0);
        }
    }
}
