using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atomic.Arc
{
    public interface IArcConsole
    {
        void RunInteractiveConsole();
        void RunNonInteractive();
        void Exit();
    }

    public class ArcConsole : IArcConsole
    {
        private Task arcServerTask;
        private CancellationTokenSource cTokenSource;
        private readonly IArcServer arcServer;
        private readonly IArcLog arcLog;
        private readonly IAlarmLog alarmLog;


        public ArcConsole(IArcServer arcServer, IArcLog arcLog)
        {
            this.arcServer = arcServer;
            this.arcLog = arcLog;
        }

        public void RunInteractiveConsole()
        {
            StartArcServer();

            while (!cTokenSource.IsCancellationRequested)
            {
                var option = MainMenu();
                HandleMainMenu(option);
            }
        }

        public void RunNonInteractive()
        {
            StartArcServer();
            while (!cTokenSource.IsCancellationRequested)
            {
                LoopLog(true);
                Task.Delay(1000).Wait();
            }
        }

        private void StartArcServer()
        {
            cTokenSource = new CancellationTokenSource();
            arcServerTask = Task.Run(() => arcServer.RunServer(cTokenSource.Token));
        }

        private void HandleMainMenu(int option)
        {
            switch (option)
            {
                case 0:
                    Exit();
                    break;
                case 1:
                    Status();
                    break;
                case 2:
                    ShowLog();
                    break;
                default:
                    Console.WriteLine("Invalid option specified");
                    break;
            }
        }

        public void Exit()
        {
            Console.Clear();
            Console.WriteLine("Exiting, please wait");
            cTokenSource.Cancel();
            Console.WriteLine("Waiting for ARC Server task to complete");
            try
            {
                arcServerTask.Wait();
            }
            catch { }

            Console.WriteLine("ARC Server has finished, exiting");
        }

        private int MainMenu()
        {
            while (!cTokenSource.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine(
                    "Main Menu:\n" +
                    "\t0 - Exit\n" +
                    "\t1 - Status\n" +
                    "\t2 - Log\n" +
                    "\nPlease enter an option from above: "
                    );
                var entry = Console.ReadLine();

                if (int.TryParse(entry, out var result)) return result;

                Console.WriteLine("Invalid option specified");
            }

            return 0;
        }

        private void Status()
        {
            Console.Clear();
            if (!arcServerTask.IsCompleted)
            {
                var count = arcServer.LineHandlers.Count();
                Console.WriteLine(
                    "Current Status:\n" +
                    "\tARC Server is RUNNING\n" +
                    "\tThere are " + count + " Line handlers running\n" +
                    "\tHandled " + alarmLog.GetAllAlarms.Count() + " alarms\n"
                    );

                if (count > 0)
                {
                    Console.WriteLine("\tLine Handlers:");
                    foreach (var lh in arcServer.LineHandlers)
                    {
                        Console.WriteLine($"\t\t{lh.ConnectedEndpoint}");
                    }
                }
            }
            else
            {
                Console.WriteLine(
                    "Current Status:\n" +
                    "\tARC Server is NOT RUNNING\n"
                    );
            }
            PressAnyKey();
        }

        private void ShowLog()
        {
            Console.Clear();
            Console.WriteLine($"There are {arcLog.Size} new log entries");
            PressAnyKey();
            LoopLog();            
            PressAnyKey();
        }

        private void LoopLog(bool nonInteractive = false)
        {
            var i = 0;
            while (arcLog.Size > 0)
            {
                var str = nonInteractive ? arcLog.Next : $"{i} - {arcLog.Next}";
                Console.WriteLine(str);
                i++;
            }
        }

        private static void PressAnyKey()
        {
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
