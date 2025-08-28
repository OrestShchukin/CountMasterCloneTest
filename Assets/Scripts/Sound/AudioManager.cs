using System;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] mainMenuMusicList;
    public Sound[] inGameMusicList;

    public static AudioManager instance;

    private Sound selectedMainMenuMusic;
    private Sound selectedInGameMusic;

    private readonly Dictionary<string, Coroutine> stopTimers = new();
    public bool useUnscaledTime = false;   // за бажанням — працювати під час паузи
    public float fadeOutSeconds = 0f;      // 0 = без фейду
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        ActivateSoundList(sounds);
        ActivateSoundList(mainMenuMusicList);
        ActivateSoundList(inGameMusicList);
        
    }

    private void ActivateSoundList(Sound[] sounds)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.spatialBlend = s.spatialBlend;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.outputAudioMixerGroup = s.mixerGroup;
            s.source.loop = s.loop;
        }
    }
    
    void Start()
    {
        StopAllSounds();
        PlayMainMenuMusic();
    }
    
    

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }
        s.source.Play();
    }

    public void PlayWhole(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }

        if (s.source.isPlaying) return;
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }
        s.source.Stop();
    }

    public void PlayForAmountOfTime(string name, float time)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }

        if (!s.source.isPlaying)
            s.source.Play();

        // Якщо вже був таймер для цього звуку — скасовуємо і ставимо новий
        if (stopTimers.TryGetValue(name, out var co))
            StopCoroutine(co);

        stopTimers[name] = StartCoroutine(StopAfterDelay(s, name, time));
    }

    
    private IEnumerator StopAfterDelay(Sound s, string name, float delay)
    {
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
        else                 yield return new WaitForSeconds(delay);

        if (s?.source != null)
        {
            if (fadeOutSeconds > 0f)
                yield return StartCoroutine(FadeOut(s.source, fadeOutSeconds));

            s.source.Stop();
        }
        stopTimers.Remove(name);
    }

    private IEnumerator FadeOut(AudioSource src, float seconds)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < seconds)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, Mathf.Clamp01(t / seconds));
            yield return null;
        }
        src.volume = startVol; // повернемо гучність для наступних відтворень
    }

    
    

    public void PlaySoundAfterDelay(string name, float delay)
    {
        StartCoroutine(PlaySoundAfterDelayIenumerator(name, delay));
    }

    IEnumerator PlaySoundAfterDelayIenumerator(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(name);
    }

    private void StopNow(Sound s)
    {
        s.source.Stop();
    }

    public void StopAllSounds()
    {
        DeactivateSoundList(sounds);
        DeactivateSoundList(mainMenuMusicList);
        DeactivateSoundList(inGameMusicList);

        void DeactivateSoundList(Sound[] sounds)
        {
            foreach (Sound s in sounds)
            {
                s.source.Stop();
            }
        }
    }

    
    // Functions that will be linked to the UI
    public void PlayMainMenuMusic()
    {
        foreach (Sound s in mainMenuMusicList)
        {
            s.source.Stop();
        }
        selectedMainMenuMusic = mainMenuMusicList[Random.Range(0, mainMenuMusicList.Length)];
        selectedMainMenuMusic.source.Play();
    }

    public void PlayInGameMusic()
    {
        StopAllSounds();
        selectedInGameMusic = inGameMusicList[Random.Range(0, inGameMusicList.Length)];
        selectedInGameMusic.source.Play();
    }
    
    

}