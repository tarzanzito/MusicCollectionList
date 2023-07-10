
namespace MusicCollectionList
{
    internal static class Constants
    {
        //public const string FolderRootCollectionLoss = @"\\NAS-QNAP\Temp\_arquivado2\";
        public const string FolderRootCollectionLoss = @"\\NAS-QNAP\music\_COLLECTION\";
        public const string FolderRootCollectionLossLess = @"\\NAS-QNAP\music_lossless\_COLLECTION\";

        public const string FilesFilterLoss = "*.MP3,*.WMA";
        public const string FilesFilterLossLess = "*.FLAC,*.APE,*.WV,*.WAVE";

        public const string FileTextNameCollectionLoss = "Music_Tree.txt";
        public const string FileTextNameCollectionLossLess = "Music_LossLess_Tree.txt";

        public const string FileCsvNameCollectionLoss = "Music_Tree_Loss.csv";
        public const string FileCsvNameCollectionLossLess = "Music_Tree_LossLess.csv";

        public const string FileNameArtistsLoss = "Artists_Loss.txt";
        public const string FileNameAlbunsLoss = "Albuns_Loss.txt";
        public const string fileNameTracksLoss = "Tracks_Loss.txt";

        public const string FileNameArtistsLossLess = "Artists_LossLess.txt";
        public const string FileNameAlbunsLossLess = "Albuns_LossLess.txt";
        public const string fileNameTracksLossLess = "Tracks_LossLess.txt";

        public const string FileErrorsLoss = "Music_Tree_ERRORS.csv";
        public const string FileErrorsLossLess = "Music_LossLess_Tree_ERRORS.csv";

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
