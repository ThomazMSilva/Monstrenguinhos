using System.Collections;
using UnityEngine;

namespace Assets.Scripts.NPCScripts
{

    [System.Serializable]
    public class CropyTypeSprites
    {
        public Interactibles.CropType cropType;
        public Sprite sprite;
        public CropyTypeSprites(Interactibles.CropType type) => cropType = type;
        public CropyTypeSprites() { }
    }
    [CreateAssetMenu(fileName = "CropSpriteReferences", menuName ="Crop References")]
    public class CropSpritesReference : ScriptableObject
    {
        public System.Collections.Generic.List<CropyTypeSprites> sprites = new(System.Enum.GetNames(typeof(Interactibles.CropType)).Length)
        {
            new((Interactibles.CropType)1),
            new((Interactibles.CropType)2),
            new((Interactibles.CropType)3)
        };
    }
}