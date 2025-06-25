using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.GridScripts
{
    public class BuildSystem : MonoBehaviour
    {
        public static BuildSystem instance;

        public GridLayout gridLayout;

        private Grid grid;
        [SerializeField] private Tilemap mainTilemap;
        [SerializeField] private TileBase occupiedTile;
        [SerializeField] private TileBase nonOccupiedTile;

        public List<GameObject> prefabs;

        private PlaceableObject objectToPlace;

        #region Unity_Methods
        
        private void Awake()
        {
            instance = this;
            grid = gridLayout.gameObject.GetComponent<Grid>();
        
        }
        #endregion


        public Vector3 SnappedPosition(Vector3 position)
        {
            Vector3Int cellPos = gridLayout.WorldToCell(position);
            return grid.GetCellCenterWorld(cellPos);
        }

        public bool CanBePlaced(PlaceableObject placeableObject)
        {
            BoundsInt area = new()
            {
                position = gridLayout.WorldToCell(placeableObject.GetStartPosition()),
                size = placeableObject.Size
            };

            TileBase[] baseArray = mainTilemap.GetTilesBlock(area);

            foreach (var tileBase in baseArray) 
            {
                if (tileBase == occupiedTile) return false;
            }
            return true;
        }
    
        public void FillArea(TileBase tile, Vector3Int start,  Vector3Int size)
        {
            mainTilemap.BoxFill(start, tile, start.x, start.y,
                                start.x + size.x, start.y + size.y);
        }

        public void PlaceObject(PlaceableObject placeableObject)
        {
            if(!CanBePlaced(placeableObject)) return;

            //placeableObject.Place();
            Vector3Int start = gridLayout.WorldToCell(placeableObject.GetStartPosition());
            FillArea(occupiedTile, start, placeableObject.Size);
        }

        public void PickObject(PlaceableObject placeableObject)
        {
            Vector3Int start = gridLayout.WorldToCell(placeableObject.GetStartPosition());
            FillArea(null, start, placeableObject.Size);
        }
    }
}
