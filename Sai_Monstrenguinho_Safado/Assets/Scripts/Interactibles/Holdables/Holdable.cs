using Assets.Scripts.PlayerScripts;
using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Holdable : Interactible
    {
        [SerializeField] private bool isPrefab;
        protected Vector3 originalPosition;
        protected Quaternion originalRotation;
        protected Transform originalParent;
        protected bool isHeld;
        protected UnityEngine.AI.NavMeshObstacle navigationObstacle;
        [SerializeField] private Collider holdableCollider;
        [SerializeField] private HeldItem relatedItem;
        [SerializeField] private float placeRayDist = 2f;
        [SerializeField] private LayerMask groundLayer = 2;
        [SerializeField] private LayerMask interactibleLayer = 5;

        public virtual void Start()
        {
            navigationObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            originalParent = isPrefab ? null : transform.parent ?? transform.root;
            if (originalParent == transform) //Se o objeto nao tiver pai na hierarquia, seu root seria ele mesmo?
            {
                GameObject parentGO = new($"Parent_{transform.name}");
                parentGO.transform.SetPositionAndRotation(transform.position, transform.rotation);
                transform.SetParent(parentGO.transform);
                originalParent = parentGO.transform;
            }

            originalPosition = transform.position;
            originalRotation = transform.rotation;
            relatedItem.transform ??= transform;
            //holdableCollider ??= GetComponent<Collider>();
        }

        /*public virtual void Update()
        {
            if (!isHeld) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                DropItem();
                //FindObjectOfType<PlayerController>().HeldItem.SetNone();
            }

            if (Input.GetKeyDown(KeyCode.Q)) 
            {
                ReturnToStartingPoint();
                //FindObjectOfType<PlayerController>().HeldItem.SetNone();
            }

        }*/

        public virtual void PickUpItem(PlayerController player)
        {
            isHeld = true;
            holdableCollider.enabled = false;
            if(navigationObstacle != null) navigationObstacle.enabled = false;
            //GridScripts.BuildSystem.instance.PickObject(_placedObject);
            transform.SetParent(player.InteractionOrigin, false);
            transform.localPosition = Vector3.zero;
            player.SetHeldItem(relatedItem);
        }

        public virtual void DropItem()
        {
            Debug.DrawLine(transform.position, transform.position + (Vector3.down * placeRayDist), Color.cyan);

            Ray ray = new(transform.position, Vector3.down);
            bool freeDrop = !Physics.Raycast(ray, placeRayDist, interactibleLayer);

            if (freeDrop && Physics.Raycast(ray, out var hit, placeRayDist, groundLayer))
            {
                Debug.Log("TRyroij");
                var buildSis = GridScripts.BuildSystem.instance;

                var pos = buildSis.SnappedPosition(hit.point);
                pos.y += transform.localScale.y * .5f;
                transform.position = pos;

                PlaceItem(pos, originalRotation);
            }
        }
        
        public virtual void ReturnToStartingPoint()
        {
            if (originalParent == null)
            {
                //Debug.Log(transform.name + "Era prefab ou nao tinha parent. Destruindo");
                Destroy(gameObject, .5f);
                return;
            }
            PlaceItem(originalPosition, originalRotation);
        }
        
        public override void Interact(object sender = null)
        {
            if (sender is PlayerController player)
            {
                switch (player.HeldItem.itemTag)
                {
                    case ItemTag.None:
                        PickUpItem(player);
                        
                        break;
                    
                    default: break;
                }
            }
        }

        //Zerar o held item do player não-manualmente depois
        private void PlaceItem(Vector3 position, Quaternion rotation)
        {
            isHeld = false;
            holdableCollider.enabled = true;
            if (navigationObstacle != null) navigationObstacle.enabled = true;

            transform.SetParent(originalParent, false);
            transform.SetPositionAndRotation(position, !isPrefab ? rotation : Quaternion.identity);
            //GridScripts.BuildSystem.instance.PlaceObject(_placedObject);
            
            //Temporario
            FindAnyObjectByType<PlayerController>().SetHeldEmpty();
        }

    }
}