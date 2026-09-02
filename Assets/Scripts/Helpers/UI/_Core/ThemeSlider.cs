using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeSlider : MonoBehaviour
{
    [Header("Editor")]
    public Image fillImage;
    public Color flashColor;
    public float fillAnimationDuration = 0.3f;

    public float FillAmount { get; private set; }
    public bool isFlashing { get; private set; }

    private Material fillMaterial;

    private LTDescr animCR;
    private LTDescr flashCR;

    public void SetValue(float val, bool animate = false)
    {
        float lastVal = FillAmount;

        // [TEMP-HACK] Dont animate when losing life
        if (val - lastVal < 0) animate = false;

        if (!animate)
        {
            fillMaterial.SetFloat("_FillAmountX", val);
        }
        else
        {
            if(animCR != null)
                LeanTween.cancel(animCR.uniqueId);

            animCR = LeanTween.value(lastVal, val, fillAnimationDuration).setOnUpdate((float lerp) =>
            {
                SetValue(lerp, false);
            });
        }
        FillAmount = val;
    }

    public void Flash()
    {
        if (!isFlashing)
        {
            isFlashing = true;

            if (flashCR != null)
                LeanTween.cancel(flashCR.uniqueId);

            flashCR = LeanTween.color(fillImage.rectTransform, flashColor, 0.35f).setEaseInQuad().setLoopPingPong(1).setOnComplete(() =>
            {
                isFlashing = false;
            });
        }
    }

    private void Awake()
    {
        fillMaterial = new Material(fillImage.material);
        fillImage.material = fillMaterial;
    }

    private void OnDestroy()
    {
        if (animCR != null)
            LeanTween.cancel(animCR.uniqueId);

        if (flashCR != null)
            LeanTween.cancel(flashCR.uniqueId);

        Destroy(fillMaterial);
    }

}
