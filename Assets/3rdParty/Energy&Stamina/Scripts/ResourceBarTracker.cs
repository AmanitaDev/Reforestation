using System;
using System.Collections;
using DefaultNamespace;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[SelectionBase]
public class ResourceBarTracker : MonoBehaviour
{
    [SerializeField] ResourceBarSO resourceBarSO;
    [SerializeField] private Image bar;
    [SerializeField] private TMP_Text resourceValueTextField;


    [Header("Events")] [SerializeField] private UnityEvent barIsFilledUp;
    private float _previousFillAmount;
    private Coroutine _fillRoutine;

    public bool FillWithTime => resourceBarSO.increasigByTime;

    private void OnValidate()
    {
        if (resourceBarSO == null)
            return;
        ConfigureBarShapeAndProperties();
    }

    private void Start()
    {
        TriggerFillAnimation();
    }

    private void OnDestroy()
    {
        resourceBarSO.resourceCurrent = resourceBarSO.resourceDefault;
    }

    private void ConfigureBarShapeAndProperties()
    {
        switch (resourceBarSO.shapeOfBar)
        {
            case ResourceBarSO.ShapeType.RectangleHorizontal:
                bar.fillMethod = Image.FillMethod.Horizontal;
                break;
            case ResourceBarSO.ShapeType.RectangleVertical:
                bar.fillMethod = Image.FillMethod.Vertical;
                break;
            case ResourceBarSO.ShapeType.Circle:
            case ResourceBarSO.ShapeType.Arc:
                bar.fillMethod = Image.FillMethod.Radial360;
                //bar.fillOrigin = (int)Image.Origin360.Top;
                break;
        }

        if (!resourceBarSO.useGradient)
            bar.color = resourceBarSO.barColor;

        UpdateBarAndResourceText();
    }

    private void UpdateBarAndResourceText()
    {
        if (resourceBarSO.resourceMax <= 0)
        {
            bar.fillAmount = 0;
            SetCurrentResourceValueText();
            return;
        }

        float fillAmount;

        if (resourceBarSO.shapeOfBar == ResourceBarSO.ShapeType.Arc)
            fillAmount = CalculateCircularFillAmount();
        else
            fillAmount = (float)resourceBarSO.resourceCurrent / resourceBarSO.resourceMax;

        bar.fillAmount = fillAmount;
        SetCurrentResourceValueText();
    }

    private float CalculateCircularFillAmount()
    {
        float fraction = (float)resourceBarSO.resourceCurrent / resourceBarSO.resourceMax;
        float fillRange = resourceBarSO.endDegreeValue / 360f;

        return fillRange * fraction;
    }

    private void SetCurrentResourceValueText()
    {
        switch (resourceBarSO.howToDisplayValueText)
        {
            case ResourceBarSO.DisplayType.LongValue:
                resourceValueTextField.SetText($"{resourceBarSO.resourceCurrent}/{resourceBarSO.resourceMax}");
                break;
            case ResourceBarSO.DisplayType.ShortValue:
                resourceValueTextField.SetText($"{resourceBarSO.resourceCurrent}");
                break;
            case ResourceBarSO.DisplayType.Percentage:
                float percentage = ((float)resourceBarSO.resourceCurrent / resourceBarSO.resourceMax) * 100;
                resourceValueTextField.SetText($"{Mathf.RoundToInt(percentage)} %");
                break;
            case ResourceBarSO.DisplayType.None:
                resourceValueTextField.SetText(string.Empty);
                break;
        }
    }
    
    public bool ChangeResourceByAmount(float amount)
    {
        if (resourceBarSO.resourceCurrent + amount < 0)
            return false;

        resourceBarSO.resourceCurrent += amount;
        resourceBarSO.resourceCurrent = Mathf.Clamp(resourceBarSO.resourceCurrent, 0, resourceBarSO.resourceMax);

        TriggerFillAnimation();

        return true;
    }

    private void TriggerFillAnimation()
    {
        float targetFill = CalculateTargetFill();

        if (Mathf.Approximately(bar.fillAmount, targetFill))
            return;

        if (_fillRoutine != null)
            StopCoroutine(_fillRoutine);

        _fillRoutine = StartCoroutine(SmoothlyTransitionToNewValue(targetFill));
        SetCurrentResourceValueText();
    }

    private float CalculateTargetFill()
    {
        if (resourceBarSO.shapeOfBar == ResourceBarSO.ShapeType.Arc)
            return CalculateCircularFillAmount();

        return (float)resourceBarSO.resourceCurrent / resourceBarSO.resourceMax;
    }

    private IEnumerator SmoothlyTransitionToNewValue(float targetFill)
    {
        float originalFill = bar.fillAmount;
        float elapsedTime = 0.0f;

        while (elapsedTime < resourceBarSO._animationTime)
        {
            elapsedTime += Time.deltaTime;
            float time = elapsedTime / resourceBarSO._animationTime;
            bar.fillAmount = Mathf.Lerp(originalFill, targetFill, time);

            UseGradient();

            yield return null;
        }

        bar.fillAmount = targetFill;

        HandleEvent();
        _previousFillAmount = bar.fillAmount;
    }

    private void UseGradient()
    {
        if (!resourceBarSO.useGradient)
            return;

        if (resourceBarSO.shapeOfBar == ResourceBarSO.ShapeType.Arc)
        {
            float fillRange = bar.fillAmount / (resourceBarSO.endDegreeValue / 360f);
            bar.color = resourceBarSO.barGradient.Evaluate(fillRange);
            return;
        }

        bar.color = resourceBarSO.barGradient.Evaluate(bar.fillAmount);
    }

    private void HandleEvent()
    {
        if (_previousFillAmount >= 1)
            return;

        if (bar.fillAmount >= 1)
            barIsFilledUp?.Invoke();
    }

    public void ChangeMaxAmountTo(int newMaxAmount)
    {
        newMaxAmount = Mathf.Clamp(newMaxAmount, 0, resourceBarSO.resourceAbsoluteMax);

        resourceBarSO.resourceMax = newMaxAmount;
        resourceBarSO.resourceCurrent = Mathf.Clamp(resourceBarSO.resourceCurrent, 0, resourceBarSO.resourceMax);

        TriggerFillAnimation();
    }
}