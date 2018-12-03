using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WechatConnector
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            PrintSplash();
#endif
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void PrintSplash()
        {
            try
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("".PadRight(Console.WindowWidth - 1));
                Console.WriteLine(" WeChat Connector".PadRight(Console.WindowWidth - 1));
                Console.WriteLine("".PadRight(Console.WindowWidth - 1));
                Console.WriteLine();
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
