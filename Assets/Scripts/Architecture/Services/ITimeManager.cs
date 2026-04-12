/// <summary>
/// Абстракция управления временем
/// </summary>
public interface ITimeManager
{
    /// <summary>Остановить время (пауза)</summary>
    void StopTime();

    /// <summary>Возобновить время</summary>
    void ResumeTime();

    /// <summary>Остановлено ли время</summary>
    bool IsTimeStopped { get; }
}