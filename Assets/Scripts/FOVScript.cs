using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Animations;

public class FOVScript : MonoBehaviour
{
    // TODO Visualise as mesh

    public EnemyScript enemy;

    public Transform enemyTransform;


    // Start is called before the first frame update
    void Start()
    {

        

    }


    public float distance = 1000;
    public int nH = 15;
    public int nV = 6;
    public float h = 0.6f;
    public float minV = 0f;
    public float maxV = 150f;


    // Update is called once per frame
    void Update()
    {


       // Vector3 up = enemyTransform.up;
        Vector3 right = enemyTransform.right;

        Vector3 fwd = enemyTransform.forward;

        float dh = (h * 2) / nH;
        float dv = (maxV - minV) / nV;

        for(int i = 0; i <= nH; i++)
        {
            float thisH = -h + i * dh;

            for (int j = 0; j <= nV; j++)
            {
                float thisV = maxV - j * dv;

                Vector3 vec = Vector3.Normalize(fwd + right * thisH) * distance;

                Vector3 origin = enemyTransform.position;
                origin.y = thisV;

                int mask = 1 << 10 | 1 << 8;

                RaycastHit hit;
                if (Physics.Raycast(origin, vec, out hit, distance, mask))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.tag == "Wall" || hit.collider.tag == "IndoorWall") Debug.DrawLine(origin, hit.point, Color.gray);
                        else if (hit.collider.tag == "Player")
                        {
                            EventsScript.instance.alertEvent.Invoke();
                            enemy.Alert();

                            Debug.DrawLine(origin, hit.point, Color.red);
                        }
                    }
                }
                else
                {
                    Debug.DrawRay(origin, vec, Color.white);
                }

            }

        }


        //Debug.DrawRay(origin, enemyTransform.up*distance, Color.green);
        //Debug.DrawRay(origin, enemyTransform.right * distance, Color.blue);
        //Debug.DrawRay(origin, -enemyTransform.up * distance, Color.yellow);
        //Debug.DrawRay(origin, -enemyTransform.right * distance, Color.magenta);

        //Vector3 origin = enemyTransform.position;
        //origin.y += 190;

        //Vector3 initVec = enemyTransform.forward;
        //float viewDistance = 2000f;

        //int ignoreEnemyMask = ~(1 << 9);

        //else
        //{
        //    Debug.DrawRay(origin, initVec * viewDistance);

        //}


        //RayVis();
    }

    void RayVis()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float fov = 30f;
        int rayCount = 10;
        Vector3 origin = transform.position;
        float angleIncrease = fov / rayCount;
        float viewDistance = 2000f;

        Vector3 initVec = transform.forward;
        initVec = Quaternion.AngleAxis(-fov / 2, transform.up) * initVec;

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 vertex = Quaternion.AngleAxis(angleIncrease * i, transform.right) * initVec * viewDistance;

            RaycastHit hit;
            if (Physics.Raycast(origin, transform.TransformDirection(vertex), out hit, viewDistance))
            {
                if (hit.collider != null) vertex = hit.point - origin;
            }

            Debug.DrawLine(origin, origin + vertex);

        }
    }

    void MeshVis()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float fov = 30f;
        int rayCount = 10;
        Vector3 origin = transform.position;
        float angle = 0f;
        float angleIncrease = fov / rayCount;
        float viewDistance = 2000f;

        Vector3[] verts = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[verts.Length];
        int[] triangles = new int[rayCount * 3];

        verts[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 vertex = origin + UtilsClass.GetVectorFromAngle(angle) * viewDistance;
            vertex = Quaternion.AngleAxis(-90, Vector3.left) * vertex;
            vertex = Quaternion.AngleAxis(-90 - fov / 2, Vector3.up) * vertex;

            RaycastHit hit;
            if (Physics.Raycast(origin, transform.TransformDirection(vertex), out hit, viewDistance))
            {
                if (hit.collider != null) vertex = hit.point - origin;
            }

            Debug.DrawLine(origin, origin + vertex);

            verts[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            angle -= angleIncrease;
            vertexIndex++;
        }

        mesh.vertices = verts;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
