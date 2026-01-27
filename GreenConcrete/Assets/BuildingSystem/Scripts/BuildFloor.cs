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

    void Start()
    {
        
    }

    void Update()
    {
        if (HasLog1 && HasLog2 && HasLog3 && HasLog4 && HasLog5)
        {
            IsComplete = true;
        }
    }
}
