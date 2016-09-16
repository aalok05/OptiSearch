using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Storage;

namespace OptiSearch.Services
{
    class OCRservice
    {
       public static StorageFile imageFile { get; set; }    //passed from startPage to picView
       
    }
    public class OCRLanguage
    {
        public string LangName { get; set; }
        public string LangTag { get; set; }
        public Language language
        {
            get
            {
                return language;
            }
            set
            {
                language = new Language(LangTag);
            }
         }
    }
}
