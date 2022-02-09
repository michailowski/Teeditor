using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Teeditor.Common.Models.IO
{
    public static class MostRecentlyUsedList
    {
        public static string Add(StorageFile file)
        {
            return StorageApplicationPermissions.MostRecentlyUsedList.Add(file, DateTime.Now.ToString());
        }

        public static async Task<List<MostRecentlyItem>> GetAsync()
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;

            List<MostRecentlyItem> list = new List<MostRecentlyItem>();

            foreach (AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata; // Here we store the date of addition to the mru list

                IStorageItem item = await mru.GetItemAsync(mruToken);

                if (item.IsOfType(StorageItemTypes.File))
                    list.Add(new MostRecentlyItem((StorageFile)item, DateTime.Parse(mruMetadata)));
            }

            return list;
        }
    }
}
