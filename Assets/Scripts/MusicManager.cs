using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    AudioSource _oldSource; // crossfade from
    AudioSource _newSource; // crossfade to
    Coroutine currentCrossfade;

    // TODO : If there's ever a problem where we stack too many crossfade requests, we should make a command queue with song-duration pairs that gets processed in-order

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(_instance);
        _oldSource = gameObject.AddComponent<AudioSource>();
        _oldSource.outputAudioMixerGroup = (Resources.Load("AudioMixer_Main") as AudioMixer).FindMatchingGroups("Music")[0];
        _oldSource.loop = true;
        _oldSource.minDistance = 100000;
        _oldSource.maxDistance = 200000;

        _newSource = gameObject.AddComponent<AudioSource>();
        _newSource.outputAudioMixerGroup = (Resources.Load("AudioMixer_Main") as AudioMixer).FindMatchingGroups("Music")[0];
        _newSource.loop = true;
        _newSource.minDistance = 100000;
        _newSource.maxDistance = 200000;
    }

    public void CrossfadeToSong(float duration, AudioClip newSong)
    {
        if (currentCrossfade != null) StopCoroutine(currentCrossfade);
        currentCrossfade = StartCoroutine(CrossfadeCoroutine(duration, newSong));
    }

    IEnumerator CrossfadeCoroutine(float duration, AudioClip newSong)
    {
        // swizzle sources - we alternate the audiosource back and forth to prevent interruptions
        AudioSource toggle = _oldSource;
        _oldSource = _newSource;
        _newSource = toggle;

        _newSource.clip = newSong;
        _newSource.Play();
        _newSource.volume = 0.0f;
        float timer = 0.0f;
        while (timer < duration)
        {
            _newSource.volume = timer / duration;
            _oldSource.volume = 1 - (timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _newSource.volume = 1.0f;
        _oldSource.volume = 0.0f;
        _oldSource.Stop();
        currentCrossfade = null;
    }
    




    // STATIC NONSENSE
    private static MusicManager _instance;
    public static MusicManager GetMusicManager()
    {
        if (!_instance)
        {
            GameObject newMusicManagerObject = new GameObject();
            newMusicManagerObject.AddComponent<MusicManager>(); // should init
        }
        return _instance;
    }
}
