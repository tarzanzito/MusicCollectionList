using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicCollectionLinux
{
    public class LinuxShellHelper
    {
        private string _rootPath;
        private string _fullFileNameOut;
        private string _fullFileNameTemp;
        private FileSystemContextFilter _contextFilter;
        private string _extensionFilter;
        private bool _applyExtensionsFilter;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            Log.Information("'LinuxShellHelper.TreeProcess' - Started...");
            try
            {
                _contextFilter = contextFilter;
                _applyExtensionsFilter = applyExtensionsFilter;

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLoss);
                    _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLoss);
                    _extensionFilter = Constants.FileExtensionsFilterLoss.Trim().Replace("*", "").ToUpper();
                }
                else
                {
                    _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLossLess);
                    _extensionFilter = Constants.FileExtensionsFilterLossLess.Trim().Replace("*", "").ToUpper();
                }

                if (setToLinearOutputFormat)
                    _fullFileNameTemp = _fullFileNameOut;

                if (!Directory.Exists(_rootPath))
                {
                    Log.Error($"Foler Root not exists=[{_rootPath}");
                    return;
                }


                Log.Information($"Output=[{_fullFileNameOut}");

                string bashCommand = $"ls -l -h -a -p -R {_rootPath}";
                //-p serves to put char '/' at the end of each folder, but this option seems not to be necessary

                //process 
                bool resultOk = LinuxBashProcess(bashCommand);

                if (resultOk && (setToLinearOutputFormat))
                    ChangeOutputToLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR EXCEPTION: {ex.Message}");
            }
            finally
            {
                Log.Information("'LinuxShellHelper.TreeProcess' - Finished...");
            }
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

        private bool LinuxBashProcess(string bashCommand)
        {
            Log.Information("'LinuxShellHelper.LinuxBashProcess' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "LinuxShellHelper", "LinuxBashProcess");

            bool retValue = true;

            try
            {
                //output                         
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                //Process Info
                ProcessStartInfo startInfo = new();
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = $"-c \"{bashCommand}\"";

                //dos without window
                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                //startInfo.UseShellExecute = false;
                //startInfo.CreateNoWindow = false;

                //output to files
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                //Process
                System.Diagnostics.Process process = new();
                process.StartInfo = startInfo;

                //V1
                process.OutputDataReceived += OutputDataReceived;
                process.ErrorDataReceived += ErrorDataReceived;

                //V2
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
                Log.Error($"Command:{bashCommand}");
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

                Utils.Stopwatch(stopwatch, "LinuxShellHelper", "LinuxBashProcess");

                Log.Information("'LinuxShellHelper.LinuxBashProcess' - Finished...");
            }

            return retValue;
        }

        /// <summary>
        /// set output like linear fprmat: "dir /B"
        /// lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3
        /// </summary>
        /// <param name="contextFilter"></param>
        
        private void ChangeOutputToLinearFormat()
        {
            Log.Information("LinuxShellHelper.ChangeOutputToLinearFormat Started");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");

            //_fullFileNameTemp = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless.txt";
            //_fullFileNameOut = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless_new.txt";
            //_applyExtensionsFilter = false;

            if (!File.Exists(_fullFileNameTemp))
            {
                Log.Error($"Folder Root not exists=[{_fullFileNameTemp}");
                return;
            }

            _streamReader = null;
             _streamWriter = null;

            string line = "";

            try
            {
                _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder;
                bool isValid = true;
                bool useRootPath = true;
                string basePath = "";
                string rootPath;
                string member = "";

                if (_rootPath.EndsWith('/'))
                    rootPath = _rootPath.Substring(0, _rootPath.Length - 1);
                else
                    rootPath = _rootPath;

                while ((line = _streamReader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    if ((line.Length > 5) && (line.StartsWith("total ")))
                        continue;

                    if (line == ".:")
                    {
                        useRootPath = true;
                        basePath = "/";
                        continue;
                    }

                    if (line.EndsWith(':') && line.Contains('/'))
                    {
                        useRootPath = line.StartsWith('.');

                        if (useRootPath)
                            basePath = line.Substring(1, line.Length - 2);
                        else
                            basePath = line.Substring(0, line.Length - 1);

                        continue;
                    }

                    if (line.Length < 72)
                        continue;

                    if (!DateTime.TryParse(line.Substring(56, 10), out DateTime dt))
                        continue;

                    
                    //isFolder = line.EndsWith('/');
                    isFolder = line.StartsWith('d');
                    isValid = true;

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

                    member = line.Substring(72);

                    if ((member == "./") || (member == "../"))
                        continue;

                    //Apply Extensions Filter
                    if (!isFolder)
                    {
                        //verify Extensions Filter
                        if (_applyExtensionsFilter)
                        {
                            string extension = Path.GetExtension(member).ToUpper().Trim();
                            isValid = _extensionFilter.Contains(extension);
                        }
                    }

                    //write
                    if (isValid)
                    {
                        //_rootPath
                        string newLine;

                        newLine = System.IO.Path.Join(_rootPath, basePath, member);
                        if (useRootPath)

                            newLine = $"{rootPath}{basePath}{line}";//{Path.DirectorySeparatorChar}
                        else
                            newLine = $"{basePath}{line}";//{Path.DirectorySeparatorChar}

                        _streamWriter.WriteLine(newLine); 
                        _streamWriter.Flush();
                    }
                }
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
            }

            Utils.Stopwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");

            Log.Information("LinuxShellHelper.ChangeOutputToLinearFormat Finished");
        }
    }
}

