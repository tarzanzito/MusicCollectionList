using System.Diagnostics;
using System.Globalization;

namespace MusicCollection
{
    public class Produto
    {
        public string nome { get; set; }
    }

    internal class Program
    {

        public static void Main()
        {

            PrintTextElementCount("a");
            // Number of chars: 1
            // Number of runes: 1
            // Number of text elements: 1

            PrintTextElementCount("á");
            // Number of chars: 2
            // Number of runes: 2
            // Number of text elements: 1

            PrintTextElementCount("👩🏽‍🚒");
            // Number of chars: 7
            // Number of runes: 4
            // Number of text elements: 1

            List<Produto> produtos = new List<Produto> { new Produto { nome = "aa" } };

            IEnumerable<Produto> enumerable1 = produtos.Where(x => x.nome == "aa");
            //IQueryable<Produto> enumerable2 = produtos.Where(x => x.nome == "aa");

            IEnumerator<Produto> enumerator = enumerable1.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Produto produto = enumerator.Current;
                Console.WriteLine(produto.nome);
            }

            List<Produto> list = enumerable1.ToList();
            
            //IQueryable
           // IEnumerable<Produto> xx3 = enumerable2.ToList();
          //  IQueryable<Produto> xx4 = (IQueryable<Produto>)enumerable2.ToList();


        }

        static void PrintTextElementCount(string s)
        {
            Console.WriteLine(s);
            Console.WriteLine($"Number of chars: {s.Length}");
            Console.WriteLine($"Number of runes: {s.EnumerateRunes().Count()}");

            TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(s);

            int textElementCount = 0;
            while (enumerator.MoveNext())
            {
                textElementCount++;
            }

            Console.WriteLine($"Number of text elements: {textElementCount}");
        }
    }
}
//class WhereListIterator<TSource> : Iterator<TSource>
//{
//    List<TSource> source;
//    Func<TSource, bool> predicate;
//    List<TSource>.Enumerator enumerator;

//    public WhereListIterator(List<TSource> source, Func<TSource, bool> predicate)
//    {
//        this.source = source;
//        this.predicate = predicate;
//    }

//    public override Iterator<TSource> Clone()
//    {
//        return new WhereListIterator<TSource>(source, predicate);
//    }

//    public override bool MoveNext()
//    {
//        switch (state)
//        {
//            case 1:
//                enumerator = source.GetEnumerator();
//                state = 2;
//                goto case 2;
//            case 2:
//                while (enumerator.MoveNext())
//                {
//                    TSource item = enumerator.Current;
//                    if (predicate(item))
//                    {
//                        current = item;
//                        return true;
//                    }
//                }
//                Dispose();
//                break;
//        }
//        return false;
//    }

//    public override IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
//    {
//        return new WhereSelectListIterator<TSource, TResult>(source, predicate, selector);
//    }

//    public override IEnumerable<TSource> Where(Func<TSource, bool> predicate)
//    {
//        return new WhereListIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
//    }
//}

