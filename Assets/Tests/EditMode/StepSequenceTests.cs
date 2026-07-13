using NUnit.Framework;

public class StepSequenceTests
{
    [Test]
    public void Start_SelectsFirstStep()
    {
        var sequence = CreateSequence();

        bool started = sequence.Start();

        Assert.That(started, Is.True);
        Assert.That(sequence.CurrentIndex, Is.EqualTo(0));
        Assert.That(sequence.Current, Is.EqualTo("Primeiro"));
        Assert.That(sequence.IsFirst, Is.True);
        Assert.That(sequence.Back(), Is.False);
        Assert.That(sequence.CurrentIndex, Is.EqualTo(0));
    }

    [Test]
    public void Advance_MovesUntilLastStep()
    {
        var sequence = CreateSequence();
        sequence.Start();

        StepAdvanceResult result = sequence.Advance();

        Assert.That(result, Is.EqualTo(StepAdvanceResult.Advanced));
        Assert.That(sequence.CurrentIndex, Is.EqualTo(1));
        Assert.That(sequence.IsLast, Is.True);
    }

    [Test]
    public void Advance_OnLastStep_RequestsCompletionWithoutLeavingRange()
    {
        var sequence = CreateSequence();
        sequence.Start();
        sequence.Advance();

        StepAdvanceResult result = sequence.Advance();

        Assert.That(result, Is.EqualTo(StepAdvanceResult.Completed));
        Assert.That(sequence.CurrentIndex, Is.EqualTo(1));
        Assert.That(sequence.Current, Is.EqualTo("Ultimo"));
    }

    [Test]
    public void Restart_ReturnsToFirstStep()
    {
        var sequence = CreateSequence();
        sequence.Start();
        sequence.Advance();

        bool restarted = sequence.Restart();

        Assert.That(restarted, Is.True);
        Assert.That(sequence.CurrentIndex, Is.EqualTo(0));
        Assert.That(sequence.Current, Is.EqualTo("Primeiro"));
    }

    private static StepSequence CreateSequence()
    {
        return new StepSequence(new[] { "Primeiro", "Ultimo" });
    }
}
