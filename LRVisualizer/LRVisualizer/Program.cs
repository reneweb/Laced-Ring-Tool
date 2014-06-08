using System;

namespace LRVisualizer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException();
            }

            using (LRVisualizer lrVisualizer = new LRVisualizer(args[0]))
            {
                lrVisualizer.Run();
            }
        }
    }
#endif
}

