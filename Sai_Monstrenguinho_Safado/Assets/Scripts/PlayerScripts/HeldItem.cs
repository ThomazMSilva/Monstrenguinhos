

namespace Assets.Scripts.PlayerScripts
{
    [System.Serializable]
    public class HeldItem
    {
        public ItemTag itemTag;
        public UnityEngine.Transform transform;

        public HeldItem(ItemTag itemTag, UnityEngine.Transform transform = null)
        {
            this.itemTag = itemTag;
            this.transform = transform;
        }

        public HeldItem()
        {
            this.itemTag = ItemTag.None;
            this.transform = null;
        }

        public void SetNone() => this.itemTag = ItemTag.None;

    }

    [System.Serializable]
    public enum ItemTag
    {
        None,
        Broom,
        Bucket,
        Box,
        Seed,
        Crop
    }

}