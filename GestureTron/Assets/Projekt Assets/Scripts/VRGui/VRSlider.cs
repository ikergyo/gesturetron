using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRSlider : MonoBehaviour
{
    public delegate void OnBarFilledDelegate();
    public event OnBarFilledDelegate OnBarFilled;

    public float fillTime = 2f;

    private Slider mySlider;
    private float timer;
    private bool gazedAt;
    private Coroutine fillBarRoutine;

    // Start is called before the first frame update
    void Start()
    {
        mySlider = GetComponent<Slider>();
        if (mySlider == null)
        {
            Debug.Log("Please adda  slider to this component");
        }
    }


    public void PointEnter()
    {
        gazedAt = true;
        fillBarRoutine = StartCoroutine(FillBar());
    }

    public void PointExit()
    {
        gazedAt = false;
        if (fillBarRoutine != null)
        {
            StopCoroutine(fillBarRoutine);

        }
        timer = 0f;
        mySlider.value = 0;
    }

    private IEnumerator FillBar()
    {
        timer = 0f;

        while (timer < fillTime)
        {
            timer += Time.deltaTime;

            mySlider.value = timer / fillTime;

            yield return null;

            if (gazedAt)
                continue;

            timer = 0f;
            mySlider.value = 0f;

            yield break;
        }

        if(OnBarFilled != null)
        {
            OnBarFilled();
        }

    }
    
}
