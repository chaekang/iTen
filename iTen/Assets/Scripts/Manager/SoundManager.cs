using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class SoundManager : MonoBehaviourPunCallbacks
{
    public static SoundManager Instance { get; private set; }

    private AudioSource audioSource;
    private Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    public LayerMask mosterLayer;
    private List<Vector3> soundPos = new List<Vector3>();

    private bool isFollowing = false;
    private Transform followTarget;

    private float footstepTimer;

    private PhotonView photonView;

    private void Awake()
    {
        photonView=GetComponent<PhotonView>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }

        audioSource = GetComponent<AudioSource>();
        LoadClips();
    }

    // Resources    ִ             ͼ    ųʸ     Ҵ 
    private void LoadClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
        foreach (AudioClip cl in clips)
        {
            soundClips.Add(cl.name, cl);
        }
    }

    public void PlayerFootstep(float interval, string name, Transform sourceTransform)
    {
        if (footstepTimer <= 0f)
        {
            float minSpeed = 1f;
            float maxSpeed = 4f;
            float minInterval = 0.2f;
            float maxInterval = 0.6f;

            float speedRatio = Mathf.InverseLerp(minSpeed, maxSpeed, interval);
            float footstepInterval = Mathf.Lerp(maxInterval, minInterval, speedRatio);

            var footstepClips = soundClips.Where(kvp => kvp.Key.StartsWith(name)).Select(kvp => kvp.Value).ToList();

            if (footstepClips.Count > 0)
            {
                int randomIndex = Random.Range(0, footstepClips.Count);

                // RPC 호출
                photonView.RPC("PlayFootstepRPC", RpcTarget.All, sourceTransform.position, footstepClips[randomIndex].name);
            }

            footstepTimer = footstepInterval;
        }
    }

    [PunRPC]
    public void PlayFootstepRPC(Vector3 position, string clipName)
    {
        // 모든 클라이언트에서 실행
        AudioClip clip = soundClips[clipName];

        audioSource.transform.position = position;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 20f;
        audioSource.volume = 0.4f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.PlayOneShot(clip);
    }


    [PunRPC]
    public void PlaySound(Transform target, float range, string clipKey)
    {
        Debug.Log($"Playing sound '{clipKey}' at {target.position}");
        if (soundClips.ContainsKey(clipKey))
        {
            AudioClip clip = soundClips[clipKey];
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = range;
            audioSource.maxDistance = range * 2;
            audioSource.PlayOneShot(clip);

            followTarget = target;
            isFollowing = true;

            RegisterSoundPosition(target.position);
            StartCoroutine(EmitSoundContinuously(range));
            Invoke(nameof(StopSound), 10f);
        }
        else
        {
            Debug.LogError($"Sound clip with key '{clipKey}' not found!");
        }
    }

    public void StopSound()
    {
        audioSource.Stop();
        isFollowing = false;
        followTarget = null;
    }

    [PunRPC]
    public void EmitSound(Vector3 pos, float range)
    {
        Collider[] hitMonsters = Physics.OverlapSphere(pos, range, mosterLayer);

        foreach (Collider monster in hitMonsters)
        {
            SoundMonster soundMonster = monster.GetComponent<SoundMonster>();
            if (soundMonster != null)
            {
                Debug.Log("EmitSound triggered.");
                soundMonster.OnSoundHeard(pos);
            }
        }
    }

    private IEnumerator EmitSoundContinuously(float range)
    {
        while (isFollowing && followTarget != null)
        {
            EmitSound(followTarget.position, range);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var pos in soundPos)
        {
            Gizmos.DrawWireSphere(pos, 1000f);
        }
    }

    [PunRPC]
    public void RegisterSoundPosition(Vector3 pos)
    {
        Debug.Log("RegisterSoundPos");
        soundPos.Add(pos);
    }

    public void PlayJumpScareSound(string name)
    {
        if (!soundClips.ContainsKey(name))
        {
            Debug.LogWarning($"Sound '{name}' not found in soundClips.");
            return;
        }

        AudioClip clip = soundClips[name];
        float originalVol = audioSource.volume;

        audioSource.volume = Mathf.Clamp(1.5f, 0f, 1f);
        audioSource.PlayOneShot(clip);

        audioSource.volume = originalVol;
    }

    public void PlayGrowlingSound(string name)
    {
        AudioClip clip = soundClips[name];
        audioSource.PlayOneShot(clip);
    }

    public AudioClip GetClip(string name)
    {
        if (soundClips.ContainsKey(name))
        {
            return soundClips[name];
        }
        return null;
    }

    private void Update()
    {
        if (isFollowing && followTarget != null)
        {
            audioSource.transform.position = followTarget.position;
        }

        if (footstepTimer > 0f)
        {
            footstepTimer -= Time.deltaTime;
        }
    }
}
