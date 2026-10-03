using System.Collections;
using UnityEngine;

namespace JetpackTrailRef
{
    public class JetpackTrailRendererController : MonoBehaviour
    {
        TrailRenderer _trailRenderer;
        float _startWidth;
        Gradient _targetGradient;
        float _timeAlive;
        Coroutine _startCoroutine;

        void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            _startWidth = _trailRenderer.widthMultiplier;
            _targetGradient = _trailRenderer.colorGradient;

            var transparentAlphaKeys = _targetGradient.alphaKeys;
            for (var i = 0; i < transparentAlphaKeys.Length; i++)
            {
                var alphaKey = transparentAlphaKeys[i];
                alphaKey.alpha = 0f;
                transparentAlphaKeys[i] = alphaKey;
            }

            var transparentGradient = new Gradient();
            transparentGradient.SetKeys(_targetGradient.colorKeys, transparentAlphaKeys);
            _trailRenderer.colorGradient = transparentGradient;
        }

        void Start()
        {
            _startCoroutine = StartCoroutine(StartTrailRendererRoutine());
        }

        void Update()
        {
            _timeAlive += Time.deltaTime;
        }

        IEnumerator StartTrailRendererRoutine()
        {
            var lineTime = _trailRenderer.time;
            while (lineTime > 0f)
            {
                var t = (_trailRenderer.time - lineTime) / _trailRenderer.time;
                FadeTrailRenderer(t);

                yield return null;
                lineTime -= Time.deltaTime;
            }
            _trailRenderer.colorGradient = _targetGradient;
        }

        public void EndTrailRenderer()
        {
            if (_startCoroutine != null)
            {
                StopCoroutine(_startCoroutine);
            }
            StartCoroutine(EndTrailRendererRoutine());
        }

        IEnumerator EndTrailRendererRoutine()
        {
            var lineTime = Mathf.Min(_trailRenderer.time, _timeAlive);
            while (lineTime > 0f)
            {
                var t = lineTime / _trailRenderer.time;
                FadeTrailRenderer(t);

                yield return null;
                lineTime -= Time.deltaTime;
            }
        }

        void FadeTrailRenderer(float t)
        {
            _trailRenderer.widthMultiplier = t * _startWidth;

            var curAlphaKeys = _targetGradient.alphaKeys;
            for (var i = 0; i < curAlphaKeys.Length; i++)
            {
                curAlphaKeys[i].alpha = Mathf.Lerp(0f, _targetGradient.alphaKeys[i].alpha, t);
            }

            var tempGradient = new Gradient();
            tempGradient.SetKeys(_targetGradient.colorKeys, curAlphaKeys);
            _trailRenderer.colorGradient = tempGradient;
        }
    }
}
