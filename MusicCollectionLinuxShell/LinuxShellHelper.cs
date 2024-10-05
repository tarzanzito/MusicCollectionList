using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace MusicCollectionLinux
{         
    //ls -l -h -a -p -R Path
    //
    // -l  -- list with long format - show permissions
    // -h  -- list long format with readable file size
    // -a  -- list all files including hidden file starting with '.'
    // -p  -- indicator-style=slash - append '/' indicator to directories
    // -R  -- list recursively directory tree

    public class LinuxShellHelper
    {
        private StreamWriter? _streamWriter;

        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter, bool setToLinearOutputFormat = true)
        {
            string rootPath = string.Empty;
            string fullFileNameOut = string.Empty;
            string fullFileNameTemp = string.Empty;
            string extensionFilter = string.Empty;

            Log.Information("'LinuxShellHelper.TreeProcess' - Started...");

            try
            {
                //_contextFilter = contextFilter;
                //_applyExtensionsFilter = applyExtensionsFilter;

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLoss);
                    extensionFilter = Constants.FileExtensionsFilterLoss;
                }
                else
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    fullFileNameTemp = System.IO.Path.Join(rootPath, Constants.TreeTempFileNameCollectionLossLess);
                    extensionFilter = Constants.FileExtensionsFilterLossLess;
                }

                extensionFilter = extensionFilter.Replace("*", "").Replace(" ", "").ToUpper().Trim();

                if (setToLinearOutputFormat)
                    fullFileNameTemp = fullFileNameOut;

                if (!Directory.Exists(rootPath))
                {
                    Log.Error($"Foler Root not exists=[{rootPath}");
                    return;
                }

                Log.Information($"Output=[{fullFileNameOut}");

                string bashCommand = $"ls -l -h -a -p -R {rootPath}";
                //-p -- put char '/' at the end of each folder, but this option seems not to be necessary

                //process 
                bool resultOk = LinuxBashProcess(bashCommand, fullFileNameTemp);

                if (resultOk && (setToLinearOutputFormat))
                    ChangeOutputToLinearFormat(fullFileNameTemp, fullFileNameOut, rootPath);
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

        private bool LinuxBashProcess(string bashCommand, string fullFileNameOut)
        {
            Log.Information("'LinuxShellHelper.LinuxBashProcess' - Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "LinuxShellHelper", "LinuxBashProcess");

            bool retValue = true;

            try
            {
                //output                         
                _streamWriter = new StreamWriter(fullFileNameOut, false, Constants.StreamsEncoding);

                //Process Info
                var startInfo = new ProcessStartInfo();
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
                    //_streamWriter.Flush();
                    _streamWriter.Close();
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
        private void ChangeOutputToLinearFormat(string _fullFileNameTemp, string _fullFileNameOut, string initialPath = "")
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

            StreamReader? streamReader = null;
            StreamWriter? streamWriter = null;

            string? line = null;

            try
            {
                streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder = false;
                bool hasEndFolderChar = false;
                bool useInitialPath = (initialPath.Length > 0);
                string basePath = "";
                string rootPath = "";
                string member = "";
                int memberStartAt = 0;

                //get rootPath without last '/'
                if (useInitialPath)
                {
                    if (initialPath.EndsWith("'/"))
                        rootPath = initialPath.Substring(0, initialPath.Length - 1);
                    else
                        rootPath = initialPath;
                }

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    if (line.StartsWith("total "))
                        continue;

                    if (line.StartsWith("."))
                    {
                        if ((line.Length - 2) == 0)
                            basePath = "/";
                        else
                            basePath = line.Substring(1, line.Length - 2) + "/";
                        continue;
                    }

                    if (memberStartAt == 0) //only to improve performance
                    {
                        LinuxLineInfo lineInfo = GetLineInfo(line);
                        memberStartAt = lineInfo.MemberStartAt; //add to improve
                    }

                    member = line.Substring(memberStartAt, line.Length - memberStartAt);
                    char memberType = Char.Parse(line.Substring(0, 1));

                    if (!ValidateType(memberType))
                        throw new Exception($"Error in member type:{line}");

                    isFolder = memberType == 'd';

                    if (isFolder)
                    {
                        if (member == "." || member == ".."
                                || member == "./" || member == "../") //-p
                            continue;
                        hasEndFolderChar = member.EndsWith("/");
                    }

                    //has cousts if (ValidatePermissions(line) > 0)

                    //write
                    string newLine;

                    if (useInitialPath)
                        newLine = $"{rootPath}{basePath}{member}";
                    else
                        newLine = $"{basePath}{member}";

                    streamWriter.WriteLine(newLine);
                    streamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                if (line != null)
                    Log.Error($"Line:{line}");

                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
                if (streamWriter != null)
                {
                    //_streamWriter.Flush();
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
            }
        }

        private LinuxLineInfo GetLineInfo(string line)
        {
            //-rwxrwxrwx 1 root root         69 Apr  3 23:34 file name.txt

            int count = 0;
            int posI = 0;
            int posE = 0;

            var lineInfo = new LinuxLineInfo();

            while (count < 9)
            {
                posI = NextNonSpace(line, posI);
                posE = NextSpace(line, posI + 1);

                switch (count)
                {
                    case 0:
                        lineInfo.Attributes = line.Substring(posI, posE - posI);
                        break;
                    case 1:
                        lineInfo.LinksNumber = line.Substring(posI, posE - posI);
                        break;
                    case 2:
                        lineInfo.User = line.Substring(posI, posE - posI);
                        break;
                    case 3:
                        lineInfo.Group = line.Substring(posI, posE - posI);
                        break;
                    case 4:
                        lineInfo.Size = line.Substring(posI, posE - posI);
                        break;
                    case 5: // for datetime remove case 6 and 7 passing to 6
                        lineInfo.Month = line.Substring(posI, posE - posI);
                        break;
                    case 6:
                        lineInfo.Day = line.Substring(posI, posE - posI);
                        break;
                    case 7:
                        lineInfo.Hour = line.Substring(posI, posE - posI);
                        break;
                    case 8:
                        //posI++;
                        lineInfo.Member = line.Substring(posI, line.Length - posI);
                        lineInfo.MemberStartAt = posI;
                        break;
                    default:
                        throw new Exception("ErrorEventArgs in Process line");
                }

                count++;
                posI = posE;
            };

            //lineInfo.MemberStartAt = posI;

            return lineInfo;
        }

        private int NextNonSpace(string line, int startAt)
        {
            int pos = startAt;
            while ((pos < line.Length) && (line[pos] == ' '))
            {
                pos++;
            }

            return pos;
        }

        private int NextSpace(string line, int startAt)
        {
            int pos = startAt;
            while ((pos < line.Length) && (line[pos] != ' '))
            {
                pos++;
            }

            return pos;
        }

        //Other
        private int ValidatePermissions(string line)
        {
            //drwxrwxrwx

            //type 0
            //read 1, 4, 7
            //writ 2, 5, 8
            //exec 3, 6, 9

            if (line.Length < 10)
                return -1;

            //string attrs = line.Substring(0, 10);
            char[] attrArray = line.ToCharArray(0, 10);

            //type 0
            if (!ValidateType(attrArray[0]))
                return 1;

            //user read 1
            if (!ValidateRead(attrArray[1]))
                return 2;

            //user write 2
            if (!ValidateWrite(attrArray[2]))
                return 3;

            //user execute 3
            if (!ValidateExecute(attrArray[3]))
                return 4;

            //group read 4
            if (!ValidateRead(attrArray[4]))
                return 5;

            //group write 5
            if (ValidateWrite(attrArray[5]))
                return 6;

            //group execute 6
            if (!ValidateExecute(attrArray[6]))
                return 7;

            //all read
            if (ValidateRead(attrArray[7]))
                return 8;

            //all write
            if (ValidateWrite(attrArray[8]))
                return 9;

            //all execute
            if (!ValidateWrite(attrArray[9]))
                return 10;

            return 0;
        }

        //-: Um arquivo regular
        //d: Um diretório
        //l: Um link simbólico
        //c: Um arquivo de dispositivo de caractere
        //b: Um arquivo de dispositivo de bloco
        //s: Uma tomada
        //p: Um pipe nomeado(FIFO)

        private const string attrType = "-lcdsp";
        private const string attrRead = "-r";
        private const string attrWrite = "-w";
        private const string attrExecute = "-x";

        private bool ValidateType(char chr)
        {
            return attrType.Contains(chr);
        }

        private bool ValidateRead(char chr)
        {
            return attrRead.Contains(chr);
        }

        private bool ValidateWrite(char chr)
        {
            return attrWrite.Contains(chr);
        }

        private bool ValidateExecute(char chr)
        {
            return attrExecute.Contains(chr);
        }


        //private void ChangeOutputToLinearFormatOLD_WITH_ERRORS()
        //{
        //    Log.Information("LinuxShellHelper.ChangeOutputToLinearFormat Started");

        //    Stopwatch stopwatch = Utils.GetNewStopwatch();
        //    Utils.Startwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");

        //    //_fullFileNameTemp = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless.txt";
        //    //_fullFileNameOut = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless_new.txt";
        //    //_applyExtensionsFilter = false;

        //    if (!File.Exists(_fullFileNameTemp))
        //    {
        //        Log.Error($"Folder Root not exists=[{_fullFileNameTemp}");
        //        return;
        //    }

        //    _streamReader = null;
        //     _streamWriter = null;

        //    string? line = null;

        //    try
        //    {
        //        _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
        //        _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

        //        bool isFolder;
        //        bool isValid = true;
        //        bool useRootPath = true;
        //        string basePath = "";
        //        string rootPath;
        //        string member = "";

        //        //if (_rootPath.EndsWith('/'))  //forces ls to have the -p option
        //        if (_rootPath.StartsWith('d'))
        //            if (_rootPath.EndsWith('/'))
        //                rootPath = _rootPath.Substring(0, _rootPath.Length - 1);
        //            else
        //                rootPath = _rootPath.Substring(0, _rootPath.Length);
        //        else
        //            rootPath = _rootPath;

        //        while ((line = _streamReader.ReadLine()) != null)
        //        {
        //            if (line.Length == 0)
        //                continue;

        //            if ((line.Length > 5) && (line.StartsWith("total ")))
        //                continue;

        //            if (line == ".:")
        //            {
        //                useRootPath = true;
        //                basePath = "/";
        //                continue;
        //            }

        //            if (line.EndsWith(':') && line.Contains('/'))
        //            {
        //                useRootPath = line.StartsWith('.');

        //                if (useRootPath)
        //                    basePath = line.Substring(1, line.Length - 2);
        //                else
        //                    basePath = line.Substring(0, line.Length - 1);

        //                continue;
        //            }

        //            if (line.Length < 72)
        //                continue;

        //            if (!DateTime.TryParse(line.Substring(56, 10), out DateTime dt))
        //                continue;


        //            //isFolder = line.EndsWith('/');
        //            isFolder = line.StartsWith('d');
        //            isValid = true;

        //            //Verify Context Filter
        //            if (isFolder)
        //            {
        //                if (_contextFilter == FileSystemContextFilter.FilesOnly)
        //                    continue;
        //            }
        //            else
        //            {
        //                if (_contextFilter == FileSystemContextFilter.DirectoriesOnly)
        //                    continue;
        //            }

        //            member = line.Substring(72);

        //            if ((member == "./") || (member == "../"))
        //                continue;

        //            //Apply Extensions Filter
        //            if (!isFolder)
        //            {
        //                //verify Extensions Filter
        //                if (_applyExtensionsFilter)
        //                {
        //                    string extension = Path.GetExtension(member).ToUpper().Trim();
        //                    isValid = _extensionFilter.Contains(extension);
        //                }
        //            }

        //            //write
        //            if (isValid)
        //            {
        //                //_rootPath
        //                string newLine;

        //                newLine = System.IO.Path.Join(_rootPath, basePath, member);
        //                if (useRootPath)

        //                    newLine = $"{rootPath}{basePath}{line}";//{Path.DirectorySeparatorChar}
        //                else
        //                    newLine = $"{basePath}{line}";//{Path.DirectorySeparatorChar}

        //                _streamWriter.WriteLine(newLine); 
        //                _streamWriter.Flush();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (line != null)
        //            Log.Error($"Line:{line}");
        //        Log.Error($"Outout:{_fullFileNameOut}");
        //        Log.Error($"Message Error:{ex.Message}");
        //    }
        //    finally
        //    {
        //        if (_streamReader != null)
        //        {
        //            _streamReader.Close();
        //            _streamReader.Dispose();
        //        }
        //        if (_streamWriter != null)
        //        {
        //            //_streamWriter.Flush();
        //            _streamWriter.Close();
        //            _streamWriter.Dispose();
        //        }
        //    }

        //    Utils.Stopwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");

        //    Log.Information("LinuxShellHelper.ChangeOutputToLinearFormat Finished");
        //}
    }
}

