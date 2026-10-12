using UnityEngine;

/// <summary>
/// 플레이어 효과음을 재생합니다.
/// </summary>
public class PlayerAudio
{
    private readonly AudioSource _audioSource;
    private readonly AudioClip _gunfireClip;

    public PlayerAudio(AudioSource audioSource, AudioClip gunfireClip)
    {
        _audioSource = audioSource;
        _gunfireClip = gunfireClip;
    }

    public void PlayGunfire()
    {
        if (_gunfireClip != null)
        {
            _audioSource.PlayOneShot(_gunfireClip);
        }
    }
}
