
namespace MusicCollection
{
    internal class Program
    {
        public static void Main(string[] args)
        {

            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");



            MusicCollection.Extractor extractor = new ();
            extractor.LoadDirectory(@"\\NAS-QNAP\music\_COLLECTION");
            //class1.LoadDirectory(@"D:\_COLLECTION");
        }
    }
}