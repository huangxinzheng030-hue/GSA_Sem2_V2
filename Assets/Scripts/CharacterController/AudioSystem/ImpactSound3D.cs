using UnityEngine;

public class ImpactSound3D : MonoBehaviour
{
    [Header("Impact Sound")]
    public AudioClip[] impactClips;

    [Header("Thresholds")]
    public float minImpactSpeed = 1.5f;
    public float cooldown = 0.15f;

    [Header("3D Audio Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minDistance = 1f;
    public float maxDistance = 15f;

    private float lastPlayTime = -999f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impactClips == null || impactClips.Length == 0) return;
        if (Time.time - lastPlayTime < cooldown) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minImpactSpeed) return;

        lastPlayTime = Time.time;

        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];

        Vector3 hitPoint = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : transform.position;

        PlayClipAtPoint3D(clip, hitPoint);
    }

    private void PlayClipAtPoint3D(AudioClip clip, Vector3 position)
    {
        GameObject go = new GameObject("ImpactSound3D");
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = spatialBlend;   // 1 = ´¿3D
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.playOnAwake = false;

        source.Play();

        Destroy(go, clip.length + 0.1f);
    }
}