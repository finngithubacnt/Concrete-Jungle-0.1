using UnityEngine;

public class sworddebug : Behaviour
{
    public void Start()
    {
        Debug.Log("sword debug script started");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Debug.Log("sword debug script loaded");
    }
}

