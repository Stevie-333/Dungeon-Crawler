using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip backgroundMusicClip;
    private AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioClip playerAttackClip;
    public AudioClip enemyHitClip;
    public AudioClip carrotPickupClip;
    public AudioClip doorOpenClip;
    public AudioClip playerDamageClip;
    public AudioClip enemyDeathClip;
    public AudioClip starPickupClip;

    private float lastPlayerAttackTime;
private float attackCooldown = 0.1f; // 100ms minimum between plays

public void PlayPlayerAttack()
{
    if (Time.time - lastPlayerAttackTime < attackCooldown) return;

    lastPlayerAttackTime = Time.time;
    PlaySFX(playerAttackClip);
}

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Background music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;
        musicSource.Play();
    }

    // ✅ Play SFX without cutting off others
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Create temporary GameObject for this sound
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = Vector3.zero;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.loop = false;
        aSource.Play();

        // Destroy object after clip finishes
        Destroy(tempGO, clip.length);
    }

    // Convenience functions
    //public void PlayPlayerAttack() => PlaySFX(playerAttackClip);
    public void PlayEnemyHit() => PlaySFX(enemyHitClip);
    public void PlayCarrotPickup() => PlaySFX(carrotPickupClip);
    public void PlayDoorOpen() => PlaySFX(doorOpenClip);
    public void PlayPlayerDamage() => PlaySFX(playerDamageClip);
    public void PlayEnemyDeath() => PlaySFX(enemyDeathClip);
    public void PlayStarPickup() => PlaySFX(starPickupClip);
}
