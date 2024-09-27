
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


            //CHOOSE here the input Collection
            CollectionOriginType collectionOriginType = CollectionOriginType.Lossless;

            //Action 1 - Extract ONLY folders tree from OS  (output '*.tmp' pure dir, '*.txt' LinearFormat)
            ExtractInfoFromOS(collectionOriginType, FileSystemContextFilter.DirectoriesOnly);

            //Action 2 - Transform text file from previous step to csv file (output '*.csv')
            TransformFlatFileToStandardCsv(collectionOriginType, false, true);

            //Action 3 - Validate Path rules (input file must have only Folders) (input '*.txt', output '*_ERROR.csv'.
            ValidateCollectionOriginType(collectionOriginType);

            //////////////

            //Action 4 - Extract ONLY files tree from OS  (output '*.tmp' pure dir, '*.txt' LinearFormat)
            RenameFiles(collectionOriginType, "D"); 
            ExtractInfoFromOS(collectionOriginType, FileSystemContextFilter.FilesOnly);
            TransformFlatFileToStandardCsv(collectionOriginType, false, true);

            Log.Information("Finished...");

            Log.Information("Enter ...");
            Console.ReadKey();

            return 0;
        }


        private static bool ExtractInfoFromOS(CollectionOriginType collectionOriginType, FileSystemContextFilter fileSystemContextFilter, bool applyExtensionsFilter = false)
        {
            Startwatch("ExtractInfoFromOS");

            //FileSystemContextFilter fileSystemContextFilter = FileSystemContextFilter.DirectoriesOnly;

            bool result;

            //-------------------------------------------------------------
            //Option 1 - Extractor Files and Folder via (CMD / DOS command)
            //
            //TOP 1 - BEST HIGH PERFORMANCE
            //-------------------------------------------------------------
            var msDosShellHelper = new MsDosShellHelper();
            result = msDosShellHelper.TreeProcess(collectionOriginType, fileSystemContextFilter, applyExtensionsFilter, true);

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


            Stopwatch("ExtractInfoFromOS");

            return result;
        }

        private static void TransformFlatFileToStandardCsv(CollectionOriginType collectionOriginType, bool onlyMusicFiles, bool addExtensionColumn)
        {
            Startwatch("TransformFlatFileToStandardCsv");

            //Transform text file from previous step to csv file
            //  and add  prefix 'absolute fullFolder' to line and column extension
            //  columns separated by 'fieldSeparator' char
            //  output format: absolute fullFileName ; extencion
            //  output can be upload to Access and make queries

            FilesTransformer filesTransformer = new();
            filesTransformer.FlatToCSV(collectionOriginType, onlyMusicFiles, addExtensionColumn);

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


        private static void RenameFiles(CollectionOriginType collectionOriginType, string sufixName)
        {
            string rootPath = "";
            string fullFileNameIn = "";
            string fullFileNameOut = "";
            string fullFileNameTempIn = "";
            string fullFileNameTempOut = "";
            string oldReplace = ".";
            string newReplace = $"_{sufixName}.";

            ////output files
            switch (collectionOriginType)
            {
                case CollectionOriginType.Lossless:
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    fullFileNameIn = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    fullFileNameTempIn = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                    break;
                case CollectionOriginType.Loss:
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    fullFileNameIn = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    fullFileNameTempIn = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                    break;
                default:
                    throw new Exception("CollectionOriginType error in 'MusicCollectionMsDos.TreeProcess')");
            }

            fullFileNameOut = fullFileNameIn.Replace(oldReplace, newReplace);
            fullFileNameTempOut = fullFileNameTempIn.Replace(oldReplace, newReplace);

            //renames
            if (File.Exists(fullFileNameIn))
                File.Move(fullFileNameIn, fullFileNameOut);

            if (File.Exists(fullFileNameTempIn))
                File.Move(fullFileNameTempIn, fullFileNameTempOut);

        }
    }
}
