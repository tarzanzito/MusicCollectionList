using MusicCollectionContext;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace MusicCollectionSystemIO
{
    public class SystemIOShellHelper
    {
        private StreamWriter? _streamWriter;
        private FileSystemContextFilter _contextFilter;
        private string _extensionFilter = string.Empty;
        private bool _applyExtensionsFilter;

        /// <summary>
        /// generate text file with system tree with files and folders.
        /// 
        /// line output format:
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\                             "folder"
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3   "file"
        /// 
        /// Note: This method add '/' or '\' at end if an folder.
        ///       
        /// </summary>
        /// <param name="collectionOriginType"></param>
        /// <param name="contextFilter"></param>
        /// <param name="applyExtensionsFilter"></param>
        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter)
        {
            _contextFilter = contextFilter;
            _applyExtensionsFilter = applyExtensionsFilter;

            Log.Information("'MusicCollectionSystemIO.TreeProcess' Started...");

            Stopwatch stopwatch = Utils.GetNewStopwatch();
            Utils.Startwatch(stopwatch, "SystemIOShellHelper", "TreeProcess");

            try
            {
                string rootPath;
                string fullFileNameOut;

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLoss;
                }
                else
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLossLess;
                }

                _extensionFilter = _extensionFilter.Replace("*", "").Replace(" ", "").ToUpper().Trim();

                if (applyExtensionsFilter)
                    _applyExtensionsFilter = _extensionFilter.Length > 0;

                if (!Directory.Exists(rootPath))
                {
                    Log.Error($"Folder Root not exists=[{rootPath}");
                    return;
                }

                Log.Information($"Root Path:[{rootPath}]");
                Log.Information($"Output File:[{fullFileNameOut}]");
                Log.Information($"Context Filter:[{contextFilter}]");
                Log.Information($"Apply Extensions Filter:[{applyExtensionsFilter}]");
                Log.Information($"Extensions Filter:[{_extensionFilter}]");

                _streamWriter = new StreamWriter(fullFileNameOut, false, Constants.StreamsEncoding);

                LoadDirectoryInfo(rootPath);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR EXCEPTION: {ex.Message}");
            }
            finally
            {
                if (_streamWriter != null)
                {
                    _streamWriter.Flush();
                    _streamWriter.Close();
                }

                Utils.Stopwatch(stopwatch, "SystemIOShellHelper", "TreeProcess");

                Log.Information("'MusicCollectionSystemIO.TreeProcess' Finished...");
            }
        }

        private void LoadDirectoryInfo(string directory)
        {
            if (_streamWriter == null)
                throw new Exception("_streamWriter is null");

            // Get all directories  
            string[] directoriesEntry = Directory.GetDirectories(directory);

            //process directories
            if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.DirectoriesOnly))
            {
                foreach (string item in directoriesEntry)
                {
                    //Mark Directories with DirectorySeparatorChar at end
                    _streamWriter.WriteLine($"{item}{Path.DirectorySeparatorChar}");
                    _streamWriter.Flush();
                }
            }

            //process files
            if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.FilesOnly))
            {
                // Get all files
                string[] filesEntries = Directory.GetFiles(directory);

                bool isValid = true;
                
                foreach (string item in filesEntries)
                {
                    if (_applyExtensionsFilter)
                    {
                        string extension = Path.GetExtension(item).ToUpper().Trim();
                        isValid = _extensionFilter.Contains(extension);
                    }

                    if (isValid)
                    {
                        _streamWriter.WriteLine(item);
                        _streamWriter.Flush();
                    }
                }
            }

            //process next tree directories
            foreach (string dir in directoriesEntry)
                LoadDirectoryInfo(dir);
        }
    }
}
