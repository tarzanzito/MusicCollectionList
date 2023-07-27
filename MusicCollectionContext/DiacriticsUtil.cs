using System.Globalization;
using System.Text;


namespace MusicCollectionContext
{
    internal class DiacriticsUtil
    {
        internal enum TextCaseAction
        {
            None,
            ToLower,
            ToUpper
        }

        //remoção de chars diacríticos (example ç ó õ )
        //stripping diacritics

        internal static string RemoveDiacritics(string text, TextCaseAction textCaseAction)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string normalizedString = text.Normalize(NormalizationForm.FormD);

            StringBuilder stringBuilder = new StringBuilder(capacity: normalizedString.Length);

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

