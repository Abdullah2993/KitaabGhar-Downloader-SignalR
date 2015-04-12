using System;

namespace Kitaabghar
{
    public class Novel
    {
        public Novel(string url, int firstIndex, int lastIndex, string imageLink, string refLink,bool newFormat)
        {
            Link = url;
            FirstIndex = firstIndex;
            LastIndex = lastIndex;
            TotalPages = (LastIndex - FirstIndex) + 1;
            ImageLink = imageLink;
            RefLink = refLink;
            Name = Link.Substring(Link.LastIndexOf("/", StringComparison.Ordinal) + 1);
            Name = Name.Substring(0, Name.LastIndexOf(".", StringComparison.Ordinal));
            NewFormat = newFormat;
        }

        public int FirstIndex { get; private set; }
        public int LastIndex { get; private set; }
        public int TotalPages { get; private set; }
        public string Link { get; private set; }
        public string ImageLink { get; private set; }
        public string RefLink { get; private set; }
        public string Name { get; private set; }
        public bool NewFormat { get; private set; }

        public string GetImageLink(int no)
        {
            return string.Format(ImageLink, no);
        }

        public string GetRefLink(int no)
        {
            return string.Format(RefLink, no);
        }
    }
}