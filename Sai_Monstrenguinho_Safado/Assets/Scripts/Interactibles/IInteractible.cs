namespace Assets.Scripts.Interactibles
{
    public interface IInteractible
    {
        void Interact(object sender = null);

        void Select(object sender = null) { UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Select()\'"); }

        void Deselect(object sender = null) { UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Deselect()\'"); }

    }
}