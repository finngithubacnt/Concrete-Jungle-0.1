using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    // Reference to the Prefab you want to spawn. Assign this in the Inspector.
    public GameObject prefabToSpawn;
    public Input Input1;
    // Reference to the desired spawn position (optional, you can use a fixed Vector3).
    public Transform spawnPoint;
    
    // This public function will be called by the Button's OnClick event.
   public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
           
            // Instantiate the prefab at the spawner's position and rotation.
            // Use Quaternion.identity for no rotation.
            if (prefabToSpawn != null && spawnPoint != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            }
            else if (prefabToSpawn != null)
            {
                // If no spawn point is specified, spawn at the world origin (0,0,0)
                Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
            }
        }
        
    }
}
