using MusicCollectionContext;
using Serilog;
using System;
using System.IO;


namespace MusicCollectionValidators
{
    public class ValidateCollectionAction
    {
        string _rootPath;
        string _fileNameIn;
        string _fileNameError;
        StreamWriter _writer;
        StreamReader _reader;
        CollectionFoldersValidator _validator;

        //input line: example
        //\\NAS-QNAP\music\_COLLECTION\A\
        //\\NAS-QNAP\music\_COLLECTION\I\I And Thou {United States}\
        //\\NAS-QNAP\music\_COLLECTION\I\I And Thou {United States}\I And Thou {2912} [Speak] @MP3\
        public void ValidateFoldersRulesFromLinearFormatedFile(CollectionOriginType collectionOriginType)
        {
            try
            {
                PrepareVariables(collectionOriginType);

                Log.Information(_fileNameIn);
                Log.Information(_fileNameError);

                _validator = new CollectionFoldersValidator(collectionOriginType);

                _writer = new StreamWriter(_fileNameError, false, Constants.StreamsEncoding);
                _reader = new StreamReader(_fileNameIn, Constants.StreamsEncoding);

                int count = 0;

                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    count++;

                    ValidateLine(line);
                }

                Log.Information(count.ToString());

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            finally
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader.Dispose();
                }
                if (_writer != null)
                {
                    _writer.Flush();
                    _writer.Close();
                    _writer.Dispose();
                }
            }
        }

        private void PrepareVariables(CollectionOriginType collectionOriginType)
        {
            if (collectionOriginType == CollectionOriginType.Loss)
            {
                _rootPath = Constants.FolderRootCollectionLoss;
                _fileNameIn = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLoss);
                _fileNameError = System.IO.Path.Join(_rootPath, Constants.FileErrorsLoss);
            }
            else
            {
                _rootPath = Constants.FolderRootCollectionLossLess;
                _fileNameIn = System.IO.Path.Join(_rootPath, Constants.TreeTextFileNameCollectionLossLess);
                _fileNameError = System.IO.Path.Join(_rootPath, Constants.FileErrorsLossLess);
            }

            if (!File.Exists(_fileNameIn))
                throw new Exception($"[{_fileNameIn}] not exists.");
        }

        private void ValidateLine(string line)
        {
            string temp = line.Replace(_rootPath, "");

            string[] words = temp.Split(Path.DirectorySeparatorChar);

            foreach (string item in words)
            {
                CollectionFoldersValidatorResult result = _validator.ValidateFolder(item);

                if (result.CollectionFolderType == CollectionFolderType.None)
                    continue;

                if (result.CollectionFolderType == CollectionFolderType.Ok)
                    continue;

                if (result.CollectionFolderType == CollectionFolderType.MaybeArtistOrAlbum)
                    _writer.WriteLine($"{line};{result.CollectionFolderType};{result.Info}");

                if (result.CollectionFolderType == CollectionFolderType.IncorrectFormat)
                    _writer.WriteLine($"{line};{result.CollectionFolderType};{result.Info}");

                if (result.CollectionFolderType == CollectionFolderType.Error)
                    _writer.WriteLine($"{line};{result.CollectionFolderType};{result.Info}");
            }
        }

    }
}

