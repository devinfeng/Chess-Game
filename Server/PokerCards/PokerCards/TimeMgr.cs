using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PokerCards
{
    class TimeMgr : ManagerBase<TimeMgr>
    {
        private Dictionary<string, TimerInfo> timerList = new Dictionary<string, TimerInfo>();
        // 获取当前服务器时间
        public long serverTime 
        {
            get
            {
                return (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Ticks; 
            }
        }

        public void AddTimer(string key,TimerInfo info, bool isStart = true)
        {
            if (timerList.ContainsKey(key))
            {
                LogMgr.Warning("已经包含此key的定时器");
            }
            else
            {
                timerList.Add(key, info);
            }
            if (isStart)
                Task.Run(() => timerList[key].StartTimer()).ContinueWith(OnException, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void OnException(Task t)
        {
            Exception e = t.Exception;
            LogMgr.Error("AddTimer Task is Wrong! " + e.Message);
        }

        public void StartTimer(string key)
        {
            if (!timerList.ContainsKey(key))
            {
                LogMgr.Error("没有定时器 " + key);
                return;
            }
            timerList[key].StartTimer();
        }

        public void PauseTimer(string key)
        {
            if (timerList.ContainsKey(key))
                timerList[key].PauseTimer();
        }

        public void CancleTimer(string key)
        {
            if (timerList.ContainsKey(key))
            {
                timerList[key].StopTimer();
                timerList.Remove(key);
            }
        }

        public void CancleTimer(TimerInfo timer)
        {
            foreach (var item in timerList)
            {
                if (item.Value == timer)
                {
                    item.Value.StartTimer();
                    timerList.Remove(item.Key);
                }
            }
        }

        public void StopAllTimer()
        {
            foreach (var item in timerList)
            {
                item.Value.StopTimer();
            }
            timerList.Clear();
        }
    }

    class TimerInfo
    {
        private float duration;
        private Timer mTimer;

        public TimerInfo(float dur, bool isLoop, Action call)
        {
            duration = dur;
            mTimer = new Timer(dur * 1000);
            mTimer.AutoReset = isLoop;
            mTimer.Elapsed += new ElapsedEventHandler((sender, e) => { call(); if (!isLoop) TimeMgr.Instance.CancleTimer(this); });
            mTimer.Enabled = false;
        }

        public void StartTimer()
        {
            mTimer.Start();
        }

        public void PauseTimer()
        {
            mTimer.Stop();
        }

        public void StopTimer()
        {
            mTimer.Close();
        }
    }
}
