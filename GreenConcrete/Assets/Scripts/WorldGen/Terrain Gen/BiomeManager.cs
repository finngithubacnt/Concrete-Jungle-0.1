using UnityEngine;
using System.Collections.Generic;
using UnityEngine.TerrainTools;
public class BiomeManager : MonoBehaviour
{
     public int p = 0;
    [Header("Cordinates")]
    public int x4;
    public int y4;

   /* public Vector3 corner1;
    public Vector3 corner2;
    public Vector3 corner3;
    public Vector3 corner4;
   */
    public Vector3 Center;
    public Vector3 c1;
    public Vector3 c2;
    public Vector3 c3;
    public Vector3 c4;

    public int rndfirst;
    [Header("Refrences")]

    BiomeDefenition biomeDefenitionClass;
    public Color gizmoColor = Color.red;
    public float gizmoRadius = 1f;
    [Header("Transfer")]
    TilePlacerAndSaver tilePlacerAndSaverMono;
    public int tileSize;

    public GameObject[] Centers;
    public int CenterCount = 0;
    [SerializeField] private List<Vector3> Corner1 = new List<Vector3>();
    public void Start()
    {
        tilePlacerAndSaverMono = GetComponent<TilePlacerAndSaver>();
        Debug.Log("BiomeManager started, tilePlacerAndSaverMono reference set: " + (tilePlacerAndSaverMono != null));
    }

    public void Update()
    {
        List<Vector3> Center1 = new List<Vector3>();
        foreach (GameObject center in Centers)
        {
            Center1.Add(center.transform.position);
        }

        tilePlacerAndSaverMono = GetComponent<TilePlacerAndSaver>();
        tileSize = tilePlacerAndSaverMono.TileSize;
        Centers = tilePlacerAndSaverMono.Centers1;
        for (int i = 0; i < Center1.Count; i++)
        {
            //stop infinite loop but continue if more centers are added

            if (i <= CenterCount)
            {
                i += 1;
            }

            if (CenterCount < Centers.Length)
            {

                Center = Centers[i].transform.position;
                c1 = new Vector3(Center.x - tileSize / 2, Center.y, Center.z - tileSize / 2);
                c2 = new Vector3(Center.x + tileSize / 2, Center.y, Center.z - tileSize / 2);
                c3 = new Vector3(Center.x + tileSize / 2, Center.y, Center.z + tileSize / 2);
                c4 = new Vector3(Center.x - tileSize / 2, Center.y, Center.z + tileSize / 2);


                Corner1.Add(c1);
                Corner1.Add(c2);
                Corner1.Add(c3);
                Corner1.Add(c4);

                DrawSquare(Corner1[i]);
                CenterCount+=1;
                p = i;
                //reset center1.count max for next loop
                Debug.Log("CenterCount: " + CenterCount + ", Centers.Length: " + Centers.Length );
            }
            else
            {
                continue;
            }

        }



    }


    public void DrawSquare(Vector3 Corner1)
    {
        gizmoColor = Random.ColorHSV();
        Debug.Log("Drawing square at: " + Corner1);
        Debug.DrawLine(c1, c2, gizmoColor, 90);
        Debug.DrawLine(c2, c3, gizmoColor, 90);
        Debug.DrawLine(c3, c4, gizmoColor, 90);
        Debug.DrawLine(c4, c1, gizmoColor, 90);
        //merge surrounding 4 lines onto this square


    }


}
