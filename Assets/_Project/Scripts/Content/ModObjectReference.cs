namespace Mahou.Content
{
    [System.Serializable]
    public class ModObjectReference
    {
        public string modIdentifier;
        public string objectIdentifier;

        public ModObjectReference()
        {

        }

        public ModObjectReference(string modIdentifier, string objectIdentifier)
        {
            this.modIdentifier = modIdentifier;
            this.objectIdentifier = objectIdentifier;
        }

        public override string ToString()
        {
            return $"{modIdentifier}/{objectIdentifier}";
        }
    }
}