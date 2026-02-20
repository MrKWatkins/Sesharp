namespace MrKWatkins.Sesharp.Tests;

public sealed class DisposableTests
{
    [Test]
    public void Dispose_ExecutesActionOnDispose()
    {
        var executed = false;
        var disposable = new Disposable(() => executed = true);
        executed.Should().BeFalse();

        disposable.Dispose();
        executed.Should().BeTrue();
    }

    [Test]
    public void Dispose_ExecutesActionOnlyOnce()
    {
        var executed = 0;
        var disposable = new Disposable(() => executed++);
        executed.Should().Equal(0);

        disposable.Dispose();
        executed.Should().Equal(1);

        disposable.Dispose();
        executed.Should().Equal(1);
    }
}