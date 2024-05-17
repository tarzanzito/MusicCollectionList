
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
        private static Stopwatch? _watch;

        public static int Main(string[] args)
        {
            StartLogger();

            Log.Information("App Started...");


            //Choose input Collection
            CollectionOriginType collectionOriginType = CollectionOriginType.Loss;

            //Action 1 - Extract folder tree from OS
            ExtractFoldersTree(collectionOriginType);

            //Action 2 - Transform text file from previous step to csv file
            TransformFlatFileToStandardCsv(collectionOriginType);

            //Action 3 - Validate Path rules (input file must have only Folders)
            ValidateCollectionOriginType(collectionOriginType);


            Log.Information("Finished...");

            Log.Information("Enter ...");
            Console.ReadKey();

            return 0;
        }

        private static bool ExtractFoldersTree(CollectionOriginType collectionOriginType)
        {
            Startwatch("ExtractFoldersTree");

            FileSystemContextFilter fileSystemContextFilter = FileSystemContextFilter.DirectoriesOnly;

            bool result;

            //-------------------------------------------------------------
            //Option 1 - Extractor Files and Folder via (CMD / DOS command)
            //
            //TOP 1 - BEST HIGH PERFORMANCE
            //-------------------------------------------------------------
            var msDosShellHelper = new MsDosShellHelper();
            result = msDosShellHelper.TreeProcess(collectionOriginType, fileSystemContextFilter, true, true);

            //-----------------------------------------------------
            //Option 2 - Extractor Files and Folder via (PowerShell)
            //
            //TOP 2 - MIDDLE PERFORMANCE
            //-----------------------------------------------------
            //var powerShellHelper = new PowerShellHelper();

            //V1 - using powershell pipeline
            //powerShellHelper.TreeProcessUsingPipeline(collectionOriginType, fileSystemContextFilter, true, true);

            //V2 - using powershell string command
            //powerShellHelper.TreeProcessUsingCommand(collectionOriginType, fileSystemContextFilter, true, true);

            //V3 -using powershell execute script
            //powerShellHelper.TreeProcessUsingScriptString(collectionOriginType, fileSystemContextFilter, true, true);


            //-----------------------------------------------------------------------------------------------
            //Option 3 - Extractor Files and Folder via (C#) , Directory.GetDirectories, Directory.GetFiles
            //
            //TOP 3 - LOW PERFORMANCE
            //-----------------------------------------------------------------------------------------------

            //var systemIOShellHelper = new SystemIOShellHelper();
            //systemIOShellHelper.TreeProcess(collectionOriginType, fileSystemContextFilter, true);


            //---------------------------------------------------------------------
            //Option 4 - Extractor Files and Folder via (LINUX bash / ls command)
            //
            //TOP 1 - BEST HIGH PERFORMANCE
            //---------------------------------------------------------------------
            //var linuxShellHelper = new LinuxShellHelper();     FileSystemContextFilter
            //linuxShellHelper.TreeProcess(collectionOriginType, SystemElementsFilter.FilesOnly, true, true);


            Stopwatch("ExtractFoldersTree");

            return result;
        }

        private static void TransformFlatFileToStandardCsv(CollectionOriginType collectionOriginType)
        {
            Startwatch("TransformFlatFileToStandardCsv");

            //Transform text file from previous step to csv file
            //  and add  prefix 'absolute fullFolder' to line and column extension
            //  columns separated by 'fieldSeparator' char
            //  output format: absolute fullFileName ; extencion
            //  output can be upload to Access and make queries

            FilesTransformer filesTransformer = new();
            filesTransformer.FlatToCSV(collectionOriginType, false);

            Stopwatch("TransformFlatFileToStandardCsv");
        }

        private static void ValidateCollectionOriginType(CollectionOriginType collectionOriginType)
        {
            Startwatch("ValidateCollectionOriginType");

            //Note: input file must have only Folders
            // FileSystemContextFilter.DirectoriesOnly
            //Validate folders formats
            //  Artist : ArtistName {countryName}
            //  Album  : ArtistName {year} [AlbumName] @FLAC (or @MP3) 

            ValidateCollectionAction validateCollection = new();
            validateCollection.ValidateFoldersRulesFromLinearFormatedFile(collectionOriginType);

            Stopwatch("ValidateCollectionOriginType");
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

        private static void Startwatch(string msg)
        {
            Log.Information($"Stopwatch Started... - {msg}");

            _watch = new Stopwatch();
            _watch.Start();
        }

        private static void Stopwatch(string msg)
        {
            if (_watch == null)
                return;

            _watch.Stop();

            Log.Information($"Elapsed: {_watch.ElapsedMilliseconds}");
            
            Log.Information($"Stopwatch Stopped... {msg}");
        }
    }
}
