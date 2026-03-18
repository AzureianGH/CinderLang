using BackendInterface;
using CCBBackend;
using LLVMBackend;

namespace CinderLang
{
    internal class Program
    {
        public static IBuilder Builder { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: CinderLang <input.cin> [--backend=llvm|ccb]");
                Environment.Exit(1);
            }

            var backendName = args
                .Skip(1)
                .FirstOrDefault(a => a.StartsWith("--backend=", StringComparison.OrdinalIgnoreCase))
                ?.Split('=', 2)[1]
                ?.Trim()
                .ToLowerInvariant() ?? "llvm";

            Builder = backendName switch
            {
                "ccb" => new CCBBackend.Builder(),
                "llvm" => new LLVMBackend.Builder(),
                _ => throw new ArgumentException($"Unknown backend '{backendName}'. Supported backends: llvm, ccb.")
            };

            var namespaces = Parser.Parse(File.ReadAllText(args[0]));

            var extension = backendName == "ccb" ? ".ccb" : ".asm";

            foreach (var item in namespaces)
            {
                item.Generate(null!);

                if (!item.Module.TryVerify(out var error))
                    ErrorManager.Throw(ErrorType.Generation, $"The namespace \"{item.Name}\" failed to generate in backend '{backendName}': {error}");

                Builder.EmitToFile(item.Name + extension, item.Module);

                var d = item.Module.PrintToString();
                Console.WriteLine(d);
            }
        }
    }
}
