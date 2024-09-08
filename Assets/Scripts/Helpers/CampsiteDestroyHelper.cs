using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampsiteDestroyHelper : MonoBehaviour
{
    void Start()
    {
        // Start the coroutine to destroy the object after one second
        StartCoroutine(DestroyObjectAfterDelay(2f));
    }

    IEnumerator DestroyObjectAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Destroy the game object
        Destroy(gameObject);
    }
}
