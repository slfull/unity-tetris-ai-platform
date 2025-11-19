using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextPopAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float popDuration = 0.3f;
    public float stayDuration = 1.0f;
    public float fadeDuration = 0.5f;
    public Vector3 targetScale = new Vector3(1.2f, 1.2f, 1f);

    private TextMeshProUGUI _text;
    private CanvasGroup _cg;
    private Coroutine _activeCoroutine;
    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _cg = GetComponent<CanvasGroup>();
        
        // Start completely hidden
        _cg.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    // The Manager calls this function
    public void Show(string message)
    {
        _text.text = message;

        // If an animation is currently playing, stop it so we can restart cleanly
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
        }
        
        _activeCoroutine = StartCoroutine(PopAndFadeSequence());
    }

    private IEnumerator PopAndFadeSequence()
    {
        // --- PHASE 1: POP UP ---
        _cg.alpha = 1f;
        float timer = 0f;

        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popDuration;
            
            // A custom "Elastic Out" math formula for a bouncy pop
            // (Simulates an Animation Curve in code)
            float bounce = Mathf.Sin(t * Mathf.PI * 0.5f); 
            
            transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, bounce);
            yield return null;
        }
        
        // Snap to standard size briefly
        transform.localScale = _originalScale;

        // --- PHASE 2: WAIT ---
        yield return new WaitForSeconds(stayDuration);

        // --- PHASE 3: FADE OUT ---
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            
            // Fade from 1 to 0
            _cg.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        // Final Cleanup
        _cg.alpha = 0f;
        transform.localScale = Vector3.zero;
        _activeCoroutine = null;
    }
}