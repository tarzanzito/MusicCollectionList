

namespace MusicCollectionContext
{
    public class Constants
    {
        public static readonly System.Text.Encoding StreamsEncoding = System.Text.Encoding.UTF8;

        //public const string FolderRootCollectionLoss = @"\\NAS-QNAP\music\_COLLECTION\";
        public const string FolderRootCollectionLoss = @"\\NAS-SYNOLOGY\music\_COLLECTION\";
        
        public const string FolderRootCollectionLossLess = @"\\NAS-QNAP\music_lossless\_COLLECTION\";
        //public const string FolderRootCollectionLossLess = @"\\NAS-SYNOLOGY\music_lossless\_COLLECTION\";

        public const string FileExtensionsFilterLoss = "*.MP3,*.WMA";
        public const string FileExtensionsFilterLossLess = "*.FLAC,*.APE,*.WV,*.WAVE";

        public const string TreeTextFileNameCollectionLoss = "Music_Tree_Loss.txt";
        public const string TreeTextFileNameCollectionLossLess = "Music_Tree_LossLess.txt";

        public const string TreeCsvFileNameCollectionLoss = "Music_Tree_Loss.csv";
        public const string TreeCsvFileNameCollectionLossLess = "Music_Tree_LossLess.csv";

        public const string TreeTempFileNameCollectionLoss = "Music_Tree_Loss.tmp";
        public const string TreeTempFileNameCollectionLossLess = "Music_Tree_LossLess.tmp";

        public const string FileNameArtistsLoss = "Artists_Loss.txt";
        public const string FileNameAlbunsLoss = "Albuns_Loss.txt";
        public const string fileNameTracksLoss = "Tracks_Loss.txt";

        public const string FileNameArtistsLossLess = "Artists_LossLess.txt";
        public const string FileNameAlbunsLossLess = "Albuns_LossLess.txt";
        public const string fileNameTracksLossLess = "Tracks_LossLess.txt";

        public const string FileErrorsLoss = "Music_Tree_ERRORS.csv";
        public const string FileErrorsLossLess = "Music_LossLess_Tree_ERRORS.csv";

        public const char FieldSeparator = ';';

        //folder tags
        public const char COUNTRY_OPEN_TAG = '{';
        public const char COUNTRY_CLOSE_TAG = '}';
        public const char YEAR_OPEN_TAG = '{';
        public const char YEAR_CLOSE_TAG = '}';
        public const char ALBUM_OPEN_TAG = '[';
        public const char ALBUM_CLOSE_TAG = ']';
        public const char MEDIA_FORMAT_TAG = '@';
        public const string MEDIA_FORMAT_TAG_LOSS = "MP3,128,192,160";
        public const string MEDIA_FORMAT_TAG_LOSSLESS = "FLAC,APE,WAV,WV";

    }


}
