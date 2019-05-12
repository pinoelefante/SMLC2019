using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SMLC2019.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items, bool clear = false)
        {
            if (items == null)
                return;
            if (clear)
                collection.Clear();
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
