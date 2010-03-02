using System;

namespace PhysXTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PhysXTest game = new PhysXTest())
            {
                game.Run();
            }
        }
    }
}

