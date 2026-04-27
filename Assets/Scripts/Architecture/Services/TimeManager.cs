using UnityEngine;

public class TimeManager : MonoBehaviour, ITimeManager
{
    private static TimeManager _instance;
    public static TimeManager Instance => _instance;

    private float _originalTimeScale = 1f;
    public bool IsTimeStopped { get; private set; }

    private void Awake()
    {
        // Реализация синглтона
        if (_instance == null)
        {
            _instance = this;

            // Регистрируем себя в ServiceLocator
            ServiceLocator.Register<ITimeManager>(this);

            // Не уничтожаем при загрузке новых сцен
            DontDestroyOnLoad(gameObject);


        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopTime()
    {
        if (!IsTimeStopped)
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsTimeStopped = true;
        }
    }

    public void ResumeTime()
    {
        if (IsTimeStopped)
        {
            Time.timeScale = _originalTimeScale;
            IsTimeStopped = false;
        }
    }
}