using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITabFader : MonoBehaviour
{
    
    public GameObject[] uiImages;

    
    public float showDelay = 1f;

   
    public float hideDelay = 5f;

    
    public float fadeSpeed = 2f;

    
    private Coroutine activeCoroutine;

    void Awake()
    {
        foreach (GameObject img in uiImages)
        {
            if (img == null) continue;

            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg == null) cg = img.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            img.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            
            if (activeCoroutine != null)
                StopCoroutine(activeCoroutine);

            activeCoroutine = StartCoroutine(ShowThenHide());
        }
    }

    IEnumerator ShowThenHide()
    {
        
        foreach (GameObject img in uiImages)
        {
            if (img == null) continue;
            img.GetComponent<CanvasGroup>().alpha = 0f;
            img.SetActive(false);
        }

        
        yield return new WaitForSeconds(showDelay);

        
        foreach (GameObject img in uiImages)
        {
            if (img == null) continue;
            img.SetActive(true);
            img.GetComponent<CanvasGroup>().alpha = 1f;
        }

        
        yield return new WaitForSeconds(hideDelay);

        
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            float alpha = Mathf.Lerp(1f, 0f, elapsed);

            foreach (GameObject img in uiImages)
            {
                if (img == null) continue;
                CanvasGroup cg = img.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = alpha;
            }

            yield return null;
        }

       
        foreach (GameObject img in uiImages)
        {
            if (img == null) continue;
            img.GetComponent<CanvasGroup>().alpha = 0f;
            img.SetActive(false);
        }
    }
}