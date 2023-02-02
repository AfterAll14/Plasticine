using System;
using real = System.Double;

namespace Plasticine
{
    static class Utils
    {
        const int expClamp = 500;

        public static real ExpClamped(real value)
        {
            real valueClamped = Math.Clamp(value, -expClamp, expClamp);
            return (real)Math.Exp((double)valueClamped);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void Check(real value)
        {
            if (real.IsNaN(value) || real.IsInfinity(value))
                Console.WriteLine("caught!");
        }

        public static void Check(real[] values)
        {
            foreach (real value in values)
                Check(value);
        }
    }
}
