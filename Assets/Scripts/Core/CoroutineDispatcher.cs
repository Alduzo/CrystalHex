using System.Collections;
using UnityEngine;

public class CoroutineDispatcher : MonoBehaviour
{
    public static CoroutineDispatcher Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
    
}
