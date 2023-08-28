using System.Collections.Generic;

namespace PlaceholderSoftware.WetStuff.Extensions
{
    internal static class IListExtensions
    {
        public static void RemoveNulls<T>([NotNull] [ItemCanBeNull] this IList<T> list, int nulls) where T : class
        {
            if (nulls == 0)
                return;

            // Keep track of how far back each item should be copied backwards
            var copyBackOffset = 0;

            // Find the first null item
            var index = 0;
            for (; index < list.Count; index++)
            {
                if (list[index] == null)
                    break;
            }

            // Walk through list, shifting items backwards
            for (; index < list.Count && copyBackOffset < nulls; index++)
            {
                if (list[index] == null)
                    copyBackOffset++;
                else
                    list[index - copyBackOffset] = list[index];
            }

            // We've found all nulls, shift back the last part of the list
            for (; index < list.Count; index++)
                list[index - copyBackOffset] = list[index];

            // Now delete the last items in the list
            for (var i = 0; i < copyBackOffset; i++)
                list.RemoveAt(list.Count - 1);
        }
    }
}