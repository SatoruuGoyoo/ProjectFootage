using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimationUI : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 12f;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool playOnEnable = false;

    private float _timer;
    private int _currentFrame;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;

        _currentFrame = 0;
        _timer = 0f;
        _isPlaying = true;
        targetImage.enabled = true;
        targetImage.sprite = frames[0];
    }

    public void Stop()
    {
        _isPlaying = false;
        targetImage.enabled = false;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _timer += Time.deltaTime;
        float frameDuration = 1f / frameRate;

        if (_timer < frameDuration) return;

        _timer -= frameDuration;
        _currentFrame++;

        if (_currentFrame >= frames.Length)
        {
            if (loop)
            {
                _currentFrame = 0;
            }
            else
            {
                Stop();
                return;
            }
        }

        targetImage.sprite = frames[_currentFrame];
    }
}