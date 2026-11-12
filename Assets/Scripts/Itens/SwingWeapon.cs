using UnityEngine;

public class SwingWeapon : MonoBehaviour, IUsable, ISwingWeapon
{
    public void Use()
    {
        Debug.Log("Swing Swong");
    }
}
