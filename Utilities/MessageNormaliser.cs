using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MorkoBotRavenEdition.Utilities
{
    public class MessageNormaliser
    {
        private static readonly Dictionary<char, IList<char>> SpecialNormaliser = new Dictionary<char, IList<char>>
        {
            { 'o', new List<char>
            {
                'u', 'e', '-', 'Ø', '゜',
                '⊙', '●', '♼', 'ø',
                '☆', '✧', '♥', '◕',
                'ᅌ', '◔', 'ʘ', '⓪', '@',
                'σ', '๏', 'Ꭷ', '0', '◑', '✿', '✺'
            }},

            { 'w', new List<char>
            {
                'ω', '꒳', 'ɯ', 'ʍ', 'Ꮗ', 'n'
            }},
        };

        public static string Normalise(string value)
        {
            var builder = new StringBuilder();
            foreach (var c in value.Normalize(NormalizationForm.FormD).ToCharArray())
            {
                var sp = Normalise(c);

                if (CharUnicodeInfo.GetUnicodeCategory(sp) != UnicodeCategory.NonSpacingMark)
                    builder.Append(sp);
            }
                
            return builder.ToString();
        }

        public static char Normalise(char value)
        {
            foreach (var (key, list) in SpecialNormaliser)
                if (list.Any(match => value == match)) return key;

            return value;
        }
    }
}
