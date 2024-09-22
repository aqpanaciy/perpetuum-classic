using Mono.Unix;
using Microsoft.Extensions.Logging;
using McMaster.Extensions.CommandLineUtils;
using Perpetuum.Log;
using Perpetuum.Bootstrapper;
using System;
using System.Threading.Tasks;
using System.Threading;


namespace Perpetuum.Server
{
    public class Program
    {
        private static readonly ILogger _logger = Logger.Factory.CreateLogger("Program");

        [Argument(0, Description = "Path to game root", Name = "GameRoot")]
        [FileOrDirectoryExists]
        public static string GameRoot { get; } = "/var/opt/perpetuum/data";

        private static PerpetuumBootstrapper bootstrapper;

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void OnExecute()
        {
            try
            {
                bootstrapper = new PerpetuumBootstrapper();

                bootstrapper.Init(GameRoot);
                bootstrapper.Start();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
                return;
            }

            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 6) || (p == 128))
            {
                UnixSignal[] signals = new UnixSignal[] {
                    new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
                };

                Task.Run(() =>
                {
                    var index = UnixSignal.WaitAny(signals);
                    bootstrapper.Stop();
                });
            }
            else
            {
                System.Console.CancelKeyPress += new ConsoleCancelEventHandler((object sender, ConsoleCancelEventArgs eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    bootstrapper.Stop();
                });
            }

            bootstrapper.WaitForStop();
            Logger.Factory.Dispose();
        }
    }
}
