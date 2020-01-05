using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent( typeof(ParticleSystem))]
public class ParticleField : MonoBehaviour
{
    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _increment;
    public float _particleFlow;
    public float _waterTurbulence;
    public Vector3 _offset, _offsetSpeed;

    public ParticleSystem particle_system;  // given particle system    
    ParticleSystem.Particle[] m_particle;   // array of particles in the system
    [Range(0.0f,1.0f)]
    public float m_drift;   // the influence of the flow field [0 , 1].

    public bool ShowFlowField;  // illustrate the flowfield by using ondraw gizmos
    public int numParticles;  // the number of particles surrounding the player


    // Start is called before the first frame update
    void Start()
    {
        InitializeIfNeeded();
        spawnParticles();
    }

    // Update is called once per frame
    void Update()
    {
        //particleFieldPosition();
        updateParticles();
        Debug.Log("particle_system position" + particle_system.transform.position);
        Debug.Log("Transform position" + transform.position);
        //int particlesAlive = particle_system.GetParticles(m_particle);
        //_fastNoise = new FastNoise();
        
        //for(int i = 0; i < particlesAlive; i++)
        //{
        //    Vector3 pos = m_particle[i].position;   // get the position of the individual particle
        //    Vector3 fieldPos = curlNoise(new float3(pos.x + Time.realtimeSinceStartup, pos.y, pos.z) * _waterTurbulence); // get the flowfield
        //    Vector3 endPos = pos + (Vector3)math.normalize(fieldPos); // calculate the new direction for the particle based on the flowfield
        //    m_particle[i].velocity += (endPos - pos) * m_drift; // apply the direction to the particle
        //}

        //// Apply the particle changes to the Particle System
        //particle_system.SetParticles(m_particle, particlesAlive);
    }

    //void particleFieldPosition()
    //{
    //    //var emitparam = new ParticleSystem.EmitParams();
    //    //emitparam.position = transform.position;
    //    //particle_system.Emit(emitparam,1);  
    //}

    void updateParticles()
    {
        //var b = new Bounds(particle_system.transform.position, new Vector3(_gridSize.x, _gridSize.y, _gridSize.z) * 2); // generate a boundary check
        int particlesAlive = particle_system.GetParticles(m_particle);
        Debug.Log("particles Alive: " + particlesAlive);
        float dist;
        _fastNoise = new FastNoise();
        for (int i = 0; i < particlesAlive; i++)
        {
            //if (Mathf.Abs(m_particle[i].position.x - transform.position.x) > _gridSize.x || Mathf.Abs(m_particle[i].position.y - transform.position.y) > _gridSize.y || Mathf.Abs(m_particle[i].position.z - transform.position.z) > _gridSize.z)
            dist = Vector3.Distance(m_particle[i].position, transform.position);
            if (dist  > _gridSize.x)
            {
                //m_particle[i].remainingLifetime = 0.0f;
                m_particle[i].lifetime = 0;
                //m_particle[i].color = Color.red;
            }
            else
            {
                Vector3 pos = m_particle[i].position;   // get the position of the individual particle
                Vector3 fieldPos = curlNoise(new float3(pos.x + Time.realtimeSinceStartup, pos.y, pos.z) * _waterTurbulence); // get the flowfield
                Vector3 endPos = pos + (Vector3)math.normalize(fieldPos); // calculate the new direction for the particle based on the flowfield
                //m_particle[i].color = new Color(255, 255, 255, _gridSize.x / dist);
                m_particle[i].velocity = m_drift * m_particle[i].velocity + (endPos - pos) * (1 - m_drift); // apply the direction to the particle
            }
        }

        // Apply the particle changes to the Particle System
        particle_system.SetParticles(m_particle, particlesAlive);

        //if we have less than a set number of particles emit until we are at that number.
        if (particlesAlive < numParticles)
        {
            birthParticles(particlesAlive, numParticles);
        }

    }

    void spawnParticles()
    {
        var emitparam = new ParticleSystem.EmitParams();
        emitparam.position = transform.position + new Vector3(UnityEngine.Random.Range(-_gridSize.x * 0.5f, _gridSize.x * 0.5f),
                                                              UnityEngine.Random.Range(-_gridSize.y * 0.5f, _gridSize.y * 0.5f),
                                                              UnityEngine.Random.Range(-_gridSize.z * 0.5f, _gridSize.z * 0.5f));
        particle_system.Emit(emitparam, 1);

    }

    void birthParticles(int start, int goal)
    {
        for (int i = start; i < goal; start++)
        {
            //var emitparam = new ParticleSystem.EmitParams();
            //emitparam.position = transform.position + new Vector3(UnityEngine.Random.Range(-_gridSize.x * 0.5f, _gridSize.x * 0.5f),
                                                                  //UnityEngine.Random.Range(-_gridSize.y * 0.5f, _gridSize.y * 0.5f),
                                                                  //UnityEngine.Random.Range(-_gridSize.z * 0.5f, _gridSize.z * 0.5f));
            particle_system.Emit(1);
        }
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

    void InitializeIfNeeded()
    {
        if (particle_system == null)
            particle_system = GetComponent<ParticleSystem>();

        if (m_particle == null || m_particle.Length < particle_system.main.maxParticles)
            m_particle = new ParticleSystem.Particle[particle_system.main.maxParticles];
    }

private void OnDrawGizmos()
    {
        if (ShowFlowField)
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
                        float noise = _fastNoise.GetSimplex(xOff + _offset.x + noiseFlow.x, yOff + _offset.y + noiseFlow.y, zOff + _offset.z + noiseFlow.z) + 1;
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
                        Gizmos.DrawLine(pos, endPos);
                        //Gizmos.DrawCube(pos, size);
                        zOff += _increment;
                    }
                    yOff += _increment;
                }
                xOff += _increment;
            }
        }
    }
}
