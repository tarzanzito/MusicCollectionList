﻿using System;
using System.IO;
using System.Text;

namespace MusicCollectionList
{
    //Escreve 3 ficheiros
    //"1 - Artists_(FLAC/MP3).txt"
    //  directorios do tipo Artist {country}

    //"2 - Albuns_(FLAC/MP3.txt"
    //  directorios do tipo Artist {year} [album] @

    //"3 - Tracks_(FLAC/MP3).txt";
    //  ficheiros com extensão MP3/

    public class SystemIOShellHelper
    {
        private StreamWriter _fileArtists;
        private StreamWriter _fileAlbuns;
        private StreamWriter _fileTracks;
        private StreamWriter _fileErrors;
        private string _artistFolder;
        private string _albumFolder;

        private string _artist;
        private string _country;
        private string _albumType;
        private string _year;
        private string _album;
        private string _audio;
        private int _count = 0;

        /// <summary>
        /// get folder tree and analyse if folder name has "{}, [], @" tags
        /// output to tree text files, 'artists', 'albuns' and 'tracks'
        /// </summary>
        /// <param name="collection"></param>
        public void TreeProcess(CollectionOriginType collectionOriginType, FileSystemContextFilter contextFilter)
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
                _fileArtists = new StreamWriter(fileNameArtists, false, Encoding.UTF8);
                _fileAlbuns = new StreamWriter(fileNameAlbuns, false, Encoding.UTF8);
                _fileTracks = new StreamWriter(fileNameTracks, false, Encoding.UTF8);
                _fileErrors = new StreamWriter(fileErrors, false, Encoding.UTF8);

                LoadSubDirectories(rootFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EXCEPTION:" + ex.Message);
            }
            finally
            {
                _fileTracks.Close();
                _fileAlbuns.Close();
                _fileArtists.Close();
            }
        }

        private void LoadSubDirectories(string rootFolder)
        {
            // Get all subdirectories  
            string[] subdirectoryEntries = Directory.GetDirectories(rootFolder);

            foreach (string subdirectory in subdirectoryEntries)
            {

                DirectoryInfo directoryInfo = new(subdirectory);
                string name = directoryInfo.Name;

                ////Rules 1
                //int count1 = CountCharOccurance(name, '{');
                //int count2 = CountCharOccurance(name, '}');
                //int count3 = CountCharOccurance(name, '[');
                //int count4 = CountCharOccurance(name, ']');
                //int count5 = CountCharOccurance(name, '@');

                //if (count1 > 1)
                //{
                //    FileErrorsWrite(directoryInfo.FullName, "{ more than 1");
                //    continue;
                //}
                //if (count2 > 1)
                //{
                //    FileErrorsWrite(directoryInfo.FullName, "} more than 1");
                //    continue;
                //}

                //if (count3 > 1)
                //{
                //    FileErrorsWrite(directoryInfo.FullName, "[ more than 1");
                //    continue;
                //}

                //if (count4 > 1)
                //{
                //    FileErrorsWrite(directoryInfo.FullName, "] more than 1");
                //    continue;
                //}

                //if (count5 > 1)
                //{
                //    FileErrorsWrite(directoryInfo.FullName, "@ more than 1");
                //    continue;
                //}

                ////Rules2
                //int pos1 = name.IndexOf("{");
                //int pos2 = name.IndexOf("}");
                //int pos3 = name.IndexOf("[");
                //int pos4 = name.IndexOf("]");
                //int pos5 = name.IndexOf("@");

                //bool isArtist = (pos1 > 0) && (pos2 > 0) && (pos3 < 0) && (pos4 < 0) && (pos5 < pos1);
                //bool isAlbum = (pos1 > 0) && (pos2 > 0) && (pos3 > 0) && (pos4 > 0) && (pos5 > 0);

                //bool errorArtist = (pos1 > 0) && (pos2 < 0) || (pos1 < 0) && (pos2 > 0);
                //bool errorAlbum = (pos3 > 0) && (pos4 < 0) || (pos3 < 0) && (pos4 > 0);
                //if (errorArtist)
                //{
                //    FileArtistsWrite("XXXX ERROR", "6", directoryInfo.FullName);
                //}
                //else if (errorAlbum)
                //{
                //    FileAlbunsWrite("XXXX ERROR", "7", directoryInfo.FullName, "", "", "", "");
                //}

                //if (isArtist && !isAlbum)
                //{
                //    _artistFolder = directoryInfo.FullName + @"\";

                //    _artist = name.Substring(0, pos1).Trim();
                //    _country = name.Substring(pos1 + 1, pos2 - pos1 - 1).Trim();

                //    FileArtistsWrite(_artist, _country, directoryInfo.FullName); //is a Artist
                //}

                ////Debug.WriteLine(name);

                //if (isAlbum)
                //{
                //    _albumFolder = directoryInfo.FullName + @"\"; ;
                //    _albumType = directoryInfo.FullName.Replace(_artistFolder, "");
                //    int pos = _albumType.IndexOf(@"\");
                //    if (pos > 0)
                //        _albumType = _albumType.Substring(0, pos).Trim();
                //    else
                //        _albumType = "";

                //    string artist2 = name.Substring(0, pos1).Trim();
                //    _year = name.Substring(pos1 + 1, pos2 - pos1 - 1).Trim();
                //    _album = name.Substring(pos3 + 1, pos4 - pos3 - 1).Trim();
                //    _audio = name.Substring(pos4 + 1, name.Length - pos4 - 1).Trim();

                //    if (artist2 == _artist)
                //        FileAlbunsWrite(_artist, _country, _albumType, _year, _album, _audio, directoryInfo.FullName);  //is a Album
                //    else
                //        FileAlbunsWrite("ZZZZ ERROR", "2", "", "", "", "", directoryInfo.FullName);
                //}

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
            _fileErrors.WriteLine(fullName + ";" + info);
            _fileErrors.Flush();
        }

        private void FileArtistsWrite(string artist, string country, string fullName)
        {
            _fileArtists.WriteLine(artist + ";" + country + ";" + fullName);
            _fileArtists.Flush();
        }

        private void FileAlbunsWrite(string artist, string country, string albumType, string year, string album, string audio, string fullName)
        {
            _fileAlbuns.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + audio + ";" + fullName);
            _fileAlbuns.Flush();
        }

        private void FileTracksWrite(string artist, string country, string albumType, string year, string album, string disc, string trackName, string audio, string extension, string fullName)
        {
            _fileTracks.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + disc + ";" + trackName + ";" + audio + ";" + extension + ";" + fullName);
            _fileTracks.Flush();
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
