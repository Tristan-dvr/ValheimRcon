using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

internal static class ThreadingUtil
{
    internal static IDisposable RunPeriodical(Action action, int periodMilliseconds)
    {
        return new System.Threading.Timer((arg) => action?.Invoke(), null, 0, periodMilliseconds);
    }

    internal static IDisposable RunPeriodicalInSingleThread(Action action, int periodMilliseconds)
    {
        var thread = new Thread((arg) =>
        {
            while (true)
            {
                action?.Invoke();
                Thread.Sleep(periodMilliseconds);
            }
        });
        thread.Start();
        return new DisposableThread(thread);
    }

    internal static void RunInMainThread(Action action)
    {
        MainThreadDispatcher.GetInstante().AddAction(action);
    }

    internal static void RunCoroutine(IEnumerator coroutine)
    {
        MainThreadDispatcher.GetInstante().AddCoroutine(coroutine);
    }

    internal static void RunDelayed(float delay, Action action)
    {
        MainThreadDispatcher.GetInstante().AddCoroutine(DelayedActionCoroutine(delay, action));
    }

    internal static IDisposable RunThread(Action action)
    {
        var thread = new Thread(new ThreadStart(action));
        thread.Start();
        return new DisposableThread(thread);
    }

    internal static IEnumerator DelayedActionCoroutine(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private class DisposableThread : IDisposable
    {
        private Thread _thread;

        internal DisposableThread(Thread thread)
        {
            _thread = thread;
        }

        public void Dispose()
        {
            _thread.Abort();
        }
    }

    private class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;

        public static MainThreadDispatcher GetInstante()
        {
            if (_instance == null)
            {
                var go = new GameObject(nameof(MainThreadDispatcher), typeof(MainThreadDispatcher));
                DontDestroyOnLoad(go);
                _instance = go.GetComponent<MainThreadDispatcher>();
            }
            return _instance;
        }

        private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private ConcurrentQueue<IEnumerator> _coroutinesQueue = new ConcurrentQueue<IEnumerator>();

        public void AddAction(Action action) => _queue.Enqueue(action);

        public void AddCoroutine(IEnumerator coroutine) => _coroutinesQueue.Enqueue(coroutine);

        private void Update()
        {
            while (_queue.Count > 0 && _queue.TryDequeue(out var action))
                action?.Invoke();

            while (_coroutinesQueue.Count > 0 && _coroutinesQueue.TryDequeue(out var coroutine))
                StartCoroutine(coroutine);
        }
    }
}
