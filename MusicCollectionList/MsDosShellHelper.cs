using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace MusicCollectionList
{
    public class MsDosShellHelper
    {
        private string _rootPath;
        private string _fullFileNameOut;
        private StreamWriter _streamWriter;

    public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter)
        {
            Log.Information("'MsDosShellHelper.TreeProcess' - Started...");

            try
            {
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    _rootPath = Constants.FolderRootCollectionLoss;
                    _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLoss);
                }
                else
                {
                    _rootPath = Constants.FolderRootCollectionLossLess;
                    _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLossLess);
                }

                //ProcessStartInfo
                _streamWriter = new StreamWriter(_fullFileNameOut);

                //Process Info
                System.Diagnostics.ProcessStartInfo startInfo = new();
                //basic

                //TODO: Extensions filter

                startInfo.FileName = "cmd.exe";
                switch (contextFilter)
                {
                    case FileSystemContextFilter.All:
                        startInfo.Arguments = $"/C chcp 65001 & dir /S /B {_rootPath}";
                        break;
                    case FileSystemContextFilter.DirectoriesOnly:
                        startInfo.Arguments = $"/C chcp 65001 & dir /S /A:D /B {_rootPath}";
                        break;
                    case FileSystemContextFilter.FilesOnly:
                        startInfo.Arguments = $"/C chcp 65001 & dir /S /A:-D /B {_rootPath}";
                        break;
                }
                //  /S     - All sub-folders (tree)
                //  /A:-D  - Get all except folders -- /A:D Get only folders
                //  /B     - linear output format
                //
                //@chcp 65001
                //@dir /b /s \\NAS - QNAP\music\_COLLECTION\*.* >% 1


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
                Log.Error(ex.Message);
                Log.Error(_fullFileNameOut);
            }
            finally
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter.Dispose();
            }

            Log.Information("'MsDosShellHelper.TreeProcess' - Finished...");
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
    }
}
