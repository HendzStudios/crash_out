using UnityEngine;

public class CarAudioController : MonoBehaviour
{
    [SerializeField] CarController target;
    [SerializeField] bool useSounds = false;
    [SerializeField] AudioSource carEngineSound;
    [SerializeField] AudioSource tireScreechSound;
    float initialPitch;

    void Start()
    {
        if (carEngineSound) initialPitch = carEngineSound.pitch;
    }

    void Update()
    {
        if (!useSounds || target == null) return;
        var rb = target.GetComponent<Rigidbody>();
        if (rb && carEngineSound && carEngineSound.isActiveAndEnabled)
        {
            float enginePitch = initialPitch + (Mathf.Abs(rb.linearVelocity.magnitude) / 25f);
            carEngineSound.pitch = Mathf.Clamp(enginePitch, 0.5f, 3f);
        }
        bool shouldScreech = target.isDrifting || (target.isTractionLocked && Mathf.Abs(target.carSpeed) > 12f);
        if (tireScreechSound && tireScreechSound.isActiveAndEnabled)
        {
            if (shouldScreech)
            {
                if (!tireScreechSound.isPlaying) tireScreechSound.Play();
            }
            else
            {
                if (tireScreechSound.isPlaying) tireScreechSound.Stop();
            }
        }
    }
}
