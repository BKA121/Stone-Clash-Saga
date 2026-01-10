using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartAtBottom : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GoToBottom());
    }

    IEnumerator GoToBottom()
    {
        yield return new WaitForEndOfFrame();
        GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;
    }
}