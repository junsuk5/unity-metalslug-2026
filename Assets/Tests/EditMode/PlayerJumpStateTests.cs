using NUnit.Framework;

public class PlayerJumpStateTests
{
    private FakePlayerContext _context;
    private PlayerState _state;

    [SetUp]
    public void SetUp()
    {
        _context = new FakePlayerContext();
        _state = _context.JumpState;
    }

    [Test]
    public void 진입하면_점프한다()
    {
        _state.Enter();

        Assert.AreEqual(1, _context.JumpCount);
    }

    [Test]
    public void 점프_직후_아직_땅에_닿아있으면_착지로_보지_않는다()
    {
        _state.Enter();
        _context.IsGrounded = true;

        _state.Update();

        Assert.IsNull(_context.ChangedTo);
    }

    [Test]
    public void 공중에_있는_동안은_전이하지_않는다()
    {
        _state.Enter();
        _context.IsGrounded = false;

        _state.Update();
        _state.Update();

        Assert.IsNull(_context.ChangedTo);
    }

    [Test]
    public void 땅을_떠났다가_입력_없이_착지하면_Idle로_전이한다()
    {
        _state.Enter();
        _context.IsGrounded = false;
        _state.Update();

        _context.IsGrounded = true;
        _state.Update();

        Assert.AreSame(_context.IdleState, _context.ChangedTo);
    }

    [Test]
    public void 땅을_떠났다가_좌우를_누른_채_착지하면_Move로_전이한다()
    {
        _state.Enter();
        _context.IsGrounded = false;
        _state.Update();

        _context.FakeInput.Horizontal = 1f;
        _context.IsGrounded = true;
        _state.Update();

        Assert.AreSame(_context.MoveState, _context.ChangedTo);
    }

    [Test]
    public void 다시_진입하면_땅을_떠났는지_여부를_초기화한다()
    {
        _state.Enter();
        _context.IsGrounded = false;
        _state.Update();

        _context.IsGrounded = true;
        _state.Enter();
        _state.Update();

        Assert.IsNull(_context.ChangedTo);
    }

    [TestCase(1f)]
    [TestCase(-1f)]
    public void 공중에서_좌우를_누르면_이동하고_걷기_애니메이션을_켠다(float horizontal)
    {
        _state.Enter();
        _context.IsGrounded = false;
        _context.FakeInput.Horizontal = horizontal;

        _state.Update();

        CollectionAssert.AreEqual(new[] { horizontal }, _context.Moves);
        Assert.AreEqual(true, _context.Walking);
    }

    [Test]
    public void 공중에서_입력이_없으면_이동하지_않고_걷기_애니메이션을_끈다()
    {
        _state.Enter();
        _context.IsGrounded = false;

        _state.Update();

        Assert.IsEmpty(_context.Moves);
        Assert.AreEqual(false, _context.Walking);
    }
}
