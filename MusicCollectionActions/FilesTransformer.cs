
using System;
using System.IO;
using MusicCollectionContext;
using Serilog;

namespace MusicCollectionActions
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    public class FilesTransformer
    {
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;
        private int _count = 0;

        /// <summary>
        /// transform text file to csv file and add prefix 'absolute fullFolder' to line and columb extension
        /// columns separated by 'fieldSeparator' char
        /// 
        /// input  - text file with result of "PowerShellHelper" class
        /// output - csv file with format: fullPathFile;extention
        /// </summary>
        /// <param name="collectionOriginType"></param>
        /// 
        public void FlatToCSV(CollectionOriginType collectionOriginType, bool onlyMusicFiles, bool addExtensionColumn)
        {
            Log.Information("FlatToCSV started...");

            string rootFolder;
            string fullFileNameIn;
            string fullFileNameOut;

            //output files
            switch (collectionOriginType)
            {
                case CollectionOriginType.Lossless:
                    rootFolder = Constants.FolderRootCollectionLossLess;
                    fullFileNameIn = Path.Join(rootFolder, Constants.TreeTextFileNameCollectionLossLess);
                    fullFileNameOut = Path.Join(rootFolder, Constants.TreeCsvFileNameCollectionLossLess);
                    break;
                case CollectionOriginType.Loss:
                    rootFolder = Constants.FolderRootCollectionLoss;
                    fullFileNameIn = Path.Join(rootFolder, Constants.TreeTextFileNameCollectionLoss);
                    fullFileNameOut = Path.Join(rootFolder, Constants.TreeCsvFileNameCollectionLoss);
                    break;
                default:
                    throw new Exception("CollectionOriginType error in 'FilesTransformer.FlatToCSV')");
            }

            Log.Information($"rootFolder={rootFolder}");
            Log.Information($"fullFileNameIn={fullFileNameIn}");
            Log.Information($"fullFileNameOut={fullFileNameOut}");

            try
            {
                if (!File.Exists(fullFileNameIn))
                    throw new Exception($"File not found: '{fullFileNameIn}'");

                _streamReader = new StreamReader(fullFileNameIn, Constants.StreamsEncoding);
                _streamWriter = new StreamWriter(fullFileNameOut, false, Constants.StreamsEncoding);

                string? line;
                bool isValidFile = true;
                while ((line = _streamReader.ReadLine()) != null)
                {
                        //add extinson column
                        string extension = Path.GetExtension(line).Trim().ToUpper();

                        if (extension.Length > 0)
                            extension = extension.Substring(1);

                    if (onlyMusicFiles)
                        isValidFile = Enum.IsDefined(typeof(MusicFileExtension), extension);

                    if (isValidFile)
                    {
                        string newLine;

                        if (addExtensionColumn)
                            newLine = $"{rootFolder}{line}{Constants.FieldSeparator}{extension}";
                        else
                            newLine = $"{rootFolder}{line}";

                        _streamWriter.WriteLine(newLine);
                        _streamWriter.Flush();
                        _count++;
                    }
                }

                Log.Information($"FlatToCSV Count={_count}");
            }

            catch (Exception exp)
            {
                Log.Error($"FlatToCSV Error={exp.Message}");
            }
            finally
            {
                _streamReader?.Close();
                _streamWriter?.Close();
            }

            Log.Information("FlatToCSV Finished...");
        }
    }
}
