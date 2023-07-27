
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using MusicCollectionContext;
using Serilog;


//ATENTION:
//if the folder name contains  [ or ] char or others "wildcard characters", "-Path" do not work correctly
// -Path -> no returns files if file name contains []

//resolve problem :

//using parameter
// -LiteralPath -> return ok

//Read about wildcard characters caracteres curinga
//https://support.microsoft.com/pt-br/office/exemplos-de-caracteres-curinga-939e153f-bd30-47e4-a763-61897c87b3f4
//
namespace MusicCollectionPowerShell
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse | Out-File "d:\mp3_p.txt"
    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse -Name | Out-File "d:\mp3_p.txt" 

    public class PowerShellHelper
    {
        private string _rootPath;
        private string _fullFileNameTemp;
        private string _fullFileNameOut;
        private string _extensionFilter;
        private string[] _extensionFilterArray;

        //////////////////////////////////////////////////
        //Create text file with tree files and directories
        //////////////////////////////////////////////////

        //1-using pipeline - o comando seguinte PODE obter o resultado do comando anterior
        public void TreeProcessUsingPipeline(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyFilterExtensions, bool useLinearOutputFormat = true)
        {
            Runspace runspace = null;
            Pipeline pipeline = null;

            try
            {
                PrepareVariables(collectionOriginType);

                ///

                runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
               
                pipeline = runspace.CreatePipeline();
                
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
                        command1.Parameters.Add("Include", _extensionFilterArray);
                    else if (_extensionFilter.Length > 0)
                        command1.Parameters.Add("Filter", _extensionFilter);
                }

                command1.Parameters.Add("Recurse");
                command1.Parameters.Add("Name"); //output format
                pipeline.Commands.Add(command1);

                // like: command1 | command2

                Command command2 = new("Out-File");
                command2.Parameters.Add("File", _fullFileNameTemp); //output file
                pipeline.Commands.Add(command2);
                
                //Run
                System.Collections.ObjectModel.Collection<PSObject> results = pipeline.Invoke();

                //show result messages
                foreach (PSObject item in results)
                    Log.Information($"Result: {item}");

                if (pipeline.Error.Count > 0)
                    foreach (object item in pipeline.Error.ReadToEnd())
                        Log.Error($"Error: {item}");

                runspace.Close();

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
            }
        }

        //2-utilizando comando
        public void TreeProcessUsingCommand(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyFilterExtensions, bool useLinearOutputFormat = true)
        {
            PowerShell powerShell = null;
            StreamWriter writer = null;

            try
            {
                PrepareVariables(collectionOriginType);

                //Mount Command
                powerShell = PowerShell.Create();
                powerShell.AddCommand("Get-ChildItem");
                powerShell.AddParameter("LiteralPath", _rootPath);

                //Context, only: All, Directories, Files 
                if (contextFilter == FileSystemContextFilter.DirectoriesOnly)
                    powerShell.AddParameter("Directory");
                if (contextFilter == FileSystemContextFilter.FilesOnly)
                    powerShell.AddParameter("File");

                //Extension filter
                if ((applyFilterExtensions) && (contextFilter != FileSystemContextFilter.DirectoriesOnly))
                {
                    if (_extensionFilterArray != null)
                        powerShell.AddParameter("Include", _extensionFilterArray);
                    else if (_extensionFilter.Length > 0)
                        powerShell.AddParameter("Filter", _extensionFilter);
                }

                powerShell.AddParameter("Recurse");
                powerShell.AddParameter("Name");

                System.Collections.ObjectModel.Collection<PSObject> results = powerShell.Invoke();

                writer = new(_fullFileNameTemp);

                foreach (PSObject item in results)
                {
                    writer.WriteLine(item.ToString());
                    writer.Flush();
                }

                //saber como passar results para input de "Out-File"  like "dir>aa.txt"
                //ps.Commands.Clear();
                //ps.AddCommand("Out-File");
                //ps.AddParameter("File", @"d:\mp3_bbbb.txt");
                //results = ps.Invoke();

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
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
            }
        }

        //3-Utilizando script string
        public void TreeProcessUsingScriptString(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool useLinearOutputFormat = true)
        {
            PowerShell powerShell = null;

            try
            {
                PrepareVariables(collectionOriginType);

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
                    bool isFilterArray = _extensionFilterArray != null;
                    bool hasFilter = _extensionFilter.Trim().Length > 0;

                    if (isFilterArray)
                    {
                        extensionFilterPhrase = $"-Include ({_extensionFilter})";
                    }
                    else if (hasFilter)
                        extensionFilterPhrase = $"-Filter {_extensionFilter}";
                }

                //mount
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Recurse -Name -File | Out-File 'c:\result.txt'";
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Filter '*.jpg' -Recurse -Name -File | Out-File 'c:\result.txt'";
                //"Get-ChildItem -LiteralPath 'C:\\Test' -Include '*.jpg,*.mp3' -Recurse -Name -File | Out-File 'c:\result.txt'";
                string script = $"Get-ChildItem -LiteralPath '{_rootPath}' {extensionFilterPhrase} -Recurse -Name {sysContext} | Out-File '{_fullFileNameTemp}'";

                Log.Information($"Powershell Command:{script}");

                //

                powerShell = PowerShell.Create();
                powerShell.AddScript(script);

                ///

                System.Collections.ObjectModel.Collection<PSObject> results = powerShell.Invoke();
                foreach (PSObject result in results)
                {
                    Log.Information(result.ToString());
                    Console.WriteLine(result);
                }

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
        }

        private void PrepareVariables(CollectionOriginType collectionOriginType)
        {
            if (collectionOriginType == CollectionOriginType.Loss)
            {
                _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                _extensionFilter = FilterVerify(Constants.FileExtensionsFilterLoss);
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLoss);
                _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLoss);
            }
            else
            {
                _rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                _extensionFilter = FilterVerify(Constants.FileExtensionsFilterLossLess);
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLossLess);
                _fullFileNameTemp = System.IO.Path.Join(_rootPath, Constants.TreeTempFileNameCollectionLossLess);
            }

            bool isFilterArray = _extensionFilter.Contains(',');

            _extensionFilterArray = null;
            if (isFilterArray)
                _extensionFilterArray = _extensionFilter.Split(",");
        }

        /// <summary>
        /// set output like: "dir /B"
        /// </summary>
        /// <param name="contextFilter"></param>
        private void ChangeOutputToLinearFormat()
        {
            if (!File.Exists(_fullFileNameTemp))
                return;

            StreamReader reader = null;
            StreamWriter writer = null;
            int count = 0;
            string line = "";

            try
            {
                reader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                writer = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                while ((line = reader.ReadLine()) != null)
                {
                    //TODO: colocar char '/' final da string no linear format 
                    writer.WriteLine($"{_rootPath}{line}");
                    writer.Flush();
                }

                Log.Information($"Processed lines: {count}");

            }
            catch (Exception ex)
            {
                Log.Error($"Line:{line}");
                Log.Error($"Outout:{_fullFileNameOut}");
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

        /// <summary>
        /// Input : "*.MP3, *.WMA"
        /// Output: "'*.MP3','*.WMA'
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string FilterVerify(string filter)
        {
            if (filter.Trim().Length == 0)
                return "";

            if (!filter.Contains(','))
                return filter.Trim();

            string res = "";
            string[] array = filter.Split(',');
            foreach (var item in array)
            {
                string tmp = item.Trim().Replace("\"", "").Replace("'", "");
                res += $"'{tmp}',";
            }
            res = res.TrimEnd(',');

            return res;
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