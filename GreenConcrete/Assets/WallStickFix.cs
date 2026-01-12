using UnityEngine;

public class WallStickFix : MonoBehaviour
{
    public float moveCheckDist = 0.1f; // Small distance to check for walls
    public LayerMask wallLayer; // Define which layers count as walls

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
     
        if (Physics.Raycast(transform.position, transform.forward, moveCheckDist, wallLayer))
        {
         
            // Debug.Log("Hit a wall ahead!"); 
        }
    }
}