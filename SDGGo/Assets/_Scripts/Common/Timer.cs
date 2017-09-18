using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG
{
    public class Timer
    {

        bool _isTicking;
        float _secondTime;
        public int _currentTime;
        int _endTime;
        public delegate void EventHander();
        public event EventHander tickEvent;
        public event EventHander tickSeceondEvent;

        public Timer(int interval)
        {
            _secondTime = 0;
            _currentTime = interval;
            _endTime = interval;
        }

        public void StartTimer()
        {
            _isTicking = true;
        }

        public void ResetTimer()
        {
            _secondTime = 0;
            _currentTime = _endTime;
        }

        public void EndTimer() {
            _isTicking = false;
        }

        public void UpdateTimer() {
            if (_isTicking)
            {
                _secondTime += Time.deltaTime;
            }

            if (_secondTime >= 1.0f)
            {
                tickSeceondEvent();
                _secondTime = 0;
                _currentTime--;
            }

            if (_currentTime < 0)
            {
                tickEvent();
                ResetTimer();
            }
        }

    }
}