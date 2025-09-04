using System.Diagnostics;
namespace Applet.Nat
{
    public class Program
    {
        public static void Main(string[] vcoArgs)
        {
            CreateHostBuilder(vcoArgs).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] vcoArgs) =>
            Host.CreateDefaultBuilder(vcoArgs)
            .ConfigureWebHostDefaults(ioWebBuilder =>
                {
                    ioWebBuilder.UseStartup<Startup>();
                });
    }
}
