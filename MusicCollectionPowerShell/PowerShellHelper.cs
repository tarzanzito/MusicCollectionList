
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using MusicCollectionContext;
using Serilog;
using System.Diagnostics;


//ATENTION:
//if the folder name contains  [ or ] char or others "wildcard characters", "-Path" do not work correctly
//-Path -> no returns files if folder name contains []

//resolve problem :

//using parameter
// -LiteralPath -> return ok

//Read about wildcard characters caracteres curinga
//https://support.microsoft.com/pt-br/office/exemplos-de-caracteres-curinga-939e153f-bd30-47e4-a763-61897c87b3f4
//
namespace MusicCollectionPowerShell
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s /b
    
    // /b = -Name
    
    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse | Out-File "d:\mp3_p.txt"
    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse -Name | Out-File "d:\mp3_p.txt" 

    public class PowerShellHelper
    {
        private string _rootPath;
        private string _fullFileNameTemp;
        private string _fullFileNameOut;
        private FileSystemContextFilter _contextFilter;
        private string _extensionFilter;
        private string[] _extensionFilterArray;
        private bool _applyExtensionsFilter;
        private StreamReader _reader;
        private StreamWriter _writer;

        //////////////////////////////////////////////////
        //Create text file with tree files and directories
        //////////////////////////////////////////////////




        //1-using pipeline - the next command can get the result of the previous command (cmdlet1 | cmdlet2)
        public void TreeProcessUsingPipeline(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyFilterExtensions, bool setToLinearOutputFormat = true)
        {
            Log.Information("'PowerShellHelper.TreeProcessUsingPipeline' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "MusicCollectionMsDos", "TreeProcess");

            Runspace runspace = null;
            Pipeline pipeline = null;

            try
            {
                PrepareVariables(collectionOriginType, contextFilter, applyFilterExtensions, setToLinearOutputFormat);

                if (!Directory.Exists(_rootPath))
                {
                    Log.Error($"Foler Root not exists=[{_rootPath}");
                    return;
                }

                //CreateRunspace

                runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();


                //CreatePipeline
                pipeline = runspace.CreatePipeline();
                
                //Command 1 : Get-Childitem

                Command command1 = new("Get-ChildItem");
                command1.Parameters.Add("LiteralPath", _rootPath);

                ///Context, only: All, Directories, Files
                if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    command1.Parameters.Add("Directory");
                if (contextFilter == FileSystemContextFilter.FilesOnly)
                    command1.Parameters.Add("File");

                //extensions filter
                if ((applyFilterExtensions) && (contextFilter != FileSystemContextFilter.DirectoriesOnly))
                {
                    if (_extensionFilterArray != null)
                        command1.Parameters.Add("Include", _extensionFilterArray); //-Include only works with -Recurse
                    else if (_extensionFilter.Length > 0)
                        command1.Parameters.Add("Filter", _extensionFilter); //only one extension
                }

                command1.Parameters.Add("Recurse");
                //command1.Parameters.Add("Name");  //set output like 'liner format' but do not show difference between Directory and File
                pipeline.Commands.Add(command1);

                //Command 2 : Out-File

                Command command2 = new("Out-File");
                command2.Parameters.Add("File", _fullFileNameTemp); //output file
                pipeline.Commands.Add(command2);

                //Run
                PipelineInvoke(pipeline);

                runspace.Close();

                if (setToLinearOutputFormat)
                    ChangeOutputToLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (pipeline != null)
                {
                    pipeline.Stop();
                    pipeline.Dispose();
                }

                if (runspace != null)
                {
                    runspace.Close();
                    runspace.Dispose();
                }

                Log.Information("'PowerShellHelper.TreeProcessUsingPipeline' - Finish...");
            }
        }

        //2-utilizando comando
        public void TreeProcessUsingCommand(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyFilterExtensions, bool setToLinearOutputFormat = true)
        {
            Log.Information("'PowerShellHelper.TreeProcessUsingCommand' - Started...");

            PowerShell powerShell = null;

            try
            {
                PrepareVariables(collectionOriginType, contextFilter, applyFilterExtensions, setToLinearOutputFormat);

                if (!Directory.Exists(_rootPath))
                {
                    Log.Error($"Foler Root not exists=[{_rootPath}");
                    return;
                }

                //Mount Command1

                Command command1 = new("Get-ChildItem");
                command1.Parameters.Add("LiteralPath", _rootPath);

                //Context, only: All, Directories, Files 
                if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    command1.Parameters.Add("Directory");
                if (contextFilter == FileSystemContextFilter.FilesOnly)
                    command1.Parameters.Add("File");

                //Extension filter
                if ((applyFilterExtensions) && (contextFilter != FileSystemContextFilter.DirectoriesOnly))
                {
                    if (_extensionFilterArray != null)
                        command1.Parameters.Add("Include", _extensionFilterArray); //-Include only works with -Recurse
                    else if (_extensionFilter.Length > 0)
                        command1.Parameters.Add("Filter", _extensionFilter); 
                }

                command1.Parameters.Add("Recurse");
                //powerShell.AddParameter("Name"); //set output like 'liner format' but do not show difference between Directory and File

                //Mount Command2

                Command command2 = new("Out-File");
                command2.Parameters.Add("File", _fullFileNameTemp); //output file

                //Create PowerShell

                powerShell = PowerShell.Create();
                powerShell.Commands.AddCommand(command1);
                powerShell.Commands.AddCommand(command2);

                //RUN
                Invoke(powerShell);

                if (setToLinearOutputFormat)
                    ChangeOutputToLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (powerShell != null)
                {
                    powerShell.Stop();
                    powerShell.Dispose();
                }
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Close();
                }
            }

            Log.Information("'PowerShellHelper.TreeProcessUsingCommand' - Finish...");
        }

        //3-Utilizando script string
        public void TreeProcessUsingScriptString(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            Log.Information("'PowerShellHelper.TreeProcessUsingScriptString' - Started...");

            PowerShell powerShell = null;

            try
            {
                PrepareVariables(collectionOriginType, contextFilter, applyExtensionsFilter, setToLinearOutputFormat);

                if (!Directory.Exists(_rootPath))
                {
                    Log.Error($"Folder Root not exists=[{_rootPath}");
                    return;
                }

                //Context, only: All, Directories, Files 
                string sysContext = "";
                if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    sysContext = "-Directory";
                if (contextFilter == FileSystemContextFilter.FilesOnly)
                    sysContext = "-File";

                string extensionFilterPhrase = "";
                if (applyExtensionsFilter && (contextFilter != FileSystemContextFilter.DirectoriesOnly))
                {
                    //Filter
                    bool hasFilter = _extensionFilter.Trim().Length > 0;
                    bool isFilterArray = _extensionFilterArray != null;

                    if (hasFilter)
                    {
                        if (isFilterArray)
                            extensionFilterPhrase = $"-Include {_extensionFilter}"; //-Include *.log,*.txt
                        else
                            extensionFilterPhrase = $"-Filter {_extensionFilter}"; // "*.FLAC"
                    }
                }

                //mount
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Recurse -Name -File | Out-File 'c:\result.txt'";
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Filter '*.jpg' -Recurse -Name -File | Out-File 'c:\result.txt'";
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Include '*.jpg,*.mp3' -Recurse -Name -File | Out-File 'c:\result.txt'";
                
                //-name -> set outputlike 'liner format' but do not show difference between Directory and File
                //string script = $"Get-ChildItem -LiteralPath '{_rootPath}' {extensionFilterPhrase} -Recurse -Name {sysContext} | Out-File '{_fullFileNameTemp}'";
                string script = $"Get-ChildItem -LiteralPath '{_rootPath}' {extensionFilterPhrase} -Recurse {sysContext} | Out-File '{_fullFileNameTemp}'";

                Log.Information($"Powershell script:{script}");

                //

                powerShell = PowerShell.Create();
                powerShell.AddScript(script);

                //RUN
                Invoke(powerShell);
                
                if (setToLinearOutputFormat)
                    ChangeOutputToLinearFormat();
            }
            catch (Exception ex)
            {
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (powerShell != null)
                {
                    powerShell.Stop();
                    powerShell.Dispose();
                }
            }

            Log.Information("'PowerShellHelper.TreeProcessUsingScriptString' - Finish...");
        }

        private void PrepareVariables(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat)
        {
            _contextFilter = contextFilter;
            _applyExtensionsFilter = applyExtensionsFilter;

            if (collectionOriginType == CollectionOriginType.Loss)
            {
                _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                _extensionFilter = Constants.FileExtensionsFilterLoss;
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLoss);
                _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLoss);
            }
            else
            {
                _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                _extensionFilter = Constants.FileExtensionsFilterLossLess;
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLossLess);
                _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLossLess);
            }

            _extensionFilter = _extensionFilter.Replace("*", "").Replace(" ", "").ToUpper().Trim();

            _extensionFilterArray = null;
            if (_extensionFilter.Contains(','))
                _extensionFilterArray = _extensionFilter.Split(",");

            if (!setToLinearOutputFormat)
                _fullFileNameTemp = _fullFileNameOut;
        }

        private void PipelineInvoke(Pipeline pipeline)
        {
            Log.Information("'PowerShellHelper.PipelineInvoke' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "PowerShellHelper", "PipelineInvoke");

            try
            {
                //RUN
                System.Collections.ObjectModel.Collection<PSObject> results = pipeline.Invoke();

                //show result messages
                foreach (PSObject item in results)
                    Log.Information($"Result: {item}");

                if (pipeline.Error.Count > 0)
                    foreach (object item in pipeline.Error.ReadToEnd())
                        Log.Error($"Error: {item}");
            }
            catch (Exception ex)
            {
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (pipeline != null)
                {
                    pipeline.Stop();
                    pipeline.Dispose();
                }

                Log.Information("'PowerShellHelper.PipelineInvoke' - Finished...");
            }
        }

        private void Invoke(PowerShell powerShell)
        {
            Log.Information("'PowerShellHelper.Invoke' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "PowerShellHelper", "Invoke");

            try
            {
                System.Collections.ObjectModel.Collection<PSObject> results = powerShell.Invoke();
                foreach (PSObject result in results)
                {
                    Log.Warning(result.ToString());
                }

                //if (powerShell.Error.Count > 0)
                //    foreach (object item in pipeline.Error.ReadToEnd())
                //        Log.Error($"Error: {item}");
            }
            catch (Exception ex)
            {
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (powerShell != null)
                {
                    powerShell.Stop();
                    powerShell.Dispose();
                }

                Log.Information("'PowerShellHelper.Invoke' - Finished...");
            }
        }


        /// <summary>
        /// set output like: "dir /B" and append '\' at end if is an folder
        /// </summary>
        private void ChangeOutputToLinearFormat()
        {
            Log.Information("'PowerShellHelper.ChangeOutputToLinearFormat' - Started...");

            if (!File.Exists(_fullFileNameTemp))
            {
                Log.Error($"Folder Root not exists=[{_fullFileNameTemp}");
                return;
            }

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "PowerShellHelper", "ChangeOutputToLinearFormat");

            int count = 0;
            string line = "";

            try
            {
                bool applyExtensionsFilter = _extensionFilter.Length > 0;

                _reader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                _writer = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder = false;
                string baseDir = "";
                string member = "";
                bool isInsideBaseDir = false;
                bool isInsideMember = false;

                while ((line = _reader.ReadLine()) != null)
                {
                    if (line == "")
                    {
                        if (isInsideBaseDir) //finalize directory base name
                            isInsideBaseDir = false;

                        if (isInsideMember) //finalize member (dir/file)
                        {
                            WriteValidLine(baseDir, member, isFolder);
                            isInsideMember = false;
                        }
                        continue;
                    }

                    if (line.Length < 15)
                        continue;

                    if (isInsideBaseDir)
                    {
                        if (line.Substring(0, 15).Trim() == "") //append directory base name and continue
                        {
                            baseDir += line.Substring(15).Trim();
                            continue;
                        }
                    }

                    if (line.Substring(0, 15) == "    Directory: ") //init directory base name
                    {
                        baseDir = line.Substring(15).Trim();
                        isInsideBaseDir = true;
                        continue;
                    }

                    /////

                    if (line.Length < 50)
                        continue;

                    if (isInsideMember)
                    {
                        if (line.Substring(0, 50).Trim() == "") //append member name and continue
                        {
                            member += line.Substring(50).Trim();
                            continue;
                        }
                    }

                    //is new member
                    if (DateTime.TryParse(line.Substring(15, 10), out DateTime dt)) //init member name
                    {
                        if (isInsideMember)
                            WriteValidLine(baseDir, member, isFolder);

                        isFolder = (line.Substring(0, 1) == "d");
                        member = line.Substring(50).Trim();
                        isInsideMember = true;
                        continue;
                    }
                }

                if (isInsideMember)
                    WriteValidLine(baseDir, member, isFolder);

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
                if (_reader != null)
                {
                    _reader.Close();
                }
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Close();
                }

                Utils.Stopwatch(stopwatch, "PowerShellHelper", "ChangeOutputToLinearFormat");

                Log.Information("'PowerShellHelper.ChangeOutputToLinearFormat' - Finished...");
            }
        }

        private void WriteValidLine(string baseDir, string member, bool isFolder)
        {
            char dirMark = '\0';
            bool isValid = true;

            //Verify Context Filter
            if (isFolder)
            {
                if (_contextFilter == FileSystemContextFilter.FilesOnly)
                    return;
            }
            else
            {
                if (_contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    return;
            }

            //Apply Extensions Filter
            if (isFolder)
            {
                //append 'DirectorySeparatorChar' at end
                dirMark = Path.DirectorySeparatorChar;
            }
            else
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
                _writer.WriteLine($"{baseDir}{Path.DirectorySeparatorChar}{member}{dirMark}");
                _writer.Flush();
            }
        }
    }




    //Impersonator ?!?!??!?
    //using ( new Impersonator( "myUsername", "myDomainname", "myPassword" ) )
    //{
    //using (RunspaceInvoke invoker = new RunspaceInvoke())
    //{
    //invoker.Invoke("Set-ExecutionPolicy Unrestricted");
    //}
    //}
}

//Collection<PSObject> res;
//using PowerShell powershell = PowerShell.Create();
//powershell.AddScript("param($param1) $d = get-date; $s = 'test string value'; " + "$d; $s; $param1;");
//powershell.AddParameter("param1", "parameter 1 value!");
//res = powershell.Invoke();


////////////////////////////////////////////////////////////////

//To Learning.....
//private Process GetProcessByName(string name)
//{
//    Collection<Process> foundProcesses;

//    using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
//    {
//        ps.AddCommand("Get-Process").AddParameter("Name", name);
//        foundProcesses = ps.Invoke<Process>();
//    }

//    if (foundProcesses.Count == 0)
//    {
//        ThrowTerminatingError(
//        new ErrorRecord(
//                    new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessNoProcessFoundWithName, name)),
//                    "EnterPSHostProcessNoProcessFoundWithName",
//                    ErrorCategory.InvalidArgument,
//                    this)
//                );
//    }
//    else if (foundProcesses.Count > 1)
//    {
//        ThrowTerminatingError(
//        new ErrorRecord(
//                    new PSArgumentException(StringUtil.Format(RemotingErrorIdStrings.EnterPSHostProcessMultipleProcessesFoundWithName, name)),
//                    "EnterPSHostProcessMultipleProcessesFoundWithName",
//                    ErrorCategory.InvalidArgument,
//                    this)
//                );
//    }

//    return foundProcesses[0];
//}