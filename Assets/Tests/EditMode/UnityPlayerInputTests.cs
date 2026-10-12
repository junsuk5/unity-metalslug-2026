using NUnit.Framework;
using UnityEngine.InputSystem;

public class UnityPlayerInputTests : InputTestFixture
{
    private Keyboard _keyboard;
    private UnityPlayerInput _input;

    public override void Setup()
    {
        base.Setup();
        _keyboard = InputSystem.AddDevice<Keyboard>();
        _input = new UnityPlayerInput();
    }

    public override void TearDown()
    {
        _input.Dispose();
        base.TearDown();
    }

    [Test]
    public void 입력이_없으면_Horizontal은_0이다()
    {
        InputSystem.Update();

        Assert.AreEqual(0f, _input.Horizontal);
    }

    [Test]
    public void 왼쪽_화살표를_누르면_Horizontal은_마이너스1이다()
    {
        Press(_keyboard.leftArrowKey);

        Assert.AreEqual(-1f, _input.Horizontal);
    }

    [Test]
    public void 오른쪽_화살표를_누르면_Horizontal은_1이다()
    {
        Press(_keyboard.rightArrowKey);

        Assert.AreEqual(1f, _input.Horizontal);
    }

    [Test]
    public void 왼쪽과_오른쪽을_함께_누르면_왼쪽이_우선한다()
    {
        Press(_keyboard.leftArrowKey);
        Press(_keyboard.rightArrowKey);

        Assert.AreEqual(-1f, _input.Horizontal);
    }

    [Test]
    public void 위_화살표를_누른_업데이트에서만_JumpPressed다()
    {
        Press(_keyboard.upArrowKey);

        Assert.IsTrue(_input.JumpPressed);

        InputSystem.Update();

        Assert.IsFalse(_input.JumpPressed);
    }

    [Test]
    public void Space를_누른_업데이트에서만_AttackPressed다()
    {
        Press(_keyboard.spaceKey);

        Assert.IsTrue(_input.AttackPressed);

        InputSystem.Update();

        Assert.IsFalse(_input.AttackPressed);
    }
}
