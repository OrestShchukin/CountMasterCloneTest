using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private GameObject platform;

    [SerializeField] private float range = 2.75f;
    [SerializeField] private float speed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField, Range(-2.75f, 2.75f)] float startX = -2.75f;

    void Start()
    {
        // Запам'ятовуємо початкову позицію по X
         platform.transform.position =  new Vector3(startX,  platform.transform.position.y, platform.transform.position.z);
         
    }

    void Update()
    {
        // Обчислюємо зсув відносно стартової позиції
        float x = Mathf.PingPong((Time.time + startX) * speed, range * 2) - range;

        // Присвоюємо нову позицію (Y та Z не змінюємо)
        platform.transform.position = new Vector3(x, platform.transform.position.y, platform.transform.position.z);
    }
}
