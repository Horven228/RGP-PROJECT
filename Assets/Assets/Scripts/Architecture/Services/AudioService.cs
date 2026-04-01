using UnityEngine;

/// <summary>
/// Конкретная реализация аудио сервиса
/// </summary>
public class AudioService : IAudioService
{
    private float _masterVolume = 1f;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = _masterVolume;
        }
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
    }
}