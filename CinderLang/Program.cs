using System.Buffers.Text;
using System.Reflection;
using System.Text;
using BackendInterface;
using LLVMBackend;
using LLVMSharp;
using LLVMSharp.Interop;

namespace CinderLang
{
    internal class Program
    {
        public static IBuilder Builder { get; set; }

        static void Main(string[] args)
        {
            if (!Directory.Exists("backends")) Directory.CreateDirectory("backends");

            foreach (var item in Directory.GetDirectories("backends"))
            {
                var bdll = Path.Combine(item,"backend.dll");

                if (File.Exists(bdll)) Assembly.LoadFrom(bdll);
            }

            CLIManager.ManageCommands(args);
        }
    }
}
