using Newtonsoft.Json;
using System.Text;

namespace Unichain.CLI.Commands
{
    internal class Print
    {
        internal static int Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            if (bc == null) return 4; // bad blockchain

            bool ishex = args.Contains("-x") || args.Contains("--hex");
            bool isb64 = args.Contains("-b") || args.Contains("--base64");
            bool isdump = args.Contains("-d") || args.Contains("--dump");
            string dumppath = $"{Environment.CurrentDirectory}\\dumpfile-{DateTime.Now.Ticks}.txt";
            string rawjson = JsonConvert.SerializeObject(bc, Formatting.Indented);

            if (ishex)
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
                catch (Exception)
                {
                    return 2;
                }
            }
            else
            {
                Utils.Print(rawjson);
            }
            return 0;
        }
    }
}
