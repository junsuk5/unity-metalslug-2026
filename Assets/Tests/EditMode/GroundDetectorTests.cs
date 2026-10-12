using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GroundDetectorTests
{
    private GameObject _ground;
    private GameObject _wall;
    private GroundDetector _detector;
    private List<bool> _events;

    [SetUp]
    public void SetUp()
    {
        _ground = new GameObject("Ground") { tag = "Ground" };
        _ground.AddComponent<BoxCollider2D>();
        _wall = new GameObject("Wall");
        _wall.AddComponent<BoxCollider2D>();

        _detector = new GroundDetector();
        _events = new List<bool>();
        _detector.GroundedChanged += _events.Add;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_ground);
        Object.DestroyImmediate(_wall);
    }

    [Test]
    public void 처음에는_땅에_닿아있지_않다()
    {
        Assert.IsFalse(_detector.IsGrounded);
    }

    [Test]
    public void Ground_태그와_닿으면_착지하고_true를_알린다()
    {
        _detector.OnContactEnter(_ground.GetComponent<Collider2D>());

        Assert.IsTrue(_detector.IsGrounded);
        CollectionAssert.AreEqual(new[] { true }, _events);
    }

    [Test]
    public void Ground_태그에서_떨어지면_공중이고_false를_알린다()
    {
        _detector.OnContactEnter(_ground.GetComponent<Collider2D>());

        _detector.OnContactExit(_ground.GetComponent<Collider2D>());

        Assert.IsFalse(_detector.IsGrounded);
        CollectionAssert.AreEqual(new[] { true, false }, _events);
    }

    [Test]
    public void 다른_태그와의_접촉은_무시한다()
    {
        _detector.OnContactEnter(_ground.GetComponent<Collider2D>());

        _detector.OnContactEnter(_wall.GetComponent<Collider2D>());
        _detector.OnContactExit(_wall.GetComponent<Collider2D>());

        Assert.IsTrue(_detector.IsGrounded);
        CollectionAssert.AreEqual(new[] { true }, _events);
    }
}
