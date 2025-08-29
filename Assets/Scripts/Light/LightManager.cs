using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class LightManager : MonoBehaviour
{

    
    [SerializeField] private Vector3 MorningDLPos = new Vector3(320f, 196f, 326f);
    [SerializeField] private Vector3 AfternoonDLPos = new Vector3(320f, 196f, 326f);
    [SerializeField] private Vector3 NightDLPos = new Vector3(320f, 196f, 326f);
    
    [SerializeField] Transform directionalLight;

    public Image buttonImage;
    public Sprite sunImage;
    public Sprite moonImage;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public static LightManager lightManagerInstance;

    public bool isNight = false;
    void Awake()
    {
        isNight = PlayerPrefs.GetInt("IsNight", 0) == 1;
        lightManagerInstance = this;
        Debug.Log($"IsNight: {isNight}");
        directionalLight.rotation = isNight ? Quaternion.Euler(NightDLPos) : Quaternion.Euler(MorningDLPos);
        buttonImage.sprite = isNight ?  sunImage : moonImage;
    }

    public void ChangeDayTime()
    {
        isNight = !isNight;
        PlayerPrefs.SetInt("IsNight", isNight ? 1 : 0);
        directionalLight.DORotate(isNight ? NightDLPos : MorningDLPos, 0.5f);
        buttonImage.sprite = isNight ? sunImage : moonImage;
        
    }

}
