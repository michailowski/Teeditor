using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Teeditor.Common.Models.IO
{
    public static class MostRecentlyUsedList
    {
        public static string Add(StorageFile file) => StorageApplicationPermissions.MostRecentlyUsedList.Add(file, DateTime.Now.ToString());
        
        public static async Task<List<MostRecentlyItem>> GetAsync()
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            var list = new List<MostRecentlyItem>();

            foreach (AccessListEntry entry in mru.Entries)
            {
                var mruToken = entry.Token;
                var mruMetadata = entry.Metadata;

                var item = await mru.GetItemAsync(mruToken);

                if (item.IsOfType(StorageItemTypes.File))
                    list.Add(new MostRecentlyItem((StorageFile)item, DateTime.Parse(mruMetadata)));
            }

            return list;
        }

        public static async Task<bool> HasItems()
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;

            foreach (AccessListEntry entry in mru.Entries)
            {
                var item = await mru.GetItemAsync(entry.Token);

                if (item.IsOfType(StorageItemTypes.File))
                    return true;
            }

            return false;
        }
    }
}
