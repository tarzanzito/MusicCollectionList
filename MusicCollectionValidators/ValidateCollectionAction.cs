using MusicCollectionContext;
using Serilog;
using System;
using System.IO;


namespace MusicCollectionValidators
{
    public class ValidateCollectionAction
    {
        string _rootPath = string.Empty;
        string _fileNameIn = string.Empty;
        string _fileNameError = string.Empty;
        StreamWriter? _writer;
        StreamReader? _reader;
        CollectionFoldersValidator? _validator;

        //input line: example
        //\\NAS-QNAP\music\_COLLECTION\A\
        //\\NAS-QNAP\music\_COLLECTION\I\I And Thou {United States}\
        //\\NAS-QNAP\music\_COLLECTION\I\I And Thou {United States}\I And Thou {2912} [Speak] @MP3\
        public void ValidateFoldersRulesFromLinearFormatedFile(CollectionOriginType collectionOriginType)
        {
            Log.Information("ValidateFoldersRulesFromLinearFormatedFile: Started...");

            try
            {
                PrepareVariables(collectionOriginType);

                Log.Information($"fileNameIn={_fileNameIn}");
                Log.Information($"fileNameError={_fileNameError}");

                _validator = new CollectionFoldersValidator(collectionOriginType);

                _writer = new StreamWriter(_fileNameError, false, Constants.StreamsEncoding);
                _reader = new StreamReader(_fileNameIn, Constants.StreamsEncoding);

                int count = 0;

                string? line;
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
                _reader?.Close();

                _writer?.Flush();
                _writer?.Close();
            }

            Log.Information("ValidateFoldersRulesFromLinearFormatedFile: Finished...");
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
            if (_writer == null)
                throw new Exception("_writer is null.");

            if (_validator == null)
                throw new Exception("_validator is null.");


            string temp = line.Replace(_rootPath, "");

            string[] words = temp.Split(Path.DirectorySeparatorChar);

            string letter = words[0];

            foreach (string item in words)
            {
                CollectionFoldersValidatorResult result = _validator!.ValidateFolder(item, letter);

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

