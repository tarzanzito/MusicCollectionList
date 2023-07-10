
using Microsoft.PowerShell.Commands;

namespace MusicCollectionList
{
    internal enum CollectionFolderType
    {
        None,
        Ok,
        Error,
        Artist,
        Album,
        //ArtistIncorrect,
        //AlbumIncorrect,
        MaybeArtistOrAlbum,
        //DuplicateControlChar,
        IncorrectFormat
    }
}