
namespace MusicCollection
{
    internal static class Constants
    {
        //public const string FolderRootCollectionLoss = @"\\NAS-QNAP\Temp\_arquivado2\";
        public const string FolderRootCollectionLoss = @"\\NAS-QNAP\music\_COLLECTION\";
        public const string FolderRootCollectionLossLess = @"\\NAS-QNAP\music_lossless\_COLLECTION\";

        public const string FilesFilterLoss = "*.MP3,*.WMA";
        public const string FilesFilterLossLess = "*.FLAC,*.APE,*.WV,*.WAVE";

        public const string FileTextNameCollectionLoss = "All-Files-Music_Loss.txt";
        public const string FileTextNameCollectionLossLess = "All-Files-Music_LossLess.txt";

        public const string FileCsvNameCollectionLoss = "All-Files-Music_Loss.csv";
        public const string FileCsvNameCollectionLossLess = "All-Files-Music_LossLess.csv";

        public const string FileNameArtistsLoss = "Artists_Loss.txt";
        public const string FileNameAlbunsLoss = "Albuns_Loss.txt";
        public const string fileNameTracksLoss = "Tracks_Loss.txt";

        public const string FileNameArtistsLossLess = "Artists_LossLess.txt";
        public const string FileNameAlbunsLossLess = "Albuns_LossLess.txt";
        public const string fileNameTracksLossLess = "Tracks_LossLess.txt";

        public const char FieldSeparator = ';';
    }

    public enum MusicFileExtension
    {
        MP3,
        WMA,
        OGG,
        ACC,
        FLAC,
        APE,
        WAV,
        WV
    }
}
