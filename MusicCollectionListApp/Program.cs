﻿
using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using MusicCollectionContext;
using MusicCollectionSystemIO;
using MusicCollectionMsDos;
using MusicCollectionPowerShell;
using MusicCollectionActions;
using MusicCollectionValidators;
using MusicCollectionLinux;

//via dotnet command
//dotnet add package Microsoft.PowerShell.SDK
//dotnet add package Serilog.Sinks.File
//dotnet add package Serilog.Sinks.Console

//via powershell
//Install-Package Microsoft.PowerShell.SDK
//Install-Package Serilog.Sinks.Console
//Install-Package Serilog.Sinks.File 

namespace MusicCollectionListApp
{
    internal class Program
    {
        private static Stopwatch _watch;

        public static int Main(string[] args)
        {
            StartLogger();

            Log.Information("App Started...");

            Startwatch();

            CollectionOriginType collectionOriginType = CollectionOriginType.Lossless;

            //==================================================================
            //Action 1 - Extract tree folder/files and save result in text file
            //==================================================================


            //-------------------------------------------------------------
            //Option 1 - Extractor Files and Folder via (CMD / DOS command)
            //
            //TOP 1 - BEST HIGH PERFORMANCE
            //-------------------------------------------------------------
            MsDosShellHelper msDosShellHelper = new();
            //msDosShellHelper.TreeProcess(collectionOriginType, SystemElementsFilter.FilesOnly, true, true);
            //msDosShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.DirectoriesOnly, true, true);
            msDosShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.All, true, true);


            //-----------------------------------------------------
            //Option 2- Extractor Files and Folder via (PowerShell)
            //
            //TOP 2 - MIDLE PERFORMANCE
            //-----------------------------------------------------
            var powerShellHelper = new PowerShellHelper();

            //V1 - using powershell pipeline
            ////powerShellHelper.TreeProcessUsingPipeline(collectionOriginType, FileSystemContextFilter.DirectoriesOnly);

            //V2 - using powershell string command
            ////powerShellHelper.TreeProcessUsingCommand(collectionOriginType, FileSystemContextFilter.DirectoriesOnly);

            //V3 -using powershell execute script
            //powerShellHelper.TreeProcessUsingScriptString(collectionOriginType, FileSystemContextFilter.DirectoriesOnly);
            //powerShellHelper.TreeProcessUsingScriptString(collectionOriginType, FileSystemContextFilter.All, false);


            //--------------------------------------------------------------------------------------------
            //Option 3- Extractor Files and Folder via (C#) , Directory.GetDirectories, Directory.GetFiles
            //
            //TOP 3 - LOW PERFORMANCE
            //--------------------------------------------------------------------------------------------

            // via C# extract treefolder/files and save result 3 in text file (Artists, Albums and tracks
            var systemIOShellHelper = new SystemIOShellHelper();
            //systemIOShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.All, true);
            //systemIOShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.DirectoriesOnly);


            //-------------------------------------------------------------------
            //Option 4 - Extractor Files and Folder via (LINUX bash / ls command)
            //
            //TOP 1 - BEST HIGH PERFORMANCE
            //--------------------------------------------------------------------
            LinuxShellHelper linuxShellHelper = new();
            //linuxShellHelper.TreeProcess(collectionOriginType, SystemElementsFilter.FilesOnly, true, true);
            //linuxShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.DirectoriesOnly, true, true);
            //linuxShellHelper.TreeProcess(collectionOriginType, FileSystemContextFilter.All, true, true);



            //===================================================================
            //Action 2 - Transform text file from previous step to csv file
            //and add  prefix 'absolute fullFolder' to line and column extension
            //columns separated by 'fieldSeparator' char
            //output format: absolute fullFileName ; extencion
            //output can be upload to Access and make queries
            //===================================================================

            //---------------------------------------------------------------------
            var filesTransformer = new FilesTransformer();
            //filesTransformer.FlatToCSV(collectionOriginType);
            //---------------------------------------------------------------------


            //===================================================================
            //Action 3 - Validate  text file from previous step to csv file
            //and add  prefix 'absolute fullFolder' to line and column extension
            //columns separated by 'fieldSeparator' char
            //output format: absolute fullFileName ; extencion
            //output can be upload to Access and make queries
            //===================================================================

            //---------------------------------------------------------------------
            var validateCollection = new ValidateCollectionAction();
            //validateCollection.ValidateSequencialFileWithTreeCollection(collectionOriginType);
            //---------------------------------------------------------------------

            //////////////////////////////////////////

            Stopwatch();

            Debug.WriteLine($"Elapsed: {_watch.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed: {_watch.ElapsedMilliseconds}");
            Log.Warning($"Elapsed: {_watch.ElapsedMilliseconds}");

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
            Log.Information($"LogFile:{logFileFullName}");
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