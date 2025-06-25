using System.Collections;
using UnityEngine;

namespace Assets.Scripts.GridScripts
{
    public class PlaceableObject : MonoBehaviour
    {
        public bool IsPlaced { get; private set; }
        public Vector3Int Size { get; private set; }
        private Vector3[] _vertices;

        private void GetColliderVertexPosLocal()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();

            _vertices = new Vector3[4];

            _vertices[0] =
                boxCollider.center + new Vector3(-boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * .5f;

            _vertices[1] = 
                boxCollider.center + new Vector3(boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * .5f;
            
            _vertices[2] = 
                boxCollider.center + new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * .5f;
            
            _vertices[3] = 
                boxCollider.center + new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * .5f;
        }

        private void CalculateCellSize()
        {
            Vector3Int[] verts = new Vector3Int[_vertices.Length];

            for (int i = 0; i < verts.Length; i++) 
            {
                Vector3 worldPos = transform.TransformPoint(_vertices[i]);
                verts[i] = BuildSystem.instance.gridLayout.WorldToCell(worldPos);
            }

            Size = new Vector3Int
            (
                Mathf.Abs((verts[0] - verts[1]).x), 
                Mathf.Abs((verts[0] - verts[3]).y), 
                1
            );
        }

        public Vector3 GetStartPosition() => transform.TransformPoint(_vertices[0]);

        private void Start()
        {
            GetColliderVertexPosLocal();
            CalculateCellSize();
        }

        public virtual void Place()
        {

        }   
    }
}