using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [Header("Minigame Panel")]
    public GameObject minigamePanel;

    [Header("The Track (horizontal bar background)")]
    public RectTransform trackRect;

    [Header("Fish Icon")]
    public RectTransform fishIcon;
    public float fishErraticism = 120f;
    public float fishDamping = 3.5f;

    [Header("Catch Bar")]
    public RectTransform catchBar;

    [Range(0.1f, 0.5f)]
    public float catchBarWidthFraction = 0.25f;

    public float barRightSpeed = 400f;
    public float barLeftSpeed = 280f;

    [Header("Progress Bar")]
    public Image progressBarFill;
    public float progressFillRate = 0.35f;
    public float progressDrainRate = 0.25f;

    private float _trackWidth;
    private float _fishX;
    private float _fishVelocity;
    private float _barX;
    private float _progress;
    private bool _running;
    private Action<bool> _onComplete;

    public IEnumerator RunMinigame(Action<bool> onComplete)
    {
        _onComplete = onComplete;

        _trackWidth = trackRect.rect.width;
        _fishX = 0.5f;
        _fishVelocity = 0f;
        _barX = 0.5f;
        _progress = 0.3f;
        _running = true;

        ApplySizes();
        minigamePanel.SetActive(true);

        while (_running)
        {
            yield return null;
            TickMinigame(Time.deltaTime);
        }

        minigamePanel.SetActive(false);
    }

    private void TickMinigame(float dt)
    {
        // Fish movement left/right
        float target = UnityEngine.Random.value;
        float spring = (target - _fishX) * fishErraticism;
        _fishVelocity += spring * dt;
        _fishVelocity -= _fishVelocity * fishDamping * dt;
        _fishX += _fishVelocity * dt;
        _fishX = Mathf.Clamp01(_fishX);

        // Catch bar movement
        bool holding = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (holding)
            _barX += barRightSpeed / _trackWidth * dt;
        else
            _barX -= barLeftSpeed / _trackWidth * dt;

        _barX = Mathf.Clamp01(_barX);

        // Overlap check
        float barHalf = catchBarWidthFraction * 0.5f;
        float barMin = _barX - barHalf;
        float barMax = _barX + barHalf;

        bool overlapping = (_fishX >= barMin && _fishX <= barMax);

        if (overlapping)
            _progress += progressFillRate * dt;
        else
            _progress -= progressDrainRate * dt;

        _progress = Mathf.Clamp01(_progress);

        ApplyPositions();

        if (_progress >= 1f)
        {
            _running = false;
            _onComplete?.Invoke(true);
        }
        else if (_progress <= 0f)
        {
            _running = false;
            _onComplete?.Invoke(false);
        }
    }

    private void ApplySizes()
    {
        _trackWidth = trackRect.rect.width;

        Vector2 sd = catchBar.sizeDelta;
        sd.x = _trackWidth * catchBarWidthFraction;
        catchBar.sizeDelta = sd;
    }

    private void ApplyPositions()
    {
        fishIcon.anchoredPosition = new Vector2(
            _fishX * _trackWidth - _trackWidth * 0.5f,
            fishIcon.anchoredPosition.y
        );

        catchBar.anchoredPosition = new Vector2(
            _barX * _trackWidth - _trackWidth * 0.5f,
            catchBar.anchoredPosition.y
        );

        if (progressBarFill != null)
            progressBarFill.fillAmount = _progress;
    }

    private void OnDisable()
    {
        if (_running)
        {
            _running = false;

            if (minigamePanel != null)
                minigamePanel.SetActive(false);
        }
    }
}