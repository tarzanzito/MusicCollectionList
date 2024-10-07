
using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;


namespace MusicCollectionMsDos
{
    public class MsDosShellHelper
    {
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;
        private string _fullFileNameTemp = string.Empty;
        private string _fullFileNameOut = string.Empty;
        private FileSystemContextFilter _contextFilter;
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
        /// <param name="collectionOriginType"></param>
        /// <param name="contextFilter"></param>
        /// <param name="applyExtensionsFilter"></param>
        /// <param name="setToLinearOutputFormat"></param>
        /// 
        public bool TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            Log.Information("'MusicCollectionMsDos.TreeProcess' - Started...");
            
            bool resultOk = false;

            try
            {
                _contextFilter = contextFilter;
                _applyExtensionsFilter = applyExtensionsFilter;

                string rootPath;

                //output files
                switch (collectionOriginType)
                {
                    case CollectionOriginType.Lossless:
                        rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                        _fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                        _fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                        if (applyExtensionsFilter)
                            _extensionFilter = Constants.FileExtensionsFilterLossLess;
                        break;
                    case CollectionOriginType.Loss:
                        rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                        _fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                        _fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                        if (applyExtensionsFilter)
                            _extensionFilter = Constants.FileExtensionsFilterLoss;

                        break;
                    default:
                        throw new Exception("CollectionOriginType error in 'MusicCollectionMsDos.TreeProcess')");
                }

                _extensionFilter = _extensionFilter.Replace("*", "").Replace(" ", "").ToUpper().Trim();

                if (applyExtensionsFilter)
                    _applyExtensionsFilter = _extensionFilter.Length > 0;

                if (!Directory.Exists(rootPath))
                {
                    Log.Error($"Folder Root not exists=[{rootPath}");
                    return false;
                }

                if (!setToLinearOutputFormat)
                    _fullFileNameTemp = _fullFileNameOut;

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

                Log.Information($"Output File:[{_fullFileNameOut}]");
                Log.Information($"Context Filter:[{_contextFilter}]");
                Log.Information($"Apply Extensions Filter:[{_applyExtensionsFilter}]");
                Log.Information($"Extensions Filter:[{_extensionFilter}]");
                Log.Information($"MS-DOS Command:[{msDosCommand}]");

                //process 
                resultOk = MsDosProcess(msDosCommand);

                if (resultOk && setToLinearOutputFormat)
                    ChangeToCsvLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR EXCEPTION: {ex.Message}");
                resultOk = false;
            }
            finally
            {
                Log.Information("'MusicCollectionMsDos.TreeProcess' - Finished...");
            }

            return resultOk;
        }

        private bool MsDosProcess(string msDosCommand)
        {
            Log.Information("'MusicCollectionMsDos.MsDosProcess' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");
            
            bool retValue = true;

            try
            {
                //output
                _streamWriter = new StreamWriter(_fullFileNameTemp, false, Constants.StreamsEncoding);

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
                Log.Error($"Command:{msDosCommand}");
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
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

                Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");

                Log.Information("'MusicCollectionMsDos.MsDosProcess' - Finished...");
            }

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

            _streamWriter.WriteLine(e.Data);
            _streamWriter.Flush();
        }

        /// <summary>
        /// set output like linear format (like "dir /B") but adding char '\' at the end of folder entries
        /// lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\   "folder" ('\' at end)
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3  "file"
        /// 
        /// linux relation command
        /// ls -l -h -a -p -R Path
        //
        // -l  -- list with long format - show permissions
        // -h  -- list long format with readable file size
        // -a  -- list all files including hidden file starting with '.'
        // -p  -- indicator-style=slash - append '/' indicator to directories
        // -R  -- list recursively directory tree
        /// </summary>
        private void ChangeToCsvLinearFormat()
        {
            Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

            _streamReader = null;
            _streamWriter = null;

            string? line = null;
            int countFolders = 0;
            int countFiles = 0;

            try
            {
                if (!File.Exists(_fullFileNameTemp))
                    throw new Exception($"InputFile:'{_fullFileNameTemp}' not found.");

                if (!CanCreateFile(_fullFileNameOut))
                    throw new Exception($"OutputFile:'{_fullFileNameOut}' cannot be created.");

                bool isFolder = false;
                bool isValid = true;
                string baseDir = "";
                string item;
                
                //using (StreamReader reader = new StreamReader(fileName)) //C# 8
                //{
                //}
                //using var _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);

                _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                while ((line = _streamReader.ReadLine()) != null)
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

                    //Verify Context Filter
                    if (isFolder)
                    {
                        if (_contextFilter == FileSystemContextFilter.FilesOnly)
                            continue;
                    }
                    else
                    {
                        if (_contextFilter == FileSystemContextFilter.DirectoriesOnly)
                            continue;
                    }

                    //Apply Extensions Filter
                    if (isFolder)
                        isValid = true;
                    else
                    {
                        //verify Extensions Filter
                        if (_applyExtensionsFilter)
                        {
                            string extension = Path.GetExtension(item).ToUpper().Trim();
                            isValid = _extensionFilter.Contains(extension);
                        }
                        else
                            isValid = true;
                    }

                    //write
                    if (isValid)
                    {
                        string newLine = $"{baseDir}{Path.DirectorySeparatorChar}{item}";

                        if (isFolder)
                            newLine += Path.DirectorySeparatorChar;

                        _streamWriter.WriteLine(newLine);
                        _streamWriter.Flush();
                    }

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
                _streamReader?.Close();
                //_streamWriter?.Flush();
                _streamWriter?.Close();
                _streamWriter?.Dispose();

                Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

                Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Finished...");
            }
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


