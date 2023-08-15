
using System;
using System.Text;
using System.IO;

namespace MusicCollectionList
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    public class FilesTransformer
    {
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
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
        public void FlatToCSV(CollectionOriginType collectionOriginType, bool onlyMusicFiles)
        {
            string rootFolder;
            string fullFileNameIn;
            string fullFileNameOut;

            if (collectionOriginType == CollectionOriginType.Loss)
            {
                rootFolder = Constants.FolderRootCollectionLoss;
                fullFileNameIn = Path.Join(rootFolder, Constants.TreeTextFileNameCollectionLoss); 
                fullFileNameOut = Path.Join(rootFolder, Constants.TreeCsvFileNameCollectionLoss);
            }
            else
            {
                rootFolder = Constants.FolderRootCollectionLossLess;
                fullFileNameIn = Path.Join(rootFolder, Constants.TreeTextFileNameCollectionLossLess);
                fullFileNameOut = Path.Join(rootFolder, Constants.TreeCsvFileNameCollectionLossLess);
            }

            try
            {
                if (!File.Exists(fullFileNameIn))
                    throw new Exception($"File not found: '{fullFileNameIn}'");

                _streamReader = new StreamReader(fullFileNameIn, Encoding.UTF8);
                _streamWriter = new StreamWriter(fullFileNameOut, false, Encoding.UTF8);

                string line;
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
                        
                        _streamWriter.WriteLine(rootFolder + line + Constants.FieldSeparator + extension);
                        _streamWriter.Flush();
                        _count++;
                    }
                }

                Console.WriteLine(_count);
            }

            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                _streamReader.Close();
                _streamWriter.Close();
            }

        }
    }
}
