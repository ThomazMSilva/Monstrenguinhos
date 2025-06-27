
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace Assets.Scripts.GridScripts
{
    [CreateAssetMenu(fileName = "PrefabBrush", menuName = "Brushes/Prefab Brush")]
    [CustomGridBrush(false, true, false, "Prefab Brush")]
    public class PrefabBrush : GameObjectBrush
    {
        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget.layer == 31) return;

            Transform transformToErase = 
                GetTransformInCell(gridLayout, brushTarget.transform, new(position.x, position.z, 0));

            if (transformToErase != null) 
            {
                Undo.DestroyObjectImmediate(transformToErase.gameObject);
            }

        }

        public static Transform GetTransformInCell(GridLayout grid, Transform parent, Vector3Int position)
        {
            int childCount = parent.childCount;
            Vector3 min = grid.LocalToWorld(grid.CellToLocalInterpolated(position));
            Vector3 max = grid.LocalToWorld(grid.CellToLocalInterpolated(position + Vector3.one));
            Bounds bounds = new Bounds((min + max) * .5f, max - min);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (bounds.Contains(child.position))
                {
                    return child;
                }
            }
            return null;
        }
    }
}
#endif