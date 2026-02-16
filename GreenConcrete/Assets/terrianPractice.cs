using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.TerrainTools;

public class terrianPractice : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Terrain terrain;
    void Start()
    {
        terrain = GetComponent<Terrain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
           
        }
    }
}
