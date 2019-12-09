using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleField : MonoBehaviour
{
    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _increment;
    public Vector3 _offset, _offsetSpeed;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
                    float noise = _fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y, zOff + _offset.z) + 1;
                    Gizmos.color = new Color(1, 1, 1, noise * noise * 0.5f);
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 size = new Vector3(1, 1, 1);
                    Gizmos.DrawCube(pos, size);
                    zOff += _increment;
                }
                yOff += _increment;
            }
            xOff += _increment;
        }
    }
}
