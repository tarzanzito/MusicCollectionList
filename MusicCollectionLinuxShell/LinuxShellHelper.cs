using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace MusicCollectionLinux
{
    public class LinuxShellHelper
    {
    
        private StreamWriter _streamWriter;

        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool useLinearOutputFormat = true)
        {
            Log.Information("'LinuxShellHelper.TreeProcess' - Started...");

            string rootPath;
        string fullFileNameOut;
            string fullFileNameTemp;
            string extensionFilter;

            //output files
            if (collectionOriginType == CollectionOriginType.Loss)
            {
                rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                extensionFilter = Constants.FileExtensionsFilterLoss.Trim().Replace("*", "").ToUpper();
            }
            else
            {
                rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                extensionFilter = Constants.FileExtensionsFilterLossLess.Trim().Replace("*", "").ToUpper();
            }

            if (useLinearOutputFormat)
                fullFileNameTemp = fullFileNameOut;

            Log.Information($"Output=[{fullFileNameOut}");

            //TODO: Extensions filter

            string bashCommand = $"ls -a -R -p {rootPath}";
            //startInfo.Arguments = $"-c \"ls -a -R -p\"";
            //startInfo.Arguments = $"-c \"ls -lha -R\"";

            //process 
            bool resultOk = LinuxBashProcess(bashCommand, fullFileNameTemp);

            if (resultOk && (useLinearOutputFormat))
                ChangeOutputToLinearFormat(fullFileNameTemp, fullFileNameOut, contextFilter, extensionFilter);

            Log.Information("'LinuxShellHelper.TreeProcess' - Finished...");
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

        private bool LinuxBashProcess(string bashCommand, string fullFileNameOut)
        {
            bool retValue = true;

            try
            {
                //output                         
                _streamWriter = new StreamWriter(fullFileNameOut, false); //, Constants.StreamsEncoding);

                //Process Info
                System.Diagnostics.ProcessStartInfo startInfo = new();
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
                reader = new StreamReader(fullFileNameTemp); //, Constants.StreamsEncoding);
                writer = new StreamWriter(fullFileNameOut, false); //, Constants.StreamsEncoding);

                bool isFolder = false;
                string baseDir = "";
                string member;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    if ((line == "./") || (line == "../"))
                    {
                        continue;
                    }

                    if (line == ".:")
                    {
                        baseDir = ".";
                        continue;
                    }

                    if (line.EndsWith(':') && line.Contains('/'))
                    {
                        baseDir = line.Substring(0, line.Length - 1);
                        continue;
                    }

                    isFolder = line.EndsWith('/');
                    if (isFolder)
                        member = line.Substring(0, line.Length - 1);
                    else
                        member = line;

                    //TODO: colocar char '/' final da string no linear format 

                    // if (isFolder)
                    // {
                    //     if (contextFilter == FileSystemContextFilter.FilesOnly)
                    //         continue;
                    // }
                    // else
                    // {
                    //     if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    //         continue;
                    // }

                    writer.WriteLine($"{baseDir}{Path.DirectorySeparatorChar}{member}");
                    writer.Flush();
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
            }
        }
    }
}

