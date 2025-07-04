using System.Collections;
using UnityEngine;

namespace Assets.Scripts.NPCScripts
{

    [System.Serializable]
    public class CropTypeSprites
    {
        public Interactibles.CropType cropType;
        public Sprite sprite;
        public Sprite stage1sprite;
        public Sprite stage2sprite;
        public Sprite stage3sprite;
        public CropTypeSprites(Interactibles.CropType type) => cropType = type;
        public CropTypeSprites() { }
    }
    [CreateAssetMenu(fileName = "CropSpriteReferences", menuName ="Crop References")]
    public class CropSpritesReference : ScriptableObject
    {
        public System.Collections.Generic.List<CropTypeSprites> sprites = new(System.Enum.GetNames(typeof(Interactibles.CropType)).Length)
        {
            new((Interactibles.CropType)1),
            new((Interactibles.CropType)2),
            new((Interactibles.CropType)3)
        };
    }
}