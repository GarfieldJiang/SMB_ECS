using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using COL.UnityGameWheels.Core.Asset;
using COL.UnityGameWheels.Unity;
using UnityEngine;
using UnityEngine.Assertions;

public class AudioManager : MonoBehaviourEx, IManager
{
    [SerializeField]
    private AudioSource[] m_SoundEffectAudioSources = null;

    [SerializeField]
    private AudioSource m_MusicAudioSource = null;

    [SerializeField]
    private string m_MusicExt = ".mp3";

    [SerializeField]
    private string m_SoundExt = ".wav";

    [SerializeField]
    private string m_MusicRootPath = null;

    [SerializeField]
    private string m_SoundEffectRootPath = null;

    [SerializeField]
    private string[] m_MusicFileNames = null;

    [SerializeField]
    private string[] m_SoundEffectFileNames = null;

    private readonly Dictionary<string, IAssetAccessor> m_AssetAccessors = new Dictionary<string, IAssetAccessor>();
    private bool m_HasLoadedAssets;
    private int m_CountToLoad;
    private Action m_OnComplete;

    public void Init()
    {
    }

    public void ShutDown()
    {
        if (m_HasLoadedAssets)
        {
            UnloadAll();
        }
    }

    protected override void OnDestroy()
    {
        ShutDown();
        base.OnDestroy();
    }

    private void Update()
    {
        foreach (var soundEffectAudioSource in m_SoundEffectAudioSources)
        {
            if (soundEffectAudioSource.clip && !soundEffectAudioSource.isPlaying)
            {
                soundEffectAudioSource.clip = null;
            }
        }
    }

    public void LoadAll(System.Action onComplete)
    {
        m_HasLoadedAssets = true;
        m_OnComplete = onComplete;
        m_CountToLoad = m_SoundEffectFileNames.Length + m_MusicFileNames.Length;
        foreach (var path in m_MusicFileNames)
        {
            GameEntry.Instance.Asset.LoadAsset(Path.Combine(m_MusicRootPath, path + m_MusicExt), new LoadAssetCallbackSet
            {
                OnSuccess = OnLoadMusicAssetSuccess,
            }, null);
        }

        foreach (var path in m_SoundEffectFileNames)
        {
            GameEntry.Instance.Asset.LoadAsset(Path.Combine(m_SoundEffectRootPath, path + m_SoundExt), new LoadAssetCallbackSet
            {
                OnSuccess = OnLoadSoundEffectAssetSuccess,
            }, null);
        }
    }

    private void OnLoadMusicAssetFailure(IAssetAccessor assetaccessor, string errormessage, object context)
    {
        throw new NotImplementedException();
    }

    private void OnLoadSoundEffectAssetSuccess(IAssetAccessor assetAccessor, object context)
    {
        Assert.IsNotNull(assetAccessor.AssetPath);
        m_AssetAccessors.Add(Path.GetFileNameWithoutExtension(assetAccessor.AssetPath), assetAccessor);
        if (--m_CountToLoad <= 0)
        {
            m_OnComplete?.Invoke();
        }
    }

    private void OnLoadMusicAssetSuccess(IAssetAccessor assetAccessor, object context)
    {
        Assert.IsNotNull(assetAccessor.AssetPath);
        m_AssetAccessors.Add(Path.GetFileNameWithoutExtension(assetAccessor.AssetPath), assetAccessor);
        if (--m_CountToLoad <= 0)
        {
            m_OnComplete?.Invoke();
        }
    }

    public void UnloadAll()
    {
        if (GameEntry.Instance == null || GameEntry.Instance.Asset == null)
        {
            return;
        }

        foreach (var assetAccessor in m_AssetAccessors.Values)
        {
            GameEntry.Instance.Asset.UnloadAsset(assetAccessor);
        }

        m_AssetAccessors.Clear();
    }


    public void PlayMusic(string musicName, bool loop)
    {
        Assert.IsTrue(m_AssetAccessors.ContainsKey(musicName));
        m_MusicAudioSource.clip = (AudioClip)m_AssetAccessors[musicName].AssetObject;
        m_MusicAudioSource.loop = loop;
        m_MusicAudioSource.Play();
    }

    public void StopMusic()
    {
        m_MusicAudioSource.Stop();
        m_MusicAudioSource.clip = null;
    }

    public void PlaySoundEffect(string soundName)
    {
        Assert.IsTrue(m_AssetAccessors.ContainsKey(soundName));

        var alreadyPlayed = false;
        foreach (var soundEffectAudioSource in m_SoundEffectAudioSources)
        {
            if (soundEffectAudioSource.clip != null && soundEffectAudioSource.clip.name == soundName)
            {
                soundEffectAudioSource.Stop();
                soundEffectAudioSource.Play();
                alreadyPlayed = true;
                break;
            }
        }

        if (alreadyPlayed)
        {
            return;
        }

        foreach (var soundEffectAudioSource in m_SoundEffectAudioSources)
        {
            if (soundEffectAudioSource.clip == null)
            {
                soundEffectAudioSource.clip = (AudioClip)m_AssetAccessors[soundName].AssetObject;
                soundEffectAudioSource.Play();
                break;
            }
        }
    }

    public void StopSoundEffects()
    {
        foreach (var soundEffectAudioSource in m_SoundEffectAudioSources)
        {
            if (soundEffectAudioSource.clip != null)
            {
                soundEffectAudioSource.Stop();
                soundEffectAudioSource.clip = null;
            }
        }
    }

    public bool StopSoundEffect(string soundName)
    {
        foreach (var soundEffectAudioSource in m_SoundEffectAudioSources)
        {
            if (soundEffectAudioSource.clip != null && soundEffectAudioSource.clip.name == soundName)
            {
                soundEffectAudioSource.Stop();
                soundEffectAudioSource.clip = null;
                return true;
            }
        }

        return false;
    }

    public bool IsPlayingSoundEffect(string soundName)
    {
        AudioClip clip;
        return m_SoundEffectAudioSources.Any(source => (clip = source.clip) != null && clip.name == soundName);
    }
}