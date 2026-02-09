using UnityEngine;

public class TerrainAssignParent : MonoBehaviour
{
    public GameObject parentObject; // Reference to the parent GameObject
    public GameObject me;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parentObject = GameObject.FindWithTag("system"); // Find the parent GameObject by name
        me = this.gameObject; // Get a reference to this GameObject
        me.transform.parent = parentObject.transform; // Assign this GameObject as a child of the parentObject
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
