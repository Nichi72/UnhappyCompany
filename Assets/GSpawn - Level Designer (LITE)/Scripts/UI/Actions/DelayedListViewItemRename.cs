#if UNITY_EDITOR
namespace GSpawn_Lite
{
    public class DelayedListViewItemRename<TData> : EditorUpdateAction
        where TData : IUIItemStateProvider
    {
        private ListViewItem<TData> _item;

        public DelayedListViewItemRename(ListViewItem<TData> item)
        {
            _item = item;
        }

        protected override void execute()
        {
            _item.beingDelayedRename();
        }
    }
}
#endif