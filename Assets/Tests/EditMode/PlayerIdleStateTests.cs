using NUnit.Framework;

public class PlayerIdleStateTests
{
    private FakePlayerContext _context;
    private PlayerState _state;

    [SetUp]
    public void SetUp()
    {
        _context = new FakePlayerContext();
        _state = _context.IdleState;
    }

    [Test]
    public void 진입하면_걷기_애니메이션을_끈다()
    {
        _state.Enter();

        Assert.AreEqual(false, _context.Walking);
    }

    [Test]
    public void 점프를_누르면_Jump로_전이한다()
    {
        _context.FakeInput.JumpPressed = true;

        _state.Update();

        Assert.AreSame(_context.JumpState, _context.ChangedTo);
    }

    [TestCase(1f)]
    [TestCase(-1f)]
    public void 좌우를_누르면_Move로_전이한다(float horizontal)
    {
        _context.FakeInput.Horizontal = horizontal;

        _state.Update();

        Assert.AreSame(_context.MoveState, _context.ChangedTo);
    }

    [Test]
    public void 점프와_좌우를_함께_누르면_Jump가_우선한다()
    {
        _context.FakeInput.JumpPressed = true;
        _context.FakeInput.Horizontal = 1f;

        _state.Update();

        Assert.AreSame(_context.JumpState, _context.ChangedTo);
    }

    [Test]
    public void 입력이_없으면_전이하지_않고_움직이지_않는다()
    {
        _state.Update();

        Assert.IsNull(_context.ChangedTo);
        Assert.IsEmpty(_context.Moves);
    }
}
