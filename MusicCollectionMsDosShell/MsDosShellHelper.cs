
using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;


namespace MusicCollectionMsDos
{
    public class MsDosShellHelper
    {
         private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private string _fullFileNameTemp;
        private string _fullFileNameOut;
        private FileSystemContextFilter _contextFilter;
        private string _extensionFilter;
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
        /// This method add '\' at end of any folder.
        /// 
        /// </summary>
        /// <param name="collectionOriginType"></param>
        /// <param name="contextFilter"></param>
        /// <param name="applyExtensionsFilter"></param>
        /// <param name="setToLinearOutputFormat"></param>
        /// 
        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            Log.Information("'MusicCollectionMsDos.TreeProcess' - Started...");

            try
            {
                _contextFilter = contextFilter;
                _applyExtensionsFilter = applyExtensionsFilter;

                string rootPath;

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    _fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    _fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLoss.Trim().Replace("*", "").ToUpper();
                }
                else
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    _fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    _fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLossLess.Trim().Replace("*", "").ToUpper();
                }

                if (applyExtensionsFilter)
                    _applyExtensionsFilter = _extensionFilter.Length > 0;

                if (!Directory.Exists(rootPath))
                {
                    Log.Error($"Folder Root not exists=[{rootPath}");
                    return;
                }

                if (!setToLinearOutputFormat)
                    _fullFileNameTemp = _fullFileNameOut;

                //make MS-DOS command

                //NOTE: because the /B option does not show the difference between folder and file, /B is no longer used)
                //(but can see on DesableOption_B();
                
                string msDosCommand = "";

                switch (contextFilter)
                {
                    case FileSystemContextFilter.All:
                        msDosCommand = $"chcp 65001 & dir /S {rootPath}";
                        break;
                    case FileSystemContextFilter.DirectoriesOnly:
                        msDosCommand = $"chcp 65001 & dir /S /A:D {rootPath}";
                        break;
                    case FileSystemContextFilter.FilesOnly:
                        msDosCommand = $"chcp 65001 & dir /S /A:-D {rootPath}";
                        break;
                }

                Log.Information($"Output File:[{_fullFileNameOut}]");
                Log.Information($"Context Filter:[{_contextFilter}]");
                Log.Information($"Apply Extensions Filter:[{_applyExtensionsFilter}]");
                Log.Information($"Extensions Filter:[{_extensionFilter}]");
                Log.Information($"MS-DOS Command:[{msDosCommand}]");

                //process 
                bool resultOk = MsDosProcess(msDosCommand);

                if (resultOk && setToLinearOutputFormat)
                    ChangeOutputToLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR EXCEPTION: {ex.Message}");
            }
            finally
            {
                Log.Information("'MusicCollectionMsDos.TreeProcess' - Finished...");
            }
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
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                //Process Info
                ProcessStartInfo startInfo = new();
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

                //V2 - with lambdar (i dont like)
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
                    _streamWriter.Flush();
                    _streamWriter.Close();
                    _streamWriter.Dispose();
                }

                Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");

                Log.Information("'MusicCollectionMsDos.MsDosProcess' - Finished...");
            }

            return retValue;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _streamWriter.WriteLine("ERROR:" + e.Data);
            _streamWriter.Flush();
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _streamWriter.WriteLine(e.Data);
            _streamWriter.Flush();
        }

        /// <summary>
        /// set output like linear format ("dir /B") but adding char '\' at the end of folder entries
        /// lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\   "folder"
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3  "file"
        /// </summary>
        private void ChangeOutputToLinearFormat()
        {
            Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Started...");

            if (!File.Exists(_fullFileNameTemp))
                return;

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

            StreamWriter writer = null;
            int count = 0;
            string line = "";

            try
            {
                _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder = false;
                bool isValid = false;
                string baseDir = "";
                string item;
                char dirMark;

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

                    //

                    //Verify Context Filter
                    if (isFolder)
                        if (_contextFilter == FileSystemContextFilter.FilesOnly)
                            continue;
                    else
                        if (_contextFilter == FileSystemContextFilter.DirectoriesOnly)
                            continue;

                    //Apply Extensions Filter
                    if (isFolder)
                    {
                        //append 'DirectorySeparatorChar' at end
                        dirMark = Path.DirectorySeparatorChar;
                        isValid = true;
                    }
                   else
                    {
                        //verify Extensions Filter
                        if (_applyExtensionsFilter)
                        {
                            string extension = Path.GetExtension(item).ToUpper().Trim();
                            isValid = _extensionFilter.Contains(extension);
                        }
                        dirMark = '\0';
                    }

                    //write
                    if (isValid)
                    {
                        writer.WriteLine($"{baseDir}{Path.DirectorySeparatorChar}{item}{dirMark}");
                        writer.Flush();
                    }
                }

                Log.Information(count.ToString());

            }
            catch (Exception ex)
            {
                Log.Error($"Line:{line}");
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (_streamReader != null)
                {
                    _streamReader.Close();
                    _streamReader.Dispose();
                }
                if (_streamWriter != null)
                {
                    _streamWriter.Flush();
                    _streamWriter.Close();
                    _streamWriter.Dispose();
                }

                Utils.Stopwatch(stopwatch, "MusicCollectionMsDos", "ChangeOutputToLinearFormat");

                Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Finished...");
            }
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


