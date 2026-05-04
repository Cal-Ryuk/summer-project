using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop instance;
    [SerializeField] private float hitStopTimescale;

    private void Awake()
    {
        instance= this;
    }

    public void Stop(float duration)
    {
        StartCoroutine(DoHitStop(duration));
    }

    private IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = hitStopTimescale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
