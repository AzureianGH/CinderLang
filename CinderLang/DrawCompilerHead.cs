using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixPix;

namespace CinderLang
{
    public static class DrawCompilerHead
    {
        public const string RVersion = "0.0.0";
        public const string Codename = "Ci0 - Fresh dust";

#if DEBUG
        public const string Version = RVersion + "-dev";
#else
        public const string Version = RVersion;
#endif

        public static void DrawHead()
        {
            Console.WriteLine();

            void PrintOrange(string s)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(s);
                Console.ResetColor();
            }

            if (Sixel.IsSupported())
            {
                var enc = Sixel.CreateEncoder(Image.Load<Rgba32>("assets/termlogo.png")).Encode();

                Console.WriteLine(enc);

                var np = Console.GetCursorPosition();
                var Y = np.Top;

                Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.SetCursorPosition(18, Y - 3);
                PrintOrange(Codename);

                Console.SetCursorPosition(18, Y - 2);
                PrintOrange("V" + Version);

                Console.SetCursorPosition(np.Left, np.Top);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("    ██████████");
                Console.WriteLine("  ██████");
                Console.WriteLine("████");
                Console.WriteLine("███");
                Console.WriteLine("███");
                Console.WriteLine("███");
                Console.WriteLine("████");
                Console.WriteLine("  ██████");
                Console.Write    ("    ██████████");

                var x = Console.GetCursorPosition().Left;
                var y = Console.GetCursorPosition().Top;

                void Print(string s)
                {
                    Console.CursorLeft = x;
                    Console.CursorTop--;
                    Console.Write(s);
                }


                Console.ResetColor();

                Console.Write(" ███ ██   ██  ███████  █████  ██");
                Print(" ███ ██   ██ ███  ███ ██      ██");
                Print(" ███ ██   ██ ███  ███ ███████ ██    "); PrintOrange("V"+Version);
                Print(" ███ ██████   ███████  █████  ████  "); PrintOrange(Codename);
                Print("                   ██");
                Print(" ███               ██");

                Console.SetCursorPosition(x, y);

                Console.WriteLine();
                Console.WriteLine();
            }
            
        }
    }
}
