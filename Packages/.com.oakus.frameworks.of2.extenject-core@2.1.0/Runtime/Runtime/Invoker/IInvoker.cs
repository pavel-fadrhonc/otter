using System;

namespace Zenject
{
    public delegate void InvokerTask(float deltaTime);
    
    /// <summary>
    /// Allows for invoking of any method with interval and delay.
    /// If repeating, reacts to two kinds of pauses: world pause by pausing everything that is IPausable. That pauses all tasks except those that
    /// were spawn with ignoreWorldPause = false. This pause has precedent over task pause.
    /// Task pause is setup for each individual task and can be setup even for those that were spawned with ignoreWorldPause = true
    /// Also when whole Invoker is paused, unpausing tasks that were spawned with ignoreWorldPause = false has no effect.
    /// </summary>
    public interface IInvoker
    {
        int InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0, bool ignoreWorldPause = false);
        int Invoke(InvokerTask task_, float delay_, bool ignorePause);
        void StopInvoke(int taskId, float delay);
        bool HasTask(int taskId);
        void Pause(int taskId);
        void PauseFor(int taskId, float time);
        bool IsPaused(int taskId);
        void Resume(int taskId);
    }
}