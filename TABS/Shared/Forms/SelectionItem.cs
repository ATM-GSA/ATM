namespace TABS.Shared.Forms
{
    public class SelectionItem<T>
    {
        public bool isSelected { get; set; }
        public T value { get; set; }

        public SelectionItem(T item)
        {
            isSelected = false;
            value = item;
        }

        public SelectionItem(T item, bool selected)
        {
            isSelected = selected;
            value = item;
        }

    }
}
