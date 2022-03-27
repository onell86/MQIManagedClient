using Serilog;
using System;

namespace CertificateInstallTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()                
                .CreateLogger();

            if (args.Length < 3)
            {
                Log.Error("Please execute  \"dotnet install-tool.exe [filename].p12 [password] [certLabel\"");                
                return;
            }
            CertInstaller.SetupCert(args[0], args[1], args[2]);
        }
    }
}
