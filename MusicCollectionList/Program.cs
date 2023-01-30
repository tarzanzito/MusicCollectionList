
using MusicCollectionList;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

//dotnet add package Microsoft.PowerShell.SDK
//dotnet add package Serilog.Sinks.File
//dotnet add package Serilog.Sinks.Console

//powershell
//Install-Package Microsoft.PowerShell.SDK
//Install-Package Serilog.Sinks.Console
//Install-Package Serilog.Sinks.File 

namespace MusicCollection
{
    internal class Program
    {
        private static Stopwatch _watch;

        public static int Main(string[] args)
        {
            StartLogger();

            Log.Information("App Staring...");

            Startwatch();
            var watch = new Stopwatch();
            watch.Start();

            //--------------------------------------------------
            //1 - Extractor Files and Folder (CMD / DOS command)
            //--------------------------------------------------
            MsDosShellHelper msDosShellHelper = new();
            //msDosShellHelper.Process(CollectionOriginType.Loss); //TOP 1 - BEST PERFORMANCE


            //-------------------------------------------
            //2- Extractor Files and Folder (PowerShell)
            //-------------------------------------------

            // via PowerShell extract treefolder/files and save result in text file
            var powerShellHelper = new MusicCollection.PowerShellHelper();

            //V1 - using powershell pipeline
            ////powerShellHelper.PowerShellRunWithPipelne(CollectionOriginType.Loss);

            //V2 - using powershell string command
            ////powerShellHelper.PowerShellRunCommand(CollectionOriginType.Loss);

            //V3 -using powershell execute script
            //powerShellHelper.PowerShellRunScriptString(CollectionOriginType.Loss); //TOP 2 - BEST PERFORMANCE


            //----------------------------------
            //3- Extractor Files and Folder (C#)
            //----------------------------------

            // via C# extract treefolder/files and save result 3 in text file (Artists, Albuns and tracks
            var extractor = new MusicCollection.FoldersTreeExtractor();
            //extractor.Process(CollectionOriginType.Loss);  //LOW PERFORMANCE


            //------------------------------------------------
            //4- Transform text from previous step to csv file
            //format -> file ; extencion
            //output can be upload to Access and make queries
            //------------------------------------------------

            //extract all folders and files

            var filesTransformer = new MusicCollection.FilesTransformer();
            filesTransformer.TextToCSV(CollectionOriginType.Loss, false);

            watch.Stop();
            Debug.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");

            Log.Information("Finished...");

            return 0;
        }

        private static void StartLogger()
        {
            //string frameWorkVersion = AppContext.TargetFrameworkName;
            //string appFolder = AppContext.BaseDirectory;

            string appName = AppDomain.CurrentDomain.FriendlyName;
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;
            //string appFolder = Directory.GetCurrentDirectory();

            string logFileFullName = Path.Combine(appFolder, appName);

            Log.Logger = new LoggerConfiguration()
               .WriteTo.File(logFileFullName, rollingInterval: RollingInterval.Minute)
               .WriteTo.Console()
               .CreateLogger();

            Log.Information("Logger Started...");
        }

        private static void Startwatch()
        {
            Log.Information("Stopwatch Started...");

            _watch = new Stopwatch();
            _watch.Start();
        }

        private static void Stopwatch()
        {
            _watch.Stop();

            Log.Information($"Elapsed: {_watch.ElapsedMilliseconds}");
        }
    }
}
