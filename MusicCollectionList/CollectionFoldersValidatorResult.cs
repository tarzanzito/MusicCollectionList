
using Microsoft.PowerShell.Commands;

namespace MusicCollectionList
{
    internal class CollectionFoldersValidatorResult
    {
        public CollectionFolderType CollectionFolderType { get; private set; }
        public string Info { get; private set; }

        public CollectionFoldersValidatorResult(CollectionFolderType collectionFolderType, string info)
        {
            CollectionFolderType = collectionFolderType;
            Info = info;
        }
    }

}