using System.Collections.Generic;


namespace TelegramParser
{
    class KeyWord
    {
        public enum Mode { SortWord, IgnoreWord };

        public string value { get; set; }

        public Mode mode { get; set; }
    }

    class KeyWords
    {
        public List<KeyWord> keyWords = new List<KeyWord>();

        public int stopWords { get; set; }
    }
}
