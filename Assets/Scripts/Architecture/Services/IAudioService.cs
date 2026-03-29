/// <summary>
/// Абстракция аудио сервиса
/// </summary>
public interface IAudioService
{
    /// <summary>Текущая громкость (0-1)</summary>
    float MasterVolume { get; set; }

    /// <summary>Установить громкость</summary>
    void SetMasterVolume(float volume);
}