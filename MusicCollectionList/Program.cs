
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reflection.Metadata;

namespace MusicCollection
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Staring...");

            var watch = new Stopwatch();
            watch.Start();

            //-------------------------------------------
            //1- Extractor Files and Folder (PowerShell)
            //-------------------------------------------

            // via PowerShell extract treefolder/files and save result in text file
            var powerShellHelper = new MusicCollection.PowerShellHelper();

            //using powershell pipeline
            ////powerShellHelper.PowerShellRunWithPipelne(CollectionOriginType.Loss);

            //using powershell string command
            ////powerShellHelper.PowerShellRunCommand(CollectionOriginType.Loss);

            //using powershell execute script
            //powerShellHelper.PowerShellRunScriptString(CollectionOriginType.Loss);
        

            watch.Stop();
            Debug.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");
            Console.WriteLine($"Elapsed: {watch.ElapsedMilliseconds}");

            //-------------------------------------------
            //2- Extractor Files and Folder (C#)
            //-------------------------------------------

            // via C# extract treefolder/files and save result 3 in text file (Artists, Albuns and tracks
            var extractor = new MusicCollection.FoldersTreeExtractor();
            //extractor.Process(CollectionOriginType.Loss);


            //------------------------------------------------
            //3- Transform text from previous step to csv file
            //------------------------------------------------

            //extract all folders and files
            var filesTransformer = new MusicCollection.FilesTransformer();
            filesTransformer.TextxToCSV(CollectionOriginType.Loss);


            System.Console.WriteLine("Finished...");

            return 0;
        }
    }
}

//Collection<PSObject> res;
//using PowerShell powershell = PowerShell.Create();
//powershell.AddScript("param($param1) $d = get-date; $s = 'test string value'; " + "$d; $s; $param1;");
//powershell.AddParameter("param1", "parameter 1 value!");
//res = powershell.Invoke();