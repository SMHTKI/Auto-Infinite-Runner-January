using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action OnFinish;
    private void AnimationFinishedTrigger() => OnFinish?.Invoke();
    public event Action OnJumpLanded;
    private void JumpLandedTrigger() => OnJumpLanded?.Invoke();
    public event Action OnJumpStarted;
    private void JumpStartedTrigger() => OnJumpStarted?.Invoke();
    public event Action OnDeathSplash;
    private void DeathSplashTrigger() => OnDeathSplash?.Invoke();

    public event Action OnFinishedRespawning;
    private void FinishedRespawningTrigger() => OnFinishedRespawning?.Invoke();
}
