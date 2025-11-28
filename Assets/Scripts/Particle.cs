using UnityEngine;



public class Particle
{
    public Vector3 position;     
    public Vector3 velocity;     
    public Vector3 force;         
    public float mass;            
    public bool isFixed;

    public Particle(Vector3 pos, float m = 1f, bool isfixed = false)
    {
        position = pos;
        velocity = Vector3.zero;
        force = Vector3.zero;
        mass = m;
        isFixed = isfixed;
    }


    public void ClearForce()
    {
        force = Vector3.zero;
    }

    public void AddForce(Vector3 f)
    { 
        force += f;
    }
}