

using System;
using MusicCollectionContext;

namespace MusicCollectionValidators
{
    internal class CollectionFoldersValidator
    {
        private string _data;
        private readonly string _mediaFormatTags;

        public CollectionFoldersValidator(CollectionOriginType collectionOriginType)
        {
            if (collectionOriginType == CollectionOriginType.Loss)
                _mediaFormatTags = Constants.MEDIA_FORMAT_TAG_LOSS;

            if (collectionOriginType == CollectionOriginType.Lossless)
                _mediaFormatTags = Constants.MEDIA_FORMAT_TAG_LOSSLESS;
        }

        public CollectionFoldersValidatorResult ValidateFolder(string data)
        {
            _data = data;
            CollectionFoldersValidatorResult result = null;

            try
            {
                CollectionFolderType collectionFolderType = GetCollectionFolderType();

                if (collectionFolderType == CollectionFolderType.None || collectionFolderType == CollectionFolderType.MaybeArtistOrAlbum)
                    return new CollectionFoldersValidatorResult(collectionFolderType, "");



                if (collectionFolderType == CollectionFolderType.Artist)
                    result = ValidateArtistFolderRules();

                if (collectionFolderType == CollectionFolderType.Album)
                    result = ValidateAlbumFolderRules();

                if (result == null)
                    throw new Exception("CollectionFoldersValidatorResult is null");
                


                if (result.CollectionFolderType != CollectionFolderType.Ok)
                    return result;

                result = FindForDuplicateControlChars();
                if (result.CollectionFolderType != CollectionFolderType.Ok)
                    return result;

                result = FindForDuplicateSpaces();
                if (result.CollectionFolderType != CollectionFolderType.Ok)
                    return result;

                result = FindForInitOrEndSpaces();
                if (result.CollectionFolderType != CollectionFolderType.Ok)
                    return result;
                
            }
            catch(Exception ex)
            {
                result = new CollectionFoldersValidatorResult(CollectionFolderType.Error, $"-Folder:[{_data}] -Error:[{ex.Message}");
            }
 
            return result;
        }

        private CollectionFoldersValidatorResult FindForDuplicateControlChars()
        {
            int count = CountCharOccurance(Constants.COUNTRY_OPEN_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.COUNTRY_OPEN_TAG}' found");

            count = CountCharOccurance(Constants.COUNTRY_CLOSE_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.COUNTRY_CLOSE_TAG}' found");

            count = CountCharOccurance(Constants.YEAR_OPEN_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.YEAR_OPEN_TAG}' found");

            count = CountCharOccurance(Constants.YEAR_CLOSE_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.YEAR_CLOSE_TAG}' found");

            count = CountCharOccurance(Constants.ALBUM_OPEN_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.ALBUM_OPEN_TAG}' found");

            count = CountCharOccurance(Constants.ALBUM_CLOSE_TAG);
            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Duplicated '{Constants.ALBUM_CLOSE_TAG} found");
            
            return new CollectionFoldersValidatorResult(CollectionFolderType.Ok, "");
        }

        private CollectionFoldersValidatorResult FindForInitOrEndSpaces()
        {
            if (_data.Substring(0, 1) == " ")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Folder start with space");

            if (_data.Substring(_data.Length - 1, 1) == " ")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Folder end with space");

            return new CollectionFoldersValidatorResult(CollectionFolderType.Ok, "");
        }

        private CollectionFoldersValidatorResult FindForDuplicateSpaces()
        {
            int count = 0;
            foreach (char chr in _data)
            {
                if (chr == ' ')
                    count++;
                else
                    count = 0;
            }

            if (count > 1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Duplcates spaces found");

            return new CollectionFoldersValidatorResult(CollectionFolderType.Ok, "");
        }

        private CollectionFolderType GetCollectionFolderType()
        {
            int posCO = _data.IndexOf(Constants.COUNTRY_OPEN_TAG);
            int posCC = _data.IndexOf(Constants.COUNTRY_CLOSE_TAG);
            
            int posYO = _data.IndexOf(Constants.YEAR_OPEN_TAG);
            int posYC = _data.IndexOf(Constants.YEAR_CLOSE_TAG);

            int posAO = _data.IndexOf(Constants.ALBUM_OPEN_TAG);
            int posAC = _data.IndexOf(Constants.ALBUM_CLOSE_TAG);
            int posMF = _data.IndexOf(Constants.MEDIA_FORMAT_TAG);

            bool maybeItIsArtistOrAlbum = (posCO > 0) || (posCC > 0) || (posYO > 0) || (posYC > 0) || (posAO > 0) || (posAC > 0) || (posMF > 0);

            bool maybeItIsArtist = (posCO > 0) && (posCC > 0) && (posAO < 0) && (posAC < 0);// && (posMF < 0);
            //bool isArtist = (posCO > 0) && (posCC > 0) && (posCC > posCO); //COUNTRY exists
            //isArtist = isArtist && (posAO < 0) && (posAC < 0);             //ALBUM not exists
            //isArtist = isArtist  && ((posMF < 0) || (posMF < posCO));     //MEDIA_FORMAT not exists or before country

            bool maybeItIsAlbum = (posYO > 0) && (posYC > 0) && (posAO > 0) && (posAC > 0) && (posMF > 0);
            //bool isAlbum = (posYO > 0) && (posYC > 0) && (posYC > posYO);       //YEAR exists
            //isAlbum = isAlbum && (posAO > 0) && (posAC > 0) && (posAC > posAO); //ALBUM exists
            //isAlbum = isAlbum && (posAO > posYC);                               //YEAR < ALBUM rule
            //isAlbum = isAlbum && (posMF > posAC);                               //ALBUM < MEDIA_FORMAT rule
            //isAlbum = isAlbum && (posMF > posAC);                                   //MEDIA_FORMAT exits
 
            if (maybeItIsArtist)
                return CollectionFolderType.Artist;

            if (maybeItIsAlbum)
                return CollectionFolderType.Album;

            if (maybeItIsArtistOrAlbum)
                return CollectionFolderType.MaybeArtistOrAlbum;

            return CollectionFolderType.None;
        }

        private CollectionFoldersValidatorResult ValidateArtistFolderRules()
        {
            //required formats
            string str = " " + Constants.COUNTRY_OPEN_TAG;
            int pos1 = _data.IndexOf(str);
            if (pos1 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Artist: One space before '{Constants.COUNTRY_OPEN_TAG}' not found");

            //controls sequence
            int pos2 = _data.IndexOf(Constants.COUNTRY_CLOSE_TAG);
            if (pos2 < pos1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Artist: '{Constants.COUNTRY_CLOSE_TAG}' before '{Constants.COUNTRY_OPEN_TAG}'");

            //invalid formats
            str = Constants.COUNTRY_OPEN_TAG + " ";
            pos1 = _data.IndexOf(str);
            if (pos1 > 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Artist: One space after '{Constants.COUNTRY_OPEN_TAG}' found");

            //contents
            char[] chrs = { Constants.COUNTRY_OPEN_TAG, Constants.COUNTRY_CLOSE_TAG };
            string[] words = _data.Split(chrs);

            if (words[0].Trim() == "")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Artist: Artist name is empty");

            if (words[1].Trim() == "")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Artist: Country name is empty");

            return new CollectionFoldersValidatorResult(CollectionFolderType.Ok, "");
        }


        private CollectionFoldersValidatorResult ValidateAlbumFolderRules()
        {
            //required formats
            string str = " " + Constants.YEAR_OPEN_TAG;
            int pos1 = _data.IndexOf(str);
            if (pos1 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space before '{Constants.YEAR_OPEN_TAG}' not found");

            str = Constants.YEAR_CLOSE_TAG + " ";
            int pos2 = _data.IndexOf(str);
            if (pos2 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space after '{Constants.YEAR_CLOSE_TAG}' not found");

            str = " " + Constants.ALBUM_OPEN_TAG;
            int pos3 = _data.IndexOf(str);
            if (pos3 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space before '{Constants.ALBUM_OPEN_TAG}' not found");

            str = Constants.ALBUM_CLOSE_TAG + " ";
            int pos4 = _data.IndexOf(str);
            if (pos4 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space after '{Constants.ALBUM_CLOSE_TAG}' not found");

            str = " " + Constants.MEDIA_FORMAT_TAG;
            int pos5 = _data.LastIndexOf(str);
            if (pos5 < 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space before '{Constants.MEDIA_FORMAT_TAG}' not found");


            // conctrols sequences

            if (pos2 < pos1)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: '{Constants.YEAR_CLOSE_TAG}' before '{Constants.YEAR_OPEN_TAG}'");

            if (pos4 < pos3)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: '{Constants.ALBUM_CLOSE_TAG}' before '{Constants.ALBUM_OPEN_TAG}'");

            if (pos5 < pos4)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: '{Constants.MEDIA_FORMAT_TAG}' before '{Constants.ALBUM_CLOSE_TAG}'");

            if (pos3 < pos2)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: '{Constants.ALBUM_OPEN_TAG}' before '{Constants.YEAR_CLOSE_TAG}'");

            //invalid formats

            str = Constants.YEAR_OPEN_TAG + " ";
            pos1 = _data.IndexOf(str);
            if (pos1 > 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space after '{Constants.YEAR_OPEN_TAG}' found");

            str = " " + Constants.YEAR_CLOSE_TAG;
            pos1 = _data.IndexOf(str);
            if (pos1 > 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space before '{Constants.YEAR_CLOSE_TAG}' found");

            str = Constants.ALBUM_OPEN_TAG + " ";
            pos1 = _data.IndexOf(str);
            if (pos1 > 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space after '{Constants.ALBUM_OPEN_TAG}' found");

            str = " " + Constants.ALBUM_CLOSE_TAG;
            pos1 = _data.IndexOf(str);
            if (pos1 > 0)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space before '{Constants.ALBUM_CLOSE_TAG}' found");

            //str =  MEDIA_FORMAT_TAG + " ";
            //pos1 = _data.LastIndexOf(str);
            //if (pos1 > 0)
            //    return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, $"Album: One space after '{MEDIA_FORMAT_TAG}' found");
   

            //contents

            char[] chrs = { Constants.YEAR_OPEN_TAG, Constants.YEAR_CLOSE_TAG, Constants.ALBUM_OPEN_TAG, Constants.ALBUM_CLOSE_TAG };
            string[] words = _data.Split(chrs);

            if (words.Length != 5)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: Folder do not have 5 parts");

            if (words[0].Trim() == "")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: Artist name is empty");

            if (words[1].Trim() == "")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: Year is empty");

            if (words[3].Trim() == "")
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: Album name is empty");

            string temp = words[4].Trim();
            if (temp.Length < 2)
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: @File media type is empty");

            if (temp.Substring(0, 1) != Constants.MEDIA_FORMAT_TAG.ToString())
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: @File media type is empty");

            if (temp.Contains(' '))
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: @File media type is with spaces");

            if (!_mediaFormatTags.Contains(temp.Substring(1)))
                return new CollectionFoldersValidatorResult(CollectionFolderType.IncorrectFormat, "Album: @File media type is invalid");


            return new CollectionFoldersValidatorResult(CollectionFolderType.Ok, "");
        }

        private int CountCharOccurance(char chr)
        {
            int count = 0;
            foreach (char c in _data)
            {
                if (c == chr)
                    count++;
            }

            return count;
        }

        private int CountDuplicateSpaces()
        {
            int count = 0;
            foreach (char c in _data)
            {
                if (c == ' ')
                    count++;
                else
                    count = 0;
            }

            return count;
        }

    }
}
