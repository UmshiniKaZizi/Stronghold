using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{   public float speed =15f;
public float lifeTime = 3f;
 private Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = -transform.right * speed;
        Destroy(gameObject,lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
