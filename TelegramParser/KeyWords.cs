using System.Collections.Generic;


namespace TelegramParser
{
    class KeyWord
    {
        public enum Mode { SortWord, IgnoreWord };

        public string value  {get; set; }

        public Mode mode { get; set; }

        public KeyWord()
        {
            value = null;
            mode = 0;
        }
    }

    class KeyWords
    {
        public List<KeyWord> keyWords { get; set; }

        public int stopWords { get; set; }

        public KeyWords()
        {
            keyWords = new List<KeyWord>();
            stopWords = 0;
        }
    }
}
