using MusicCollectionContext;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace MusicCollectionList
{
    public class RenameFolders
    {

        /// <summary>
        /// rename folders terminated with [...] by ""
        /// </summary>
        /// <param name="collectionOriginType"></param>
        public void RunIt(CollectionOriginType collectionOriginType)
        {
            string rootPath;
            string fileNameError;

            if (collectionOriginType == CollectionOriginType.Loss)
            {
                rootPath = Constants.FolderRootCollectionLoss;
                fileNameError = System.IO.Path.Join(rootPath, Constants.FileErrorsLoss);
            }
            else
            {
                rootPath = Constants.FolderRootCollectionLossLess;
                fileNameError = System.IO.Path.Join(rootPath, Constants.FileErrorsLossLess);
            }

            if (!File.Exists(fileNameError))
                return;

//            Log.Information(fileNameError);

            StreamReader reader = null;
            int count = 0;
            string line = "";

            try
            {
                reader = new StreamReader(fileNameError, Constants.StreamsEncoding);

                int pos;
                while ((line = reader.ReadLine()) != null)
                {
                    count++;
                    pos = line.IndexOf("[...]");
                    if (pos > 0)
                    {
                        string tempS = line.Substring(0, pos + 5);
                        if (System.IO.Directory.Exists(tempS))
                        {
                            string tempT = tempS.Replace("[...]", "");
                            System.IO.Directory.Move(tempS, tempT);

                            pos = 0;
                        }
                    }
                }

                Log.Information(count.ToString());

            }
            catch (Exception ex)
            {
                Log.Error($"Folder:[{line}] -Error:[{ex.Message}");

            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

        }
    }
}

