using UnityEngine;

public class Spring
{
    public Particle particleA;    
    public Particle particleB;    
    public float restLength;      
    public float stiffness;      
    public float damping;        

    public Spring(Particle a, Particle b, float k, float damp)
    {
        particleA = a;
        particleB = b;
        stiffness = k;
        damping = damp;

        // Use initial distance as rest length
        restLength = Vector3.Distance(a.position, b.position);
    }

    // Calculate and apply spring force (Hooke's Law + Damping)
    public void ApplyForce()
    {
        Vector3 delta = particleB.position - particleA.position;
        float currentLength = delta.magnitude;

        if (currentLength == 0) return; // Avoid division by zero

        Vector3 direction = delta / currentLength;

        // Hooke's Law: F = -k * (currentLength - restLength)
        float springForceMag = stiffness * (currentLength - restLength);

        // Damping: F_damp = -damping * (velocity difference along spring direction)
        Vector3 relativeVelocity = particleB.velocity - particleA.velocity;
        float dampingForceMag = damping * Vector3.Dot(relativeVelocity, direction);

        Vector3 totalForce = direction * (springForceMag + dampingForceMag);

        // Apply forces (action-reaction pair)
        particleA.AddForce(totalForce);
        particleB.AddForce(-totalForce);
    }
}