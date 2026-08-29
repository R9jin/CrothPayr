using UnityEngine;


public class WeaponAim : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        transform.rotation = cameraTransform.rotation;
    }
}