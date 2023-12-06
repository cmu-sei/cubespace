using UnityEngine;

namespace Systems.Effects
{
    public class TriggerEffects : MonoBehaviour
    {
        [Header("Particle Objects")]
        [SerializeField]
        private ParticleSystem sandstormSystem;
        [SerializeField]
        private ParticleSystem lightningSystem;
        [SerializeField]
        private ParticleSystem flareSystem;

        private ParticleSystem curEffectSystem = null;

        // Update is called once per frame
        void Update()
        {
            // The input detection below is for testing purposes

            if (Input.GetKeyDown(KeyCode.K))
            {
                if (curEffectSystem)
                {
                    curEffectSystem.Stop();
                }
                curEffectSystem = null;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (curEffectSystem)
                {
                    curEffectSystem.Stop();
                }
                curEffectSystem = sandstormSystem;
                curEffectSystem.Play();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (curEffectSystem)
                {
                    curEffectSystem.Stop();
                }
                curEffectSystem = flareSystem;
                curEffectSystem.Play();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                if (curEffectSystem)
                {
                    curEffectSystem.Stop();
                }
                curEffectSystem = lightningSystem;
                curEffectSystem.Play();
            }
        }
    }
}
