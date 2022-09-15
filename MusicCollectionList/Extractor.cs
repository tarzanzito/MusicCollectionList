using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection
{
    //dir \\NAS-QNAP\music_lossless\_COLLECTION\*.mp3 /s>d:\mp3.txt

    //Get-ChildItem -Path "\\NAS-QNAP\music_lossless\_COLLECTION" -Filter "*.mp3" -Recurse | Out-File "d:\mp3_p.txt -Append

    internal class Extractor
    {
        private StreamWriter fileArtists;
        private StreamWriter fileAlbuns;
        private StreamWriter fileTracks;
        private string artistFolder;
        private string albumFolder;

        private string artist;
        private string country;
        private string albumType;
        private string year;
        private string album;
        private string audio;
        private int count = 0;

        public void LoadDirectory(string Dir)
        {
            fileArtists = new StreamWriter("Artists.txt", false, Encoding.UTF8);
            fileAlbuns = new StreamWriter("albuns.txt", false, Encoding.UTF8);
            fileTracks = new StreamWriter("tracks.txt", false, Encoding.UTF8);

            DirectoryInfo di = new DirectoryInfo(Dir);

            //LoadFiles(Dir);
            LoadSubDirectories(Dir);
        }

        private void LoadSubDirectories(string dir)
        {
            // Get all subdirectories  
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            
            foreach (string subdirectory in subdirectoryEntries)
            {

                DirectoryInfo di = new DirectoryInfo(subdirectory);
                string name = di.Name;
                int pos1 = name.IndexOf("{"); 
                int pos2 = name.IndexOf("}");
                int pos3 = name.IndexOf("[");
                int pos4 = name.IndexOf("]");
                int pos5 = name.IndexOf("@");

                bool isArtist = (pos1 > 0) && (pos2 > 0) && (pos3 < 0) && (pos4 < 0) && (pos5 < 0);
                bool isAlbum = (pos1 > 0) && (pos2 > 0) && (pos3 > 0) && (pos4 > 0) && (pos5 > 0);


                bool errorArtist = (pos1 > 0) && (pos2 < 0) || (pos1 < 0) && (pos2 > 0);
                bool errorAlbum = (pos3 > 0) && (pos4 < 0) || (pos3 < 0) && (pos4 > 0);
                if (errorArtist)
                {
                    fileArtists.WriteLine("ZZZ0 ERROR;" + di.FullName);
                    fileArtists.Flush();
                }
                else if (errorAlbum)
                {
                    fileAlbuns.WriteLine("ZZZ1 ERROR" + ";" + di.FullName + ";" + "" + ";" + "" + ";" + "" + ";" + "");
                    fileAlbuns.Flush();
                }


                if (isArtist && !isAlbum)
                {
                    artistFolder = di.FullName + @"\";

                    artist = name.Substring(0, pos1).Trim();
                    country = name.Substring(pos1 + 1, pos2 - pos1 - 1).Trim();

                    fileArtists.WriteLine(artist + ";" + country);
                    fileArtists.Flush();
                }

                Debug.WriteLine(name);

                if (isAlbum)
                {
                    albumFolder = di.FullName + @"\"; ;
                    albumType = di.FullName.Replace(artistFolder,"");
                    int pos = albumType.IndexOf(@"\");
                    if (pos > 0)
                        albumType = albumType.Substring(0, pos).Trim();
                    else
                        albumType = "";

                    string artist2 = name.Substring(0, pos1).Trim();
                    year = name.Substring(pos1 + 1, pos2 - pos1 -1).Trim();
                    album = name.Substring(pos3 + 1, pos4 - pos3 - 1).Trim();
                    audio = name.Substring(pos4 + 1, name.Length - pos4 - 1).Trim();

                    if (artist2 == artist)
                        fileAlbuns.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + audio);
                    else
                        fileAlbuns.WriteLine("ZZZ2 ERROR" + ";" + di.FullName + ";" + "" + ";" + "" + ";" + "" + ";" + "");

                    fileAlbuns.Flush();
                }


                LoadSubDirectories(subdirectory);
                LoadFiles(subdirectory);
            }
        }
        private void LoadFiles(string dir)
        {
            string[] fileArray = Directory.GetFiles(dir, "*.MP3");
            if (fileArray.Length == 0)
                return;

            string disc = "";
            if (albumFolder.Length < dir.Length)
                disc = dir.Replace(albumFolder, "");

            string aa = albumFolder;
            string bb = dir;



            //albumFolder = di.FullName + @"\"; ;
            //albumType = di.FullName.Replace(artistFolder, "");
            //int pos = albumType.IndexOf(@"\");
            //if (pos > 0)
            //    albumType = albumType.Substring(0, pos).Trim();
            //else
            //    albumType = "";

            //string artist2 = name.Substring(0, pos1).Trim();
            //year = name.Substring(pos1 + 1, pos2 - pos1 - 1).Trim();
            //album = name.Substring(pos3 + 1, pos4 - pos3 - 1).Trim();
            //audio = name.Substring(pos4 + 1, name.Length - pos4 - 1).Trim();


            string temp = "";
            // Loop through them to see files  
            foreach (string file in fileArray)
            {
                FileInfo fi = new FileInfo(file);

                fileTracks.WriteLine(artist + ";" + country + ";" + albumType + ";" + year + ";" + album + ";" + audio + ";" +disc + ";" + fi.Name + ";" + fi.Extension.ToLower());
                fileTracks.Flush();
                count++;
                Console.WriteLine(count);

            }
        }

    }
}
