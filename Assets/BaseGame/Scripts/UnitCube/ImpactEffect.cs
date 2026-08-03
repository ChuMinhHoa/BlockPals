using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;

    public void SetColor(Color color)
    {
        for (var i = 0; i < particles.Length; i++)
        {
            var main = particles[i].main;
            main.startColor = color;
        }
    }
}
