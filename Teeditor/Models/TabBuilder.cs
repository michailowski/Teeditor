using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;
using Teeditor.TeeWorlds.MapExtension.Api;
using Windows.Storage;

namespace Teeditor.Models
{
    internal class TabBuilder : BindableBase
    {
        private ObservableCollection<TabBuilderQueueItem> _queue;

        public event EventHandler TabBuildingStarted;
        public event EventHandler<TabBuildingEndedEventArgs> TabBuildingEnded;

        public TabBuilder()
        {
            _queue = new ObservableCollection<TabBuilderQueueItem>();
            _queue.CollectionChanged += LoadingQueue_CollectionChanged;
        }

        private async void LoadingQueue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || _queue.Count != 1)
                return;

            await Process(_queue[0]);
        }

        public void Create(ProjectType projectType)
        {
            var queueItem = new TabBuilderQueueItem()
            {
                Extension = Path.GetExtension(projectType.Extension),
                InitStrategy = new CreateEditableFileStrategy(projectType)
            };

            _queue.Add(queueItem);
        }

        public void Create(StorageFile storageFile)
        {
            var queueItem = new TabBuilderQueueItem()
            {
                Extension = Path.GetExtension(storageFile.Path),
                InitStrategy = new LoadEditableFileStrategy(storageFile)
            };

            _queue.Add(queueItem);
        }

        private async Task Process(TabBuilderQueueItem queueItem)
        {
            TabBuildingStarted?.Invoke(this, EventArgs.Empty);

            var canBeOpen = GetTabFactoryByExtension(queueItem.Extension, out var tabFactory);

            if (canBeOpen == false)
            {
                TabBuildingEnded?.Invoke(this, new TabBuildingEndedEventArgs(null, _queue.Count - 1 == 0));

                _queue.RemoveAt(0);

                if (_queue.Any() == false)
                    return;

                await Process(_queue[0]);
            }

            queueItem.Tab = await tabFactory.CreateAsync(queueItem.InitStrategy);

            if (queueItem.Tab.Data.IsLoading)
            {
                queueItem.Tab.Data.Loaded += Data_Loaded;
                return;
            }

            TabBuildingEnded?.Invoke(this, new TabBuildingEndedEventArgs(queueItem.Tab, _queue.Count - 1 == 0));

            _queue.RemoveAt(0);

            if (_queue.Any() == false)
                return;

            await Process(_queue[0]);
        }

        private async void Data_Loaded(object sender, EventArgs e)
        {
            _queue[0].Tab.Data.Loaded -= Data_Loaded;

            TabBuildingEnded?.Invoke(this, new TabBuildingEndedEventArgs(_queue[0].Tab, _queue.Count - 1 == 0));

            _queue.RemoveAt(0);

            if (_queue.Any() == false)
                return;

            await Process(_queue[0]);
        }

        private bool GetTabFactoryByExtension(string extension, out TabFactoryBase tabFactory)
        {
            tabFactory = null;

            switch (extension)
            {
                case ".map":
                    tabFactory = new MapTabFactory();
                    return true;
            }

            return false;
        }
    }
}
