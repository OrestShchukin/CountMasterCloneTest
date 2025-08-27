using UnityEngine;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Mixer & Param names")]
    public AudioMixer mixer;              // Підкинь сюди твій Master.mixer
    public string masterParam = "MASTER_VOL";
    public string musicParam  = "MUSIC_VOL";

    [Header("UI (optional)")]
    public Slider masterSlider;           // 0..1
    public Slider musicSlider;            // 0..1
    public Toggle disableMusicToggle;
  
    
    // Мінімум у dB: зазвичай -80dB достатньо тихо, щоб вважати "0"
    const float MIN_DB = -80f;

    private float lastMusicLinear = 1f;
    
    void Start()
    {
        // Завантажимо збережені значення або дефолти
        float m = PlayerPrefs.GetFloat("vol_master", 1f);
        float mu = PlayerPrefs.GetFloat("vol_music",  1f);
        bool musicDisabled = PlayerPrefs.GetInt("music_disabled", 0) == 1;
        
        lastMusicLinear = mu;
        
        ApplyLinear(masterParam, m);
        
        if (musicDisabled)
        {
            // глушимо музику і блокуємо слайдер
            MuteMusicAndLockSlider(true);
        }
        else
        {
            ApplyLinear(musicParam, mu);
            if (musicSlider) musicSlider.interactable = true;
        }


        if (masterSlider) masterSlider.SetValueWithoutNotify(m);
        if (musicSlider)  musicSlider.SetValueWithoutNotify(mu);
        if (disableMusicToggle) disableMusicToggle.SetIsOnWithoutNotify(musicDisabled);
    }

    // Викликати з OnValueChanged у слайдера (0..1)
    public void OnMasterChanged(float linear) {
        ApplyLinear(masterParam, linear);
        PlayerPrefs.SetFloat("vol_master", linear);
    }

    public void OnMusicChanged(float linear) {
        // якщо музика вимкнена тумблером — оновлюємо лише lastMusicLinear і prefs,
        // але до міксера НЕ пишемо (щоб лишалася заглушеною)
        PlayerPrefs.SetFloat("vol_music", linear);
        lastMusicLinear = linear;

        bool musicDisabled = PlayerPrefs.GetInt("music_disabled", 0) == 1;
        if (!musicDisabled)
        {
            ApplyLinear(musicParam, linear);
        }
    }
    
    
    public void OnDisableMusicToggled(bool isDisabled)
    {
        PlayerPrefs.SetInt("music_disabled", isDisabled ? 1 : 0);

        if (isDisabled)
        {
            if (musicSlider) lastMusicLinear = musicSlider.value;
            MuteMusicAndLockSlider(true);
        }
        else
        {
            ApplyLinear(musicParam, lastMusicLinear);
            if (musicSlider) musicSlider.interactable = true;
        }
    }

    private void MuteMusicAndLockSlider(bool lockSlider)
    {
        // повний mute у міксері
        mixer.SetFloat(musicParam, MIN_DB);
        if (musicSlider) musicSlider.interactable = !lockSlider;
    }
    

    // Перетворення 0..1 → dB для міксера
    void ApplyLinear(string param, float linear)
    {
        // Захист від log(0): клемимо низ
        float lin = Mathf.Clamp(linear, 0.0001f, 1f);
        float dB = Mathf.Log10(lin) * 20f;   // 1 → 0dB, 0.5 → ~ -6dB, 0.1 → -20dB, ...
        dB = Mathf.Max(dB, MIN_DB);          // обмежуємо низ
        mixer.SetFloat(param, dB);
    }
}

