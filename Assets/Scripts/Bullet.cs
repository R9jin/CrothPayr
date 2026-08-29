using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Color bulletColor = Color.white;
    private float speed = 40f;

    public void Init(Color color, float bulletSpeed)
    {
        bulletColor = color;
        speed = bulletSpeed;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = bulletColor;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        ColorTarget target = collision.gameObject.GetComponent<ColorTarget>();
        if (target != null)
            target.SetColor(bulletColor);

        Destroy(gameObject);
    }
}