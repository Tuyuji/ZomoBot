using System;

namespace Zomo.Core.Services.Audio
{
    public struct Song
    {
        public string Title;
        public string Author;
        public Uri Uri;

        public Song(Uri uri)
        {
            Title = "Unknown";
            Author = "Unknown";
            Uri = uri;
        }

        public Song(string title, string author, Uri uri)
        {
            Title = title;
            Author = author;
            Uri = uri;
        }
    }
}