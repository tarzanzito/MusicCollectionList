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

    public void TreeProcess()
        {
            Log.Information("'MsDosShellHelper.TreeProcess' - Started...");

            try
            {
                    _rootPath = ".";
                    _fullFileNameOut = System.IO.Path.Join(_rootPath, "xpto.txt");

                    Log.Information($"Output=[{_fullFileNameOut}");

                //ProcessStartInfo
                _streamWriter = new StreamWriter(_fullFileNameOut);

                //Process Info
                System.Diagnostics.ProcessStartInfo startInfo = new();
                //basic

                //TODO: Extensions filter

                startInfo.FileName = "/bin/bash";
               
// ls -d  only folders


                //xpto1 startInfo.Arguments = $"-c \"ls -lha -R\"";
                startInfo.Arguments = $"-c \"ls -a -R -p\"";
                
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
                Log.Information("'START !!!!!!!!!!!!!!!!!!!!!!");
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

