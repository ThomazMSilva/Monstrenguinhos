namespace Assets.Scripts.Interactibles
{
    public interface IInteractible
    {
        void Interact();

        void Select() { UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Select()\'"); }

        void Deselect() { UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Deselect()\'"); }

    }
}