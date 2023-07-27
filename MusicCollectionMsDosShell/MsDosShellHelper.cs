using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace MusicCollectionMsDos
{
    public class MsDosShellHelper
    {
        private StreamWriter _streamWriter;

        /// <summary>
        /// 
        //https://ss64.com/nt/dir.html
        //  /S     - All sub-folders (tree)
        //  /A:-D  - Get all except folders
        //  /A:D   - Get only folders
        //  /B     - linear output format, Bare format (no heading, file sizes or summary).
        //           but do not show diference between files and directories.
        //           bash 'ls' command in linux add char '/' at end if directory
        //
        /// linear output format examples: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3
        /// </summary>
        /// <param name="collectionOriginType"></param>
        /// <param name="contextFilter"></param>
        /// <param name="useLinearOutputFormat"></param>
        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool useLinearOutputFormat = true)
        {
            try
            {
                Log.Information("'MusicCollectionMsDos.TreeProcess' - Started...");

                string rootPath;
                string fullFileNameOut;
                string fullFileNameTemp;
                string extensionFilter = "";

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                    if (applyExtensionsFilter)
                        extensionFilter = Constants.FileExtensionsFilterLoss.Trim().Replace("*", "").ToUpper();
                }
                else
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                    if (applyExtensionsFilter)
                        extensionFilter = Constants.FileExtensionsFilterLossLess.Trim().Replace("*", "").ToUpper();
                }

                //make dos command
                string linearOutputFormat = "";
                string msDosCommand = "";

                //if (useLinearOutputFormat)
                //{
                //    linearOutputFormat = "/B "; 
                //    fullFileNameTemp = fullFileNameOut;
                //}
                if (!useLinearOutputFormat)
                    fullFileNameTemp = fullFileNameOut;

                switch (contextFilter)
                {
                    case FileSystemContextFilter.All:
                        msDosCommand = $"/C chcp 65001 & dir /S {linearOutputFormat}{rootPath}";
                        break;
                    case FileSystemContextFilter.DirectoriesOnly:
                        msDosCommand = $"/C chcp 65001 & dir /S /A:D {useLinearOutputFormat}{rootPath}";
                        break;
                    case FileSystemContextFilter.FilesOnly:
                        msDosCommand = $"/C chcp 65001 & dir /S /A:-D {useLinearOutputFormat}{rootPath}";
                        break;
                }

                Log.Information($"Output File:[{fullFileNameOut}]");
                Log.Information($"Context Filter:[{contextFilter.ToString()}]");
                Log.Information($"Apply Extensions Filter:[{applyExtensionsFilter.ToString()}]");
                Log.Information($"Extensions Filter:[{extensionFilter}]");
                Log.Information($"MS-DOS Command:[{msDosCommand}]");

                //process 
                bool resultOk = MsDosProcess(msDosCommand, fullFileNameTemp);

                if (resultOk && (useLinearOutputFormat))
                    ChangeOutputToLinearFormat(fullFileNameTemp, fullFileNameOut, contextFilter, extensionFilter);
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

        private bool MsDosProcess(string msDosCommand, string fullFileNameOut)
        {
            bool retValue = true;

            try
            {
                Log.Information("'MusicCollectionMsDos.MsDosProcess' - Started...");

                //output
                _streamWriter = new StreamWriter(fullFileNameOut, false, Constants.StreamsEncoding);

                //Process Info
                System.Diagnostics.ProcessStartInfo startInfo = new();
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = msDosCommand;

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
                Log.Error($"Command:{msDosCommand}");
                Log.Error($"Outout:{fullFileNameOut}");
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
        /// set output like linear fprmat: "dir /B"
        /// lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3
        /// </summary>
        /// <param name="contextFilter"></param>
        private void ChangeOutputToLinearFormat(string fullFileNameTemp, string fullFileNameOut, FileSystemContextFilter contextFilter, string extensionFilter)
        {
            if (!File.Exists(fullFileNameTemp))
                return;

            StreamReader reader = null;
            StreamWriter writer = null;
            int count = 0;
            string line = "";

            try
            {
                Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Syarted...");

                bool applyExtensionsFilter = extensionFilter.Length > 0;

                reader = new StreamReader(fullFileNameTemp, Constants.StreamsEncoding);
                writer = new StreamWriter(fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder = false;
                bool isValid = false;
                string baseDir = "";
                string member;
                char dirMark = '\0';

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 14)
                        continue;

                    if (line.Substring(0, 14) == " Directory of ")
                    {
                        baseDir = line.Substring(14);
                        continue;
                    }

                    if (line.Length < 37)
                        continue;

                    if (!DateTime.TryParse(line.Substring(0, 10), out DateTime dt))
                        continue;

                    isFolder = (line.Substring(21, 5) == "<DIR>");

                    member = line.Substring(36);

                    if (isFolder && (member == ".") || (member == ".."))
                        continue;

                    //

                    if (isFolder)
                    {
                        if (contextFilter == FileSystemContextFilter.FilesOnly)
                            continue;
                        dirMark = Path.DirectorySeparatorChar;
                        isValid = true;
                    }
                    else
                    {
                        if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                            continue;

                        //applyExtensionsFilter
                        if (applyExtensionsFilter)
                        {
                            string extension = Path.GetExtension(member).ToUpper().Trim();
                            isValid = extensionFilter.Contains(extension);
                        }

                        dirMark = '\0';
                    }

                    if (isValid)
                    {
                        writer.WriteLine($"{baseDir}{Path.DirectorySeparatorChar}{member}{dirMark}");
                        writer.Flush();
                    }
                }

                Log.Information(count.ToString());

            }
            catch (Exception ex)
            {
                Log.Error($"Line:{line}");
                Log.Error($"Outout:{fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }

                Log.Information("'MusicCollectionMsDos.ChangeOutputToLinearFormat' - Finished...");
            }
        }
    }
}


