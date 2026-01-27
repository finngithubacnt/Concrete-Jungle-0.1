using NUnit.Framework;
using UnityEngine;

public class BuildFloor : MonoBehaviour
{
    public static BuildFloor Instance { get; private set; }

    [Header("Build Progress")]
    public GameObject Log1;
    public bool HasLog1 = false;
    public GameObject Log2;
    public bool HasLog2 = false;
    public GameObject Log3;
    public bool HasLog3 = false;
    public GameObject Log4;
    public bool HasLog4 = false;
    public GameObject Log5;
    public bool HasLog5 = false;
    public bool IsComplete = false;
    [Header("Build Connections")]
    public Transform FrontConnection;
    public Transform BackConnection;
    public Transform LeftConnection;
    public Transform RightConnection;

    [Header("Tranfer Values")]
    public bool hasLogheld = false;
    //public List log1, log2, log3, log4, log5; 
    public GameObject Aim;
    void Start()
    {
        
    }

    void Update()
    {
        Aim = PlayerRaycast.Instance.AimedObject;
        if (hasLogheld && Aim == Log1)
        {

            if (Input.GetKeyDown(KeyCode.E))
            {
                HasLog1 = true;
                hasLogheld = false;
                Log1.SetActive(true);
            }
        }
        if (hasLogheld && Aim == Log2)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HasLog2 = true;
                hasLogheld = false;
                Log2.SetActive(true);
            }
        }
        if (hasLogheld && Aim == Log3)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HasLog3 = true;
                hasLogheld = false;
                Log3.SetActive(true);
            }
        }
        if (hasLogheld && Aim == Log4)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HasLog4 = true;
                hasLogheld = false;
                Log4.SetActive(true);
            }
        }
        if (hasLogheld && Aim == Log5)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HasLog5 = true;
                hasLogheld = false;
                Log5.SetActive(true);
            }
        }
        if (HasLog1 && HasLog2 && HasLog3 && HasLog4 && HasLog5)
        {
            IsComplete = true;
        }

     
    }
}
