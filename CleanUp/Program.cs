using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace CleanUp
{
    class Program
    {
        static void Main(string[] args)
        {

            StartCleanUpProcess().Wait();
            Console.WriteLine("Enter to continue");
            Console.ReadLine();
        }

        private static async Task StartCleanUpProcess()
        {
            var cleanUp = new CleanUp();
            cleanUp.Start();
        }

    }
}
