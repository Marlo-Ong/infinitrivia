using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private List<AudioClip> songs;
    [SerializeField] private List<AudioClip> sfx;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    private bool masterAudioMuted;

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && songs[0] != null)
        {
            musicSource.Stop();
            musicSource.clip = songs[0];
            musicSource.Play();
        }
    }

    public void PlaySFX(int index)
    {
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(sfx[index]);
        }
    }

    public void StopCurrentAudio()
    {
        musicSource.Stop();
        sfxSource.Stop();
    }

    public void MuteAudio()
    {
        masterAudioMuted = true;
        musicSource.mute = true;
        sfxSource.mute = true;
    }

    public void UnmuteAudio()
    {
        masterAudioMuted = false;
        musicSource.mute = false;
        sfxSource.mute = false;
    }

    public void ToggleAudio()
    {
        if (masterAudioMuted)
            UnmuteAudio();
        else
            MuteAudio();
    }

    public void FadeOutMusic(float fadeDuration)
    {
        StartCoroutine(ContinueFadeOutMusic(fadeDuration));
    }

    private IEnumerator ContinueFadeOutMusic(float fadeDuration)
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }
}
