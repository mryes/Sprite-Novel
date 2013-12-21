using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
    static class MiscUtilities
    {
        public static List<string> SplitEveryNthOccurrence(string s, char ch, int n)
        {
            var strings = new List<string>();
            int occurrences = 0;
            int startingSpot = 0;

            for (int i = 0; i < s.Length; i++) {
                if (s[i] == ch) {
                    ++occurrences;
                    if (occurrences >= n) {
                        strings.Add(s.Substring(startingSpot, i - startingSpot + 1));
                        startingSpot = i + 1;
                        occurrences = 0;
                    }
                }
            }

            // This doesn't seem useful for my purposes:
            // strings.Add(s.Substring(startingSpot));

            return strings.ToList();
        }
    }
}
