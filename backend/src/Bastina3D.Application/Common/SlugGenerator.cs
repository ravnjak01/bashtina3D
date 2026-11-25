using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bastina3D.Application.Common
{
    public class SlugGenerator
    {
        public static string Slugify(string s)
        {
            var normalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            var ascii = sb.ToString().ToLowerInvariant().Replace("đ", "dj");
            var slug = Regex.Replace(ascii, "[^a-z0-9]+", "-").Trim('-');
            if (slug.Length > 180) slug = slug[..180].Trim('-');   // column is 200, leave room for "-123"
            return slug.Length == 0 ? "site" : slug;
        }

        /// In-memory uniqueness check (used by bulk import). Adds the result to `taken`.
        public static string Unique(string name, HashSet<string> taken)
        {
            var baseSlug = Slugify(name);
            var slug = baseSlug;
            for (var i = 2; !taken.Add(slug); i++) slug = $"{baseSlug}-{i}";
            return slug;
        }
    }
}
