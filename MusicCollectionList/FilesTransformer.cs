
using System;
using System.Text;
using System.IO;

namespace MusicCollection
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    internal class FilesTransformer
    {
        private StreamReader fileIn;
        private StreamWriter fileOut;
        private int count = 0;

        /// <summary>
        /// File transform text to text -> path and add extention 
        /// input -text file with result of "Dir *.* /s>ll-Files-music.txt
        /// output - text file with format: fullPathFile;extention
        /// </summary>
        /// <param name="Collection"></param>
        public void TextxToCSV(CollectionOriginType collectionOriginType)
        {
            string fileNameIn;
            string fileNameOut;
            string prefix;

            if (collectionOriginType == CollectionOriginType.Loss)
            {
                prefix = Constants.FolderRootCollectionLoss;
                fileNameIn = Constants.FileTextNameCollectionLoss;
                fileNameOut = Constants.FileCsvNameCollectionLoss;
            }
            else
            {
                prefix = Constants.FolderRootCollectionLossLess;
                fileNameIn = Constants.FileTextNameCollectionLossLess;
                fileNameOut = Constants.FileCsvNameCollectionLossLess;
            }

            try
            {
                string fullFileNameIn = Path.Join(prefix, fileNameIn);
                string fullFileNameOut = Path.Join(prefix, fileNameOut);

                if (!File.Exists(fullFileNameIn))
                    throw new Exception($"File not found: '{fullFileNameIn}'");

                fileIn = new StreamReader(fullFileNameIn, Encoding.UTF8);
                fileOut = new StreamWriter(fullFileNameOut, false, Encoding.UTF8);

                string line;

                while ((line = fileIn.ReadLine()) != null)
                {
                    string extension = Path.GetExtension(line).Trim();

                    fileOut.WriteLine(prefix + line + ";"  + extension);
                    fileOut.Flush();

                    count++;
                    Console.WriteLine(count);
                }
            }

            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            finally
            {
                fileIn.Close();
                fileOut.Close();
            }

        }

    }
}
