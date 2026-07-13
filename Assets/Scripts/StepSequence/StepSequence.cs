using System;

public enum StepAdvanceResult
{
    NotStarted,
    Advanced,
    Completed
}

public sealed class StepSequence
{
    private readonly string[] steps;

    public StepSequence(string[] steps)
    {
        this.steps = steps == null ? Array.Empty<string>() : (string[])steps.Clone();
    }

    public int Count => steps.Length;
    public int CurrentIndex { get; private set; } = -1;
    public bool IsStarted => CurrentIndex >= 0;
    public bool IsFirst => IsStarted && CurrentIndex == 0;
    public bool IsLast => IsStarted && CurrentIndex == Count - 1;
    public string Current => IsStarted ? steps[CurrentIndex] : null;

    public bool Start()
    {
        if (Count == 0)
        {
            return false;
        }

        CurrentIndex = 0;
        return true;
    }

    public StepAdvanceResult Advance()
    {
        if (!IsStarted)
        {
            return StepAdvanceResult.NotStarted;
        }

        if (IsLast)
        {
            return StepAdvanceResult.Completed;
        }

        CurrentIndex++;
        return StepAdvanceResult.Advanced;
    }

    public bool Back()
    {
        if (!IsStarted || IsFirst)
        {
            return false;
        }

        CurrentIndex--;
        return true;
    }

    public bool Restart()
    {
        return Start();
    }
}
