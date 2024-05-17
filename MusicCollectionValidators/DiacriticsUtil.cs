using System.Drawing;
using System;
using System.Globalization;
using System.IO;
using System.Text;
namespace MusicCollectionValidators
{
    //Portugal
    //Diacrítico - Um diacrítico é um sinal gráfico que se coloca sobre,
    //sob ou através de uma letra para alterar a sua realização fonética,
    //isto é, o seu som, ou para marcar qualquer outra característica linguística.
    //São exemplos: acento agudo, acento grave, acento circunflexo, trema, til, mácron, caron, braquia.
    internal class DiacriticsUtil
    {
        internal enum TextCaseAction
        {
            None,
            ToLower,
            ToUpper
        }

        //
        //replace chars like á, à, ã, â -> a 
        //replace chars like é, è, ê    -> e
        //etc
        //
        //diacritics
        //
        internal static string RemoveDiacritics(string text, TextCaseAction textCaseAction)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            foreach (char letter in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(letter);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(letter);
            }

            string result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            //extra
            switch (textCaseAction)
            {
                case TextCaseAction.ToLower:
                    return result.ToLower();
                case TextCaseAction.ToUpper:
                    return result.ToUpper();
                default:
                    return result;
            }
        }
    }
}

