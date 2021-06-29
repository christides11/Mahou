using Mirror;
using UnityEngine;

public class AssetIdentifier : MonoBehaviour
{

    [SerializeField] private NetworkIdentity netId;
    [SerializeField] private string guid;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }
        if (netId == null)
        {
            return;
        }
        if (guid != netId.assetId.ToString())
        {
            guid = netId.assetId.ToString();
        }
    }

    public System.Guid GetGUID()
    {
        return new System.Guid(guid);
    }
}
