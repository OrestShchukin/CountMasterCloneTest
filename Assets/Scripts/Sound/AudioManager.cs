using System;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] mainMenuMusicList;
    public Sound[] inGameMusicList;

    public static AudioManager instance;

    private Sound selectedMainMenuMusic;
    private Sound selectedInGameMusic;

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
        {
            s.source.Play();
            Debug.Log("Play method has been called");
        }
        CancelInvoke(nameof(StopNow));
        Invoke(nameof(StopNow), time);
        
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