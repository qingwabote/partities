using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace PartityCrateDemo
{
    public class CrateDemoUI : MonoBehaviour
    {
        private const int FINAL_GOLD_COUNT_MIN = 125;
        private const int FINAL_GOLD_COUNT_MAX = 176;

        [SerializeField] private Button _openButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private TextMeshProUGUI _goldCounterText;
        [SerializeField] private float _sequenceTime = 6f;
        [SerializeField] private CrateEmitter[] _particleEmitters;
        [SerializeField] private TimelineAsset _discoveredSequence;
        [SerializeField] private TimelineAsset _openSequence;
        [SerializeField] private TimelineAsset _finalSequence;
        [SerializeField] private TimelineAsset _defaultState;

        private PlayableDirector _director;
        private Coroutine _goldRoutine;
        private Coroutine _idleRoutine;

        private void Start()
        {
            _openButton.onClick.AddListener(OnOpenButton);
            _doneButton.onClick.AddListener(OnDoneButton);
            _director = GetComponent<PlayableDirector>();
            _doneButton.gameObject.SetActive(false);
            PlayDiscovered();
        }

        private void OnDestroy()
        {
            _openButton.onClick.RemoveListener(OnOpenButton);
            _doneButton.onClick.RemoveListener(OnDoneButton);
        }

        private void PlayDiscovered()
        {
            _director.extrapolationMode = DirectorWrapMode.Loop;
            _director.playableAsset = _discoveredSequence;
            _director.Play();
        }

        private void OnOpenButton()
        {
            if (_goldRoutine != null)
            {
                StopCoroutine(_goldRoutine);
                _goldRoutine = null;
            }
            StopIdleRoutine();
            _director.extrapolationMode = DirectorWrapMode.Hold;
            _director.playableAsset = _openSequence;
            _director.Play();
            _goldRoutine = StartCoroutine(CountGold());
        }

        private IEnumerator CountGold()
        {
            var finalGoldCount = Random.Range(FINAL_GOLD_COUNT_MIN, FINAL_GOLD_COUNT_MAX);
            var elapsedTime = 0f;
            while (elapsedTime < _sequenceTime)
            {
                var curGoldCount = elapsedTime / _sequenceTime * finalGoldCount;
                _goldCounterText.text = $"{curGoldCount:N2}";
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _goldCounterText.text = $"<color=yellow>{finalGoldCount:N2}</color>";
            _goldRoutine = null;
        }

        public void StopParticleSystems()
        {
            foreach (var emitter in _particleEmitters)
            {
                emitter.Stop();
            }
        }

        public void BeginFinalSequence()
        {
            _doneButton.Select();
            _director.extrapolationMode = DirectorWrapMode.Loop;
            _director.playableAsset = _finalSequence;
            _director.Play();
        }

        private void OnDoneButton()
        {
            if (_goldRoutine != null)
            {
                StopCoroutine(_goldRoutine);
                _goldRoutine = null;
            }
            _goldCounterText.text = "0.00";
            _director.extrapolationMode = DirectorWrapMode.Hold;
            _director.playableAsset = _defaultState;
            _director.Play();
            StopIdleRoutine();
            _idleRoutine = StartCoroutine(ReturnToIdle());
        }

        private IEnumerator ReturnToIdle()
        {
            yield return new WaitForSeconds((float)_defaultState.duration);
            _idleRoutine = null;
            PlayDiscovered();
        }

        private void StopIdleRoutine()
        {
            if (_idleRoutine == null) return;
            StopCoroutine(_idleRoutine);
            _idleRoutine = null;
        }
    }
}
