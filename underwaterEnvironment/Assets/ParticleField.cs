using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class ParticleField : MonoBehaviour
{
    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _increment;
    public float _particleFlow;
    public float _waterTurbulence;
    public Vector3 _offset, _offsetSpeed;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        time = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        //_fastNoise = new FastNoise();
        //float xOff = 0;
        //for (int x = 0; x < _gridSize.x; x++)
        //{
        //    float yOff = 0;
        //    for (int y = 0; y < _gridSize.y; y++)
        //    {
        //        float zOff = 0;
        //        for (int z = 0; z < _gridSize.z; z++)
        //        {
        //            float noise = _fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y, zOff + _offset.z + time) + 1;
        //            //Vector3 noiseFlow = 
        //            noise = noise * 0.5f; // because the noise goes from -1 to 1 we first att 1 to make it go from0 to 2 and then divide by 2 for [0,1]

        //            float opacity = noise * noise;
        //            if (noise < 0.1) continue; // threshold the noise to only generate particles whre the noise is close to 1

        //            Vector3 pos = new Vector3(x, y, z) + transform.position;
        //            //Vector3 size = new Vector3(0.1, 0.1, 0.1);
        //            Gizmos.DrawSphere(pos, 0.1f);
        //            zOff += _increment;
        //        }
        //        yOff += _increment;
        //    }
        //    xOff += _increment;
        //}
    }

    private float3 curlNoise(float3 p)
    {
        const float e = 0.1f;
        float3 dx = new float3(e, 0.0f, 0.0f);
        float3 dy = new float3(0.0f, e, 0.0f);
        float3 dz = new float3(0.0f, 0.0f, e);

        float3 p_x0 = _fastNoise.GetSimplex((p - dx).x, (p - dx).y, (p - dx).z);
        float3 p_x1 = _fastNoise.GetSimplex((p + dx).x, (p + dx).y, (p + dx).z);
        float3 p_y0 = _fastNoise.GetSimplex((p - dy).x, (p - dy).y, (p - dy).z);
        float3 p_y1 = _fastNoise.GetSimplex((p + dy).x, (p + dy).y, (p + dy).z);
        float3 p_z0 = _fastNoise.GetSimplex((p - dz).x,(p - dz).y,(p - dz).z);
        float3 p_z1 = _fastNoise.GetSimplex((p + dz).x,(p + dz).y,(p + dz).z);

        float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
        float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
        float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

        const float divisor = 1.0f / (2.0f * e);
        return math.normalize(new float3(x, y, z) * divisor);
    }
    private void OnDrawGizmos()
    {
        _fastNoise = new FastNoise();
        float xOff = 0;

        for (int x = 0; x < _gridSize.x; x++)
        {
            float yOff = 0;
            for (int y = 0; y < _gridSize.y; y++)
            {
                float zOff = 0;
                for (int z = 0; z < _gridSize.z; z++)
                {
                    Vector3 noiseFlow = new Vector3(Mathf.Cos(Time.realtimeSinceStartup), Mathf.Sin(Time.realtimeSinceStartup), Mathf.Cos(Time.realtimeSinceStartup));
                    noiseFlow *= _particleFlow;
                    float noise = _fastNoise.GetSimplex(xOff + _offset.x + noiseFlow.x, yOff + _offset.y + noiseFlow.y , zOff + _offset.z + noiseFlow.z ) + 1;
                    noise = noise * 0.5f; // because the noise goes from -1 to 1 we first att 1 to make it go from0 to 2 and then divide by 2 for [0,1]

                    float opacity = noise * noise;
                    if (noise < 0.1) continue; // threshold the noise to only generate particles whre the noise is close to 1

                    Gizmos.color = new Color(1, 1, 1, opacity);
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 fieldPos = curlNoise(new float3(pos.x + Time.realtimeSinceStartup, pos.y, pos.z) * _waterTurbulence);
                    Vector3 endPos = pos + (Vector3)math.normalize(fieldPos);
                    //Vector3 endPos = pos + noiseFlow.normalized;
                    // get a noise that can represent flownoise and then use the vectors to guide the particles in the field.
                    //Vector3 size = new Vector3(1, 1, 1);
                    //Gizmos.DrawSphere(pos, 0.1f);
                    Gizmos.DrawLine(pos,endPos);
                    //Gizmos.DrawCube(pos, size);
                    zOff += _increment;
                }
                yOff += _increment;
            }
            xOff += _increment;
        }
    }
}
