using System;
using Windows.Storage;

namespace Teeditor.Common.Models.IO
{
    public class MostRecentlyItem
    {
        public StorageFile File { get; set; }
        public DateTime Date { get; set; }

        public MostRecentlyItem(StorageFile file, DateTime date)
        {
            File = file;
            Date = date;
        }
    }
}
