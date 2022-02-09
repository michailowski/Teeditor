using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Teeditor.Common.AttachedProperties
{
    public enum ItemsIndexerMode { FromZero, FromOne }

    [Bindable]
    public class ItemsIndexer : DependencyObject
    {
        private static List<ItemsControl> _ownersCollections = new List<ItemsControl>();

        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached(
              "IsEnable",
              typeof(bool),
              typeof(ItemsIndexer),
              new PropertyMetadata(false)
            );

        public static readonly DependencyProperty ItemsCollectionNameProperty =
            DependencyProperty.RegisterAttached(
              "ItemsCollectionName",
              typeof(string),
              typeof(ItemsIndexer),
              new PropertyMetadata(null)
            );

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.RegisterAttached(
              "Index",
              typeof(string),
              typeof(ItemsIndexer),
              new PropertyMetadata("Undefined")
            );

        public static readonly DependencyProperty NumeroSignTextProperty =
            DependencyProperty.RegisterAttached(
              "NumeroSignText",
              typeof(string),
              typeof(ItemsIndexer),
              new PropertyMetadata("")
            );

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.RegisterAttached(
              "Mode",
              typeof(ItemsIndexerMode),
              typeof(ItemsIndexer),
              new PropertyMetadata(ItemsIndexerMode.FromZero)
            );

        public static void SetIsEnable(UIElement element, bool value)
        {
            var itemsControl = (ItemsControl)element;

            if (string.IsNullOrEmpty(itemsControl?.Name))
            {
                element.SetValue(IsEnableProperty, false);
                return;
            }

            element.SetValue(IsEnableProperty, value);

            if (value == false)
            {
                _ownersCollections.Remove(itemsControl);
            }
            else if (value == true && _ownersCollections.Contains(itemsControl) == false)
            {
                _ownersCollections.Add(itemsControl);
            }
        }

        public static bool GetIsEnable(UIElement element)
            => (bool)element.GetValue(IsEnableProperty);

        public static void SetItemsCollectionName(UIElement element, string name)
            => element.SetValue(ItemsCollectionNameProperty, name);

        public static string GetItemsCollectionName(UIElement element)
            => (string)element.GetValue(ItemsCollectionNameProperty);

        public static void SetMode(UIElement element, ItemsIndexerMode mode)
            => element.SetValue(ModeProperty, mode);

        public static string GetNumeroSignText(UIElement element)
            => (string)element.GetValue(NumeroSignTextProperty);

        public static void SetNumeroSignText(UIElement element, string text)
            => element.SetValue(NumeroSignTextProperty, text);

        public static ItemsIndexerMode GetMode(UIElement element)
            => (ItemsIndexerMode)element.GetValue(ModeProperty);

        public static void SetIndex(UIElement element, string index)
            => element.SetValue(IndexProperty, index);

        public static string GetIndex(UIElement element)
        {
            var frameworkElement = element as FrameworkElement;
            var itemsCollectionName = GetItemsCollectionName(element);
            var numeroSignText = GetNumeroSignText(element);
            var itemsControl = _ownersCollections.FirstOrDefault(x => x.Name == itemsCollectionName);
            var index = itemsControl?.Items.IndexOf(frameworkElement?.DataContext);
            
            return index != null ?  $"{numeroSignText}{index + (int)GetMode(element)}" : "Undefined";
        }
    }
}
