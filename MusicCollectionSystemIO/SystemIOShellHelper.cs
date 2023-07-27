using MusicCollectionContext;
using Serilog;
using System;
using System.IO;
using System.Text;

namespace MusicCollectionSystemIO
{
    public class SystemIOShellHelper
    {
        private StreamWriter _streamWriter;
        private FileSystemContextFilter _contextFilter;
        private string _extensionFilter;
        private bool _applyExtensionsFilter;


        /// <summary>
        /// generate file with tree of files and folders: typical output like command "DIR /S /B"
        /// /B = linear format - AbsoluteFullPathName\FileName
        /// lines example: 
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3
        /// C:\_COLLECTION\C\Camel {United Kingdom}\Studio\Camel {1973} [Camel] @MP3\01. Slow Yourself Down.mp3
        /// </summary>
        /// <param name="collectionOriginType"></param>
        /// <param name="contextFilter"></param>
        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter, bool applyExtensionsFilter)
        {
            _contextFilter = contextFilter;
            _applyExtensionsFilter = applyExtensionsFilter;

            try
            {
                string rootPath;
                string fullFileNameOut;

                Log.Information("TreeProcess Started...");

                //output files
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLoss);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLoss.Trim().Replace("*", "").ToUpper();
                }
                else
                {
                    rootPath = Utils.AppendDirectorySeparator(Constants.FolderRootCollectionLossLess);
                    fullFileNameOut = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    if (applyExtensionsFilter)
                        _extensionFilter = Constants.FileExtensionsFilterLossLess.Trim().Replace("*", "").ToUpper();
                }

                Log.Information($"Output File:[{fullFileNameOut}]");
                Log.Information($"Context Filter:[{contextFilter.ToString()}]");
                Log.Information($"Apply Extensions Filter:[{applyExtensionsFilter.ToString()}]");
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
                    _streamWriter.Dispose();
                }

                Log.Information("TreeProcess Finished...");
            }
        }

        private void LoadDirectoryInfo(string directory)
        {
            // Get all directories  
            string[] directoriesEntry = Directory.GetDirectories(directory);

            if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.DirectoriesOnly))
            {
                foreach (string dir in directoriesEntry)
                {
                    //Mark Directories with DirectorySeparatorChar at end
                    _streamWriter.WriteLine($"{dir}{Path.DirectorySeparatorChar}");
                    _streamWriter.Flush();
                }
            }

            if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.FilesOnly))
            {
                // Get all files
                string[] filesEntries = Directory.GetFiles(directory);

                bool isValid = true;
                
                foreach (string file in filesEntries)
                {
                    if (_applyExtensionsFilter)
                    {
                        string extension = Path.GetExtension(file).ToUpper().Trim();
                        isValid = _extensionFilter.Contains(extension);
                    }

                    if (isValid)
                    {
                        _streamWriter.WriteLine(file);
                        _streamWriter.Flush();
                    }
                }
            }

            //next tree
            foreach (string dir in directoriesEntry)
                LoadDirectoryInfo(dir);
        }

    }
}
