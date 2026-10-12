using NUnit.Framework;

public class PlayerMoveStateTests
{
    private FakePlayerContext _context;
    private PlayerState _state;

    [SetUp]
    public void SetUp()
    {
        _context = new FakePlayerContext();
        _state = _context.MoveState;
    }

    [Test]
    public void 진입하면_걷기_애니메이션을_켠다()
    {
        _state.Enter();

        Assert.AreEqual(true, _context.Walking);
    }

    [TestCase(1f)]
    [TestCase(-1f)]
    public void 좌우를_누르고_있으면_그_방향으로_이동하고_전이하지_않는다(float horizontal)
    {
        _context.FakeInput.Horizontal = horizontal;

        _state.Update();

        CollectionAssert.AreEqual(new[] { horizontal }, _context.Moves);
        Assert.IsNull(_context.ChangedTo);
    }

    [Test]
    public void 점프를_누르면_이동하지_않고_Jump로_전이한다()
    {
        _context.FakeInput.Horizontal = 1f;
        _context.FakeInput.JumpPressed = true;

        _state.Update();

        Assert.AreSame(_context.JumpState, _context.ChangedTo);
        Assert.IsEmpty(_context.Moves);
    }

    [Test]
    public void 입력이_없으면_이동하지_않고_Idle로_전이한다()
    {
        _state.Update();

        Assert.AreSame(_context.IdleState, _context.ChangedTo);
        Assert.IsEmpty(_context.Moves);
    }
}
