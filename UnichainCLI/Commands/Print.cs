using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.CLI.Commands
{
    internal class Print
    {
        internal static void Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            bool isbinary = args.Contains("--binary");
            bool isoctal = args.Contains("--octal");
            bool ishex = args.Contains("-x") || args.Contains("--hex");
            bool isb64 = args.Contains("-b") || args.Contains("--base64");
            bool isdump = args.Contains("-d") || args.Contains("--dump");
            string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.txt";
            string rawjson = JsonConvert.SerializeObject(bc, Formatting.Indented);

            if (isbinary)
            {
                throw new NotImplementedException();
            }
            else if (isoctal)
            {
                throw new NotImplementedException();
            }
            else if (ishex)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rawjson);
                rawjson = Convert.ToHexString(bytes);
            }
            else if (isb64)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(rawjson);
                rawjson = Convert.ToBase64String(bytes);
            }
            if (isdump)
            {
                try
                {
                    File.WriteAllText(dumppath, rawjson);
                }
                catch (Exception ex)
                {
                    Utils.Print(ex.Message);
                    Environment.Exit(2);
                }
            }
            else
            {
                Utils.Print(rawjson);
            }
            Environment.Exit(0);
        }
    }
}
