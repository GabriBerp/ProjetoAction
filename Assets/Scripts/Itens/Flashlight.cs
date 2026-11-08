using UnityEngine;

public class Flashlight : MonoBehaviour, IUsable, IHoldListener
{
    [SerializeField] private Light lightSource;

    public void Use() => lightSource.enabled = !lightSource.enabled;

    public void OnPickedUp() { }
    public void OnDropped() { }
}