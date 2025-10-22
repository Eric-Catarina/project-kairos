// Local: Assets/Scripts/Core/LevelCompletionEvent.cs
using System;

public class LevelCompletionEvent
{
    public event Action<float> OnLevelCompleted;

    public void Invoke(float time, Rank rank)
    {
        OnLevelCompleted?.Invoke(time);
    }
}