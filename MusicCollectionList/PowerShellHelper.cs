
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using System.IO;


//ATENTION:
//if the folder name contains  [ or ] char or others "wildcard characters", "-Path" do not work correctly
// -Path -> no returns files if file name contains []

//resolve problem :

//using parameter
// -LiteralPath -> return ok

//Read about wildcard characters caracteres curinga
//https://support.microsoft.com/pt-br/office/exemplos-de-caracteres-curinga-939e153f-bd30-47e4-a763-61897c87b3f4
//
namespace MusicCollection
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse | Out-File "d:\mp3_p.txt"
    //Get-ChildItem -LiteralPath "\\NAS-QNAP\music\_COLLECTION" -Filter "*.mp3" -Recurse -Name | Out-File "d:\mp3_p.txt" 

    internal class PowerShellHelper
    {
        private string _rootPath;
        private string _fullFileNameOut;
        private string _filter;
        private string[] _filterArray;
        private string _filterParameter;

        //cria ficheiro texto com a arvore de directorios e ficheiros

        //1-utilizando pipeline - o comando seguinte PODE obter o resultado do comando anterior
        public void PowerShellRunWithPipelne(CollectionOriginType collectionOriginType)
        {
            PrepareVariables(collectionOriginType);

            ///

            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();

            Command command1 = new("Get-ChildItem");
            command1.Parameters.Add("LiteralPath", _rootPath);

            if (_filterArray != null)
                command1.Parameters.Add("Include", _filterArray);
            else if (_filter.Length > 0)
                command1.Parameters.Add("Filter", _filter);

            command1.Parameters.Add("Recurse");
            command1.Parameters.Add("Name"); //output format
            pipeline.Commands.Add(command1);

            // like: command1 | command2

            Command command2 = new("Out-File");
            command2.Parameters.Add("File", _fullFileNameOut); //output file
            pipeline.Commands.Add(command2);

            //Run
            Collection<PSObject> results = pipeline.Invoke();

            //show result messages
            foreach (PSObject item in results)
                Console.WriteLine("{0}", item);

            if (pipeline.Error.Count > 0)
                foreach (object item in pipeline.Error.ReadToEnd())
                    Console.WriteLine("{0}", item.ToString());

            runspace.Close();
        }

        //2-utilizando comando
        public void PowerShellRunCommand(CollectionOriginType collectionOriginType)
        {
            PrepareVariables(collectionOriginType);

            //Mount Command
            System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create();
            ps.AddCommand("Get-ChildItem");
            ps.AddParameter("LiteralPath", _rootPath);

            if (_filterArray != null)
                ps.AddParameter("Include", _filterArray);
            else if (_filter.Length > 0)
                ps.AddParameter("Filter", _filter);

            ps.AddParameter("-Recurse");
            ps.AddParameter("-Name");
            ps.AddParameter("-File");

            System.Collections.ObjectModel.Collection<PSObject> results = ps.Invoke();

            if (ps.Streams.Error.Count > 0)
            {
                Console.WriteLine("{0} errors", ps.Streams.Error.Count);
            }

            using StreamWriter file = new(_fullFileNameOut);
            {
                foreach (PSObject item in results)
                {
                    file.WriteLine(item.ToString());
                    Console.WriteLine(item);
                }
                file.Close();
            }

            //saber como passar results para input de "Out-File"  like "dir>aa.txt"
            //ps.Commands.Clear();
            //ps.AddCommand("Out-File");
            //ps.AddParameter("File", @"d:\mp3_bbbb.txt");
            //results = ps.Invoke();
        }

        //3-Utilizando script string
        public void PowerShellRunScriptString(CollectionOriginType collectionOriginType)
        {
            PrepareVariables(collectionOriginType);

            //mount
            //"Get-ChildItem -LiteralPath 'C:\\Test' -Recurse -Name -File | Out-File 'c:\result.txt'";
            //"Get-ChildItem -LiteralPath 'C:\\Test' -Filter '*.jpg' -Recurse -Name -File | Out-File 'c:\result.txt'";
            //"Get-ChildItem -LiteralPath 'C:\\Test' -Include '*.jpg,*.mp3' -Recurse -Name -File | Out-File 'c:\result.txt'";
            string script = $"Get-ChildItem -LiteralPath '{_rootPath}' {_filterParameter} -Recurse -Name -File | Out-File '{_fullFileNameOut}'";

            ///

            PowerShell ps = PowerShell.Create();
            ps.AddScript(script);

            ///

            System.Collections.ObjectModel.Collection<PSObject> results = ps.Invoke();
            foreach (var result in results)
                Console.WriteLine(result);
        }


        private void PrepareVariables(CollectionOriginType collectionOriginType)
        {
            if (collectionOriginType == CollectionOriginType.Loss)
            {
                _rootPath = Constants.FolderRootCollectionLoss;
                _filter = FilterVerify(Constants.FilesFilterLoss);
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.FileTextNameCollectionLoss);
            }
            else
            {
                _rootPath = Constants.FolderRootCollectionLossLess;
                _filter = FilterVerify(Constants.FilesFilterLossLess);
                _fullFileNameOut = System.IO.Path.Join(_rootPath, Constants.FileTextNameCollectionLossLess);
            }

            bool isFilterArray = _filter.Contains(',');
            bool hasFilter = _filter.Trim().Length > 0;

            _filterParameter = "";
            _filterArray = null;
            if (isFilterArray)
            {
                _filterArray = _filter.Split(",");
                _filterParameter = $"-Include ({_filter})";
            }
            else if (hasFilter)
                _filterParameter = $"-Filter {_filter}";

        }

        //Input : "*.MP3, *.WMA"
        //Output: "'*.MP3','*.WMA'
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
                res += "'" + tmp + "',";
            }
            res = res.TrimEnd(',');

            return res;
        }

        ////Passa uma string com script powershell -
        ////SEM filtro
        //public void PowerShellRunScriptAllFiles(CollectionOriginType collectionOriginType)
        //{
        //    string script;

        //    if (collectionOriginType == CollectionOriginType.Loss)
        //        script = $"Get-ChildItem -LiteralPath '{Constants.FolderRootCollectionLoss}' -Recurse -Name -File | Out-File '{Constants.FileTextNameCollectionLoss}'";
        //    else

        //        script = $"Get-ChildItem -LiteralPath '{Constants.FolderRootCollectionLoss}' -Recurse -Name -File | Out-File '{Constants.FileTextNameCollectionLossLess}'";

        //    PowerShell ps = PowerShell.Create();
        //    ps.AddScript(script);


        //    System.Collections.ObjectModel.Collection<PSObject> results = ps.Invoke();
        //    foreach (var result in results)
        //        Console.WriteLine(result);
        //}
        ////"Get-ChildItem -Path '\\\\NAS-QNAP\\music\\_COLLECTION\\' -Recurse -Name -File | Out-File 'All-Files-Music_Loss.txt'"

        ////Passa uma string com um script powershell -
        ////INPUT collection e filtro
        //public void PowerShellRunScriptCollectioWithFilter(CollectionOriginType collectionOriginType, string filter)
        //{
        //    //Get-ChildItem .\* -Include "MyProject.Data*.dll", "EntityFramework*.dll"
        //    //-Include *.xml,*.json
        //    //dir.\* -include('*.xsl', '*.xslt') - recurse
        //    //-Filter is more fast than -Include !!!

        //    string filterType = "-Filter";
        //    if (filter.Contains(","))
        //        filterType = "-Include";

        //    string rootPath;
        //    string output;
        //    string script;

        //    if (collectionOriginType == CollectionOriginType.Loss)
        //    {
        //        rootPath = @"\\NAS-QNAP\music\_COLLECTION";
        //        output = "Tree-Files-music_mp3.txt";
        //    }
        //    else
        //    {
        //        rootPath = @"\\NAS-QNAP\music_lossless\_COLLECTION";
        //        output = "Tree-Files-music_lossless.txt";
        //    }

        //    script = $"Get-ChildItem -LiteralPath '{rootPath}' {filterType} '{filter}' -Recurse -Name -File | Out-File '{output}'";

        //    PowerShell ps = PowerShell.Create();
        //    ps.AddScript(script);

        //    System.Collections.ObjectModel.Collection<PSObject> results = ps.Invoke();
        //    foreach (var result in results)
        //        Console.WriteLine(result);
        //}

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