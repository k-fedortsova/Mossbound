using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mossbound.Effects
{
    /// <summary>
    /// Makes a 2D light breathe like a real flame: the intensity drifts around its starting value with Perlin noise.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class LightFlicker : MonoBehaviour
    {
        [Tooltip("How far the intensity can move from its starting value, as a fraction of it.")]
        [SerializeField, Range(0f, 1f)] private float amount = 0.2f;
        [SerializeField, Min(0f)] private float speed = 3f;

        private Light2D flameLight;
        private float baseIntensity;
        private float seed;

        private void Awake()
        {
            flameLight = GetComponent<Light2D>();
            baseIntensity = flameLight.intensity;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(seed, Time.time * speed) * 2f - 1f;
            flameLight.intensity = baseIntensity * (1f + noise * amount);
        }
    }
}
