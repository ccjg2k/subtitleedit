﻿using System.Globalization;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcCjk : ICalcLength
    {
        /// <summary>
        /// Calculate all text including space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var s = HtmlUtil.RemoveHtmlTags(text, true);

            const char zeroWidthSpace = '\u200B';
            const char zeroWidthNoBreakSpace = '\uFEFF';
            decimal length = 0;
            for (var en = StringInfo.GetTextElementEnumerator(s); en.MoveNext();)
            {
                var element = en.GetTextElement();
                if (element.Length == 1)
                {
                    var ch = element[0];
                    if (!char.IsControl(ch) &&
                        ch != zeroWidthSpace &&
                        ch != zeroWidthNoBreakSpace &&
                        ch != '\u200E' &&
                        ch != '\u200F' &&
                        ch != '\u202A' &&
                        ch != '\u202B' &&
                        ch != '\u202C' &&
                        ch != '\u202D' &&
                        ch != '\u202E')
                    {
                        if (JapaneseHalfWidthCharacters.Contains(ch))
                        {
                            length += 0.5m;
                        }
                        else if (ChineseFullWidthPunctuations.Contains(ch) ||
                                 JapaneseFullWidthCharacters.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Japanese.Contains(ch) ||
                                 LanguageAutoDetect.Letters.Korean.Contains(ch) ||
                                 IsCjk(ch))
                        {
                            length++;
                        }
                        else
                        {
                            length += 0.5m;
                        }
                    }
                }
                else
                {
                    if (JapaneseHalfWidthCharacters.Contains(element))
                    {
                        length += 0.5m;
                    }
                    else if (ChineseFullWidthPunctuations.Contains(element) ||
                             LanguageAutoDetect.Letters.Japanese.Contains(element) ||
                             LanguageAutoDetect.Letters.Korean.Contains(element) ||
                             CjkCharRegex.IsMatch(element))
                    {
                        length++;
                    }
                    else
                    {
                        length += 0.5m;
                    }
                }
            }

            return length;
        }

        public const string JapaneseHalfWidthCharacters = "｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ";
        public const string JapaneseFullWidthCharacters = "ぁあぃいぅうぇえぉおァアィイゥウェエォオㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹ一二三四五六七八九十学校日本、。・「」々〆〇";
        public const string ChineseFullWidthPunctuations = "，。、：；？！…“”—‘’（）【】「」『』〔〕《》〈〉";

        public static readonly Regex CjkCharRegex = new Regex(@"\p{IsHangulJamo}|" +
                                                              @"\p{IsCJKRadicalsSupplement}|" +
                                                              @"\p{IsCJKSymbolsandPunctuation}|" +
                                                              @"\p{IsEnclosedCJKLettersandMonths}|" +
                                                              @"\p{IsCJKCompatibility}|" +
                                                              @"\p{IsCJKUnifiedIdeographsExtensionA}|" +
                                                              @"\p{IsCJKUnifiedIdeographs}|" +
                                                              @"\p{IsHangulSyllables}|" +
                                                              @"\p{IsCJKCompatibilityForms}", RegexOptions.Compiled);
        public static bool IsCjk(char c)
        {
            var v = (int)c;
            if (v >= 0x3040 && v <= 0x309F) // Hiragana
            {
                return true;
            }

            if (v >= 0x4E00 && v <= 0x9FAF) // Common and uncommon kanji
            {
                return true;
            }

            return CjkCharRegex.IsMatch(c.ToString());
        }
    }
}
