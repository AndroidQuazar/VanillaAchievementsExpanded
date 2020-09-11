using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsExpanded
{
    /// <summary>
    /// Easier and prettier than implementing IndexOf directly into CardList filter
    /// </summary>
    public static class StringExtension
    {
        public static bool Contains(this string source, string target, StringComparison comp)
        {
            return source?.IndexOf(target, comp) >= 0;
        }
    }
}
