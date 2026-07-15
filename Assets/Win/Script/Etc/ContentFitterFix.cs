using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContextFitterFix : MonoBehaviour
{
    private ContentSizeFitter fitter;

    void Awake()
    {
        fitter = GetComponent<ContentSizeFitter>();
    }

    IEnumerator Start()
    {
        while (true)
        {
            if (fitter != null && !fitter.enabled)
                fitter.enabled = true;

            yield return new WaitForSeconds(0.1f);
        }
    }
}