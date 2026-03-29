using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    private IAudioService _audioService;
    private bool _initialized = false;

    private void Start()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponent<Slider>();

        _audioService = ServiceLocator.Get<IAudioService>();

        if (_audioService != null && volumeSlider != null && !_initialized)
        {
            volumeSlider.value = _audioService.MasterVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            _initialized = true;
            Debug.Log($"[Volume] Инициализирован. Громкость: {_audioService.MasterVolume}");
        }
    }

    public void SetAudioService(IAudioService audioService)
    {
        if (_initialized) return;

        _audioService = audioService;
        if (_audioService != null && volumeSlider != null)
        {
            volumeSlider.value = _audioService.MasterVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            _initialized = true;
            Debug.Log($"[Volume] AudioService внедрен. Громкость: {_audioService.MasterVolume}");
        }
    }

    private void SetVolume(float volume)
    {
        _audioService?.SetMasterVolume(volume);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
    }
}