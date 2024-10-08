
using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;


namespace MusicCollectionMsDos
{
    public class MsDosShellHelper
    {
        private StreamWriter? _streamWriter;
        private string _extensionFilter = string.Empty;
        private bool _applyExtensionsFilter;


        /// <summary>
        /// generate text file with system tree with files and folders: typical output like command "DIR /S /B"
        /// /B = linear format - AbsoluteFullPathName\FileName
        /// /B lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3                              "folder"
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3   "file"
        /// 
        /// Note: /B output do not show difference between file and folder
        ///       so /B is no longer used in these methods  
        ///       
        /// Like linux 'ls' This method add '\' at end of any folder.
        /// 
        /// </summary>
        public bool TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            Log.Information("'TreeProcess' - Started...");
            
            bool resultOk = false;
            string outputFileName = string.Empty;
            string tempFileName = string.Empty;

            try
            {
                //string extensionFilter = string.Empty;
                _applyExtensionsFilter = applyExtensionsFilter;

                string rootPath;

                //output files
                switch (collectionOriginType)
                {
                    case CollectionOriginType.Lossless:
                        rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                        outputFileName = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                        tempFileName = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                        if (applyExtensionsFilter)
                            _extensionFilter = Constants.FileExtensionsFilterLossLess;
                        break;
                    case CollectionOriginType.Loss:
                        rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                        outputFileName = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                        tempFileName = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                        if (applyExtensionsFilter)
                            _extensionFilter = Constants.FileExtensionsFilterLoss;

                        break;
                    default:
                        throw new Exception("CollectionOriginType error in 'MusicCollectionMsDos.TreeProcess')");
                }

                _extensionFilter = _extensionFilter.Replace("*", "").Replace(" ", "").ToUpper().Trim();

                //if (applyExtensionsFilter)
                //    _applyExtensionsFilter = _extensionFilter.Length > 0;

                if (!Directory.Exists(rootPath))
                {
                    Log.Error($"Folder Root not exists=[{rootPath}");
                    return false;
                }

                if (!setToLinearOutputFormat)
                    tempFileName = outputFileName;

                //make MS-DOS command

                //NOTE: because the /B option does not show the difference between folder and file, /B is no longer used)
                //(but can see on DesableOption_B();
                
                string msDosCommand = "";

                switch (contextFilter)
                {
                    //chcp 65001>nul  do not send output message. note: is 'nul' not 'null'
                    case FileSystemContextFilter.All:
                        msDosCommand = $"chcp 65001>nul & dir /S {rootPath}";
                        break;
                    case FileSystemContextFilter.DirectoriesOnly:
                        msDosCommand = $"chcp 65001>nul & dir /S /A:D {rootPath}";
                        break;
                    case FileSystemContextFilter.FilesOnly:
                        msDosCommand = $"chcp 65001>nul & dir /S /A:-D {rootPath}";
                        break;
                }

                Log.Information($"Output File:[{outputFileName}]");
                Log.Information($"Context Filter:[{contextFilter}]");
                Log.Information($"Apply Extensions Filter:[{applyExtensionsFilter}]");
                Log.Information($"Extensions Filter:[{_extensionFilter}]");
                Log.Information($"MS-DOS Command:[{msDosCommand}]");

                //process 
                resultOk = MsDosProcess(msDosCommand, tempFileName);

                if (resultOk && setToLinearOutputFormat)
                    ChangeToCsvLinearFormat(tempFileName, outputFileName);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR EXCEPTION: {ex.Message}");
                resultOk = false;
            }

            Log.Information("'TreeProcess' - Finished...");

            return resultOk;
        }

        private bool MsDosProcess(string msDosCommand, string outputFileName)
        {
            Log.Information("'MsDosProcess' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");
            
            bool retValue = true;

            try
            {
                //output
                _streamWriter = new StreamWriter(outputFileName, false, Constants.StreamsEncoding);

                //Process Info
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C {msDosCommand}";

                //dos without window
                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                //startInfo.UseShellExecute = false;
                //startInfo.CreateNoWindow = false;

                //redirect output to files
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                //Process
                Process process = new();
                process.StartInfo = startInfo;

                //V1
                process.OutputDataReceived += OutputDataReceived;
                process.ErrorDataReceived += ErrorDataReceived;

                //V2 - with lambda (i dont like)
                //process.OutputDataReceived += (sender, args) =>
                //{
                //    _streamWriter.WriteLine(args.Data);
                //    _streamWriter.Flush();
                //};

                //process.OutputDataReceived += (sender, args) =>
                //{
                //    _streamWriter.WriteLine("ERROR:" + args.Data);
                //    _streamWriter.Flush();
                //};


                //Start
                process.Start();
                process.BeginOutputReadLine(); //important to file output 
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Log.Error($"Message Error:{ex.Message}");
                Log.Error($"Command:{msDosCommand}");
                Log.Error($"Outout:{outputFileName}");
                retValue = false;
            }
            finally
            {
                if (_streamWriter != null)
                {
                    //_streamWriter.Flush();
                    _streamWriter.Close();
                    _streamWriter?.Dispose();
                }
            }

            Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");

            Log.Information("'MusicCollectionMsDos.MsDosProcess' - Finished...");

            return retValue;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_streamWriter == null)
                return;

            _streamWriter.WriteLine("ERROR:" + e.Data);
            _streamWriter.Flush();
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_streamWriter == null)
                return;

            if (e.Data == null)
                return;

            bool isValid = true;
            bool isFolder = e.Data.Contains("<DIR>");

            //if (isFolder)
            //{
            //    if (contextFilter == FileSystemContextFilter.FilesOnly)
            //        continue;
            //}
            //else
            //{
            //    if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
            //        continue;
            //}

            ////Apply Extensions Filter
            if (!isFolder)
            {
                //verify Extensions Filter
                if (_applyExtensionsFilter)
                {
                    string extension = Path.GetExtension(e.Data).ToUpper().Trim();
                    isValid = _extensionFilter.Contains(extension);
                }
                else
                    isValid = true;
            }

            if (isValid)
            {
                _streamWriter.WriteLine(e.Data);
                _streamWriter.Flush();
            }
        }

        //Typical input:
        //  dir Path
        //  dir /S Path
        private void ChangeToCsvLinearFormat(string inputFileName, string outputFileName)
        {
            Log.Information("'ChangeToCsvLinearFormat' - Started...");

            Log.Information($"InputFile={inputFileName}");
            Log.Information($"OutputFile={outputFileName}");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

            StreamReader? streamReader = null;
            StreamWriter? streamWriter = null;

            string? line = null;
            int countFolders = 0;
            int countFiles = 0;

            try
            {
                if (!File.Exists(inputFileName))
                    throw new Exception($"InputFileName:'{inputFileName}' not found.");

                if (!CanCreateFile(outputFileName))
                    throw new Exception($"OutputFile:'{outputFileName}' cannot be created.");

                bool isFolder = false;
                //bool isValid = true;
                string baseDir = "";
                string item;

                //using (StreamReader reader = new StreamReader(fileName)) //C# 8
                //{
                //}
                //using var _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);

                streamReader = new StreamReader(inputFileName, Constants.StreamsEncoding);
                streamWriter = new StreamWriter(outputFileName, false, Constants.StreamsEncoding);

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length < 14) //less than phrase " Directory of "
                        continue;

                    if (line.Substring(0, 14) == " Directory of ") //new directory
                    {
                        baseDir = line.Substring(14);
                        continue;
                    }

                    if (line.Length < 37) //less than phrase "2022/09/21  22:53    <DIR>          "
                        continue;

                    if (!DateTime.TryParse(line.Substring(0, 10), out DateTime dt))
                        continue;

                    isFolder = (line.Substring(21, 5) == "<DIR>");

                    item = line.Substring(36);

                    if (isFolder && (item == ".") || (item == ".."))
                        continue;

                    //write
                    string newLine = $"{baseDir}{Path.DirectorySeparatorChar}{item}";

                    if (isFolder)
                        newLine += Path.DirectorySeparatorChar;

                    streamWriter.WriteLine(newLine);
                    streamWriter.Flush();

                    //counters
                    if (isFolder)
                        countFolders++;
                    else
                        countFiles++;
                }

                Log.Information($"Total Folders:{countFolders}");
                Log.Information($"Total Files  :{countFiles}");
                Log.Information($"Total        :{countFolders + countFiles}");
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.Message}");
                if (line != null)
                    Log.Error($"Line:{line}");
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
            }

            Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

            Log.Information("'ChangeToCsvLinearFormatt' - Finished...");
        }

        private bool CanCreateFile(string fileName)
        {
            bool canCreate = false;

            try
            {
                using (File.Create(fileName)) { };
                File.Delete(fileName);
                canCreate = true;
            }
            catch
            {
            }

            return canCreate;
        }

        #region comments

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        //private void DesableOption_B()
        //{
        //    //NOTE: because the /B option does not show of 'fullName' the difference between folder and file is no longer used)
        //    //(see commented code at the of this file)

        //    bool useLinearOutputFormat = true;
        //    string rootPath = "";
        //    string fullFileNameTemp = "";
        //    string fullFileNameOut = "";
        //    string msDosCommand = "";
        //    FileSystemContextFilter contextFilter = FileSystemContextFilter.All;

        //    string linearOutputFormat = "";

        //    if (useLinearOutputFormat)
        //    {
        //        linearOutputFormat = "/B ";
        //        fullFileNameTemp = fullFileNameOut;
        //    }

        //    switch (contextFilter)
        //    {
        //        case FileSystemContextFilter.All:
        //            msDosCommand = $"/C chcp 65001 & dir /S {linearOutputFormat}{rootPath}";
        //            break;
        //        case FileSystemContextFilter.DirectoriesOnly:
        //            msDosCommand = $"/C chcp 65001 & dir /S /A:D {useLinearOutputFormat}{rootPath}";
        //            break;
        //        case FileSystemContextFilter.FilesOnly:
        //            msDosCommand = $"/C chcp 65001 & dir /S /A:-D {useLinearOutputFormat}{rootPath}";
        //            break;
        //    }

        //    //REF-1 - END source
        //    string final = msDosCommand;
        //}

        #endregion comments
    }
}


