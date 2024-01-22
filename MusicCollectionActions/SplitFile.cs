using MusicCollectionContext;
using Serilog;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace MusicCollectionSystemIO
{
    ///////////
    // REVER //
    ///////////

    //Escreve 3 ficheiros
    //"1 - Artists_(FLAC/MP3).txt"
    //  directorios do tipo Artist {country}

    //"2 - Albuns_(FLAC/MP3.txt"
    //  directorios do tipo Artist {year} [album] @

    //"3 - Tracks_(FLAC/MP3).txt";
    //  ficheiros com extensão MP3/

    public class SystemIOShellHelper2
    {
        private StreamReader _fileInput;
        private StreamWriter _fileOutputArtists;
        private StreamWriter _fileOutputAlbums;
        private StreamWriter _fileOutputTracks;
        private StreamWriter _fileOutputErrors;
        private string _artistFolder;
        private string _albumFolder;

        private string _artist;
        private string _country;
        private string _albumType;
        private string _year;
        private string _album;
        private readonly string _audio;
        private int _count = 0;

        /// <summary>
        /// get folder tree and analyse if folder name has "{}, [], @" tags
        /// output to tree text files, 'artists', 'albuns' and 'tracks'
        /// </summary>
        /// <param name="collection"></param>
        public void TreeProcess3outputs(CollectionOriginType collectionOriginType)
        {
            Log.Information("'MusicCollectionMsDos.TreeProcess' - Started...");

            string rootPath;
            string fileNameInput;
            string fileNameArtists;
            string fileNameAlbuns;
            string fileNameTracks;
            string fileErrors;

            try
            {
                if (collectionOriginType == CollectionOriginType.Loss)
                {
                    rootPath = Constants.FolderRootCollectionLoss;
                    fileNameInput = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLoss);
                    fileNameArtists = System.IO.Path.Join(rootPath, Constants.FileNameArtistsLoss);
                    fileNameAlbuns = System.IO.Path.Join(rootPath, Constants.FileNameAlbunsLoss);
                    fileNameTracks = System.IO.Path.Join(rootPath, Constants.fileNameTracksLoss);
                    fileErrors = System.IO.Path.Join(rootPath, Constants.fileNameTracksLoss);
                }
                else
                {
                    rootPath = Constants.FolderRootCollectionLossLess;
                    fileNameInput = System.IO.Path.Join(rootPath, Constants.TreeTextFileNameCollectionLossLess);
                    fileNameArtists = System.IO.Path.Join(rootPath, Constants.FileNameArtistsLossLess);
                    fileNameAlbuns = System.IO.Path.Join(rootPath, Constants.FileNameAlbunsLossLess);
                    fileNameTracks = System.IO.Path.Join(rootPath, Constants.fileNameTracksLossLess);
                    fileErrors = System.IO.Path.Join(rootPath, Constants.fileNameTracksLossLess);
                }


                if (!File.Exists(fileNameInput))
                    return;

                _fileInput = new StreamReader(fileNameInput, Constants.StreamsEncoding);
                _fileOutputArtists = new StreamWriter(fileNameArtists, false, Constants.StreamsEncoding);
                _fileOutputArtists = new StreamWriter(fileNameArtists, false, Constants.StreamsEncoding);
                _fileOutputAlbums = new StreamWriter(fileNameAlbuns, false, Constants.StreamsEncoding);
                _fileOutputTracks = new StreamWriter(fileNameTracks, false, Constants.StreamsEncoding);
                _fileOutputErrors = new StreamWriter(fileErrors, false, Constants.StreamsEncoding);

                string line;
                while ((line = _fileInput.ReadLine()) != null)
                {

                }
            }
            catch (Exception ex)
            {
                //Log.Error($"Command:{msDosCommand}");
                //Log.Error($"Outout:{fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                //if (_streamWriter != null)
                //{
                //    _streamWriter.Flush();
                //    _streamWriter.Close();
                //    _streamWriter.Dispose();
                //}
            }

            Log.Information("'MusicCollectionMsDos.TreeProcess' - Finished...");

        }

        public void ToArtistAlbumsTracks(CollectionOriginType collectionOriginType)
        {
            string rootFolder;
            string fileNameArtists;
            string fileNameAlbuns;
            string fileNameTracks;
            string fileErrors;

            if (collectionOriginType == CollectionOriginType.Loss)
            {
                rootFolder = Constants.FolderRootCollectionLoss;
                fileNameArtists = Constants.FileNameArtistsLoss;
                fileNameAlbuns = Constants.FileNameAlbunsLoss;
                fileNameTracks = Constants.fileNameTracksLoss;
            }
            else
            {
                rootFolder = Constants.FolderRootCollectionLossLess;
                fileNameArtists = Constants.FileNameArtistsLossLess;
                fileNameAlbuns = Constants.FileNameAlbunsLossLess;
                fileNameTracks = Constants.fileNameTracksLossLess;
            }
            fileErrors = Constants.fileNameTracksLoss;

            try
            {
        _fileOutputArtists = new StreamWriter(fileNameArtists, false, Constants.StreamsEncoding);
        _fileOutputAlbums = new StreamWriter(fileNameAlbuns, false, Constants.StreamsEncoding);
        _fileOutputTracks = new StreamWriter(fileNameTracks, false, Constants.StreamsEncoding);
        _fileOutputErrors = new StreamWriter(fileErrors, false, Constants.StreamsEncoding);

                LoadSubDirectories(rootFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EXCEPTION:" + ex.Message);
            }
            finally
            {
                _fileOutputTracks.Close();
                _fileOutputAlbums.Close();
                _fileOutputArtists.Close();
            }
        }
        private void LoadSubDirectories(string rootFolder)
        {
            // Get all subdirectories  
            string[] subdirectoryEntries = Directory.GetDirectories(rootFolder);

            foreach (string subdirectory in subdirectoryEntries)
            {
                //DirectoryInfo directoryInfo = new(subdirectory);

                LoadSubDirectories(subdirectory);
                LoadFiles(subdirectory);
            }
        }

        private void LoadFiles(string directory)
        {
            //MP3, AAC, OGG WMA 
            string[] fileArray = Directory.GetFiles(directory, "*.MP3");
            if (fileArray.Length == 0)
                return;

            string disc = "";
            if (_albumFolder.Length < directory.Length)
                disc = directory.Replace(_albumFolder, "");


            // Loop through them to see files  
            foreach (string file in fileArray)
            {
                FileInfo fi = new(file);
                //if (fi.Name == "01-Stormsplinter.mp3") //for make a breakpoint
                //    count++;

                string trackName = Path.GetFileNameWithoutExtension(fi.Name);
                string ext = fi.Extension.ToLower();

                FileTracksWrite(_artist, _country, _albumType, _year, _album, disc, trackName, _audio, ext, fi.FullName);

                _count++;
                Console.WriteLine(_count);
            }
        }

        private void FileErrorsWrite(string fullName, string info)
        {
            _fileOutputErrors.WriteLine(fullName + ";" + info);
            _fileOutputErrors.Flush();
        }

        private void FileArtistsWrite(string artist, string country, string fullName)
        {
            _fileOutputArtists.WriteLine(artist + ";" + country + ";" + fullName);
            _fileOutputArtists.Flush();
        }

        private void FileAlbunsWrite(string artist, string country, string albumType, string year, string album, string audio, string fullName)
        {
            _fileOutputAlbums.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + audio + ";" + fullName);
            _fileOutputAlbums.Flush();
        }

        private void FileTracksWrite(string artist, string country, string albumType, string year, string album, string disc, string trackName, string audio, string extension, string fullName)
        {
            _fileOutputTracks.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + disc + ";" + trackName + ";" + audio + ";" + extension + ";" + fullName);
            _fileOutputTracks.Flush();
        }


        private int CountCharOccurance(string text, char chr)
        {
            int count = 0;
            foreach (char c in text)
                if (c == chr)
                    count++;
            return count;
        }
    }
}


//public void TreeProcess3(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter)
//{
//    string rootFolder;
//    string fileNameArtists;
//    string fileNameAlbuns;
//    string fileNameTracks;
//    string fileErrors;

//    if (collectionOriginType == CollectionOriginType.Loss)
//    {
//        rootFolder = Constants.FolderRootCollectionLoss;
//        fileNameArtists = Constants.FileNameArtistsLoss;
//        fileNameAlbuns = Constants.FileNameAlbunsLoss;
//        fileNameTracks = Constants.fileNameTracksLoss;
//    }
//    else
//    {
//        rootFolder = Constants.FolderRootCollectionLossLess;
//        fileNameArtists = Constants.FileNameArtistsLossLess;
//        fileNameAlbuns = Constants.FileNameAlbunsLossLess;
//        fileNameTracks = Constants.fileNameTracksLossLess;
//    }
//    fileErrors = Constants.fileNameTracksLoss;

//    try
//    {
//        _fileArtists = new StreamWriter(fileNameArtists, false, Constants.StreamsEncoding);
//        _fileAlbuns = new StreamWriter(fileNameAlbuns, false, Constants.StreamsEncoding);
//        _fileTracks = new StreamWriter(fileNameTracks, false, Constants.StreamsEncoding);
//        _fileErrors = new StreamWriter(fileErrors, false, Constants.StreamsEncoding);

//        LoadSubDirectories(rootFolder);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine("ERROR EXCEPTION:" + ex.Message);
//    }
//    finally
//    {
//        _fileTracks.Close();
//        _fileAlbuns.Close();
//        _fileArtists.Close();
//    }
//}

//private void LoadSubDirectories(string rootFolder)
//{
//    // Get all directories  
//    string[] directoriesEntry = Directory.GetDirectories(rootFolder);

//    if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.DirectoriesOnly))
//    {
//        foreach (string directory in directoriesEntry)
//        {
//            _streamWriter.WriteLine(directory);
//            _streamWriter.Flush();
//        }
//    }

//    if ((_contextFilter == FileSystemContextFilter.All) || (_contextFilter == FileSystemContextFilter.FilesOnly))
//    {
//        // Get all files
//        string[] filesEntries = Directory.GetFiles(rootFolder);
//        foreach (string file in filesEntries)
//        {
//            _streamWriter.WriteLine(file);
//            _streamWriter.Flush();
//        }
//    }

//    //next tree
//    foreach (string directory in directoriesEntry)
//        LoadSubDirectories(directory);
//}