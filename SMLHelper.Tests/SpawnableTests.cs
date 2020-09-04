namespace SMLHelper.Tests.AssestClassTests
{
    using System;
    using System.Collections.Generic;
    using NSubstitute;
    using NUnit.Framework;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Interfaces;
    using UnityEngine;
#if SUBNAUTICA
    using Sprite = Atlas.Sprite;
#endif

    [TestFixture]
    internal class SpawnableTests
    {
        private List<string> _recordedEvents;
        private IPrefabHandler _mockPrefabHandler;
        private ISpriteHandler _mockSpriteHandler;
        private ITechTypeHandlerInternal _mockTechTypeHandler;
        private ICraftDataHandler _craftDataHandler;
        private TestSpawnable _spawnable;

        [SetUp]
        public void SetupForTests()
        {
            _recordedEvents = new List<string>();
            _mockPrefabHandler = Substitute.For<IPrefabHandler>();
            _mockSpriteHandler = Substitute.For<ISpriteHandler>();
            _mockTechTypeHandler = Substitute.For<ITechTypeHandlerInternal>();
            _craftDataHandler = Substitute.For<ICraftDataHandler>();

            _spawnable = new TestSpawnable(_recordedEvents)
            {
                PrefabHandler = _mockPrefabHandler,
                SpriteHandler = _mockSpriteHandler,
                TechTypeHandler = _mockTechTypeHandler,
                CraftDataHandler = _craftDataHandler
            };
        }

        [TestCase("")]
        [TestCase(null)]
        public void Spawnable_Construct_MissingClassId_Throws(string missingClassId)
        {
            ArgumentException argEx = Assert.Throws<ArgumentException>(() =>
            {
                var badSpawnable = new TestSpawnable(missingClassId, "", "");
            });
        }

        [Test]
        public void Patch_EventsInvokedInCorrectOrder()
        {
            // ARRANGE
            const TechType createdTechType = TechType.Accumulator;

            _mockTechTypeHandler
                .AddTechType(Arg.Any<string>(), Arg.Do<string>((c) => _recordedEvents.Add("AddTechType")), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(createdTechType);

            _mockPrefabHandler.RegisterPrefab(Arg.Do<Spawnable>((s) => _recordedEvents.Add("RegisterPrefab")));
            _mockSpriteHandler.RegisterSprite(Arg.Any<TechType>(), Arg.Do<Sprite>((s) => _recordedEvents.Add("RegisterSprite")));

            // ACT
            _spawnable.Patch();

            // ASSERT
            Assert.AreEqual("OnStartedPatching", _recordedEvents[0]);
            Assert.AreEqual("AddTechType", _recordedEvents[1]);
            Assert.AreEqual("RegisterPrefab", _recordedEvents[2]);
            Assert.AreEqual("RegisterSprite", _recordedEvents[3]);
            Assert.AreEqual("OnFinishedPatching", _recordedEvents[4]);

            Assert.AreEqual(createdTechType, _spawnable.TechType);
            Assert.IsTrue(_spawnable.GetItemSpriteInvoked);
            Assert.IsTrue(_spawnable.IsPatched);
        }

        [Test]
        public void Patch_WhenSizeDifferent_CallsSetItemSize()
        {
            // ARRANGE
            const TechType createdTechType = TechType.Accumulator;

            _mockTechTypeHandler.AddTechType(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
                                .Returns(createdTechType);

            var customSize = new Vector2int(2, 2);
            _spawnable.DifferentSize = customSize;

            _spawnable.Patch();

            Assert.AreEqual(createdTechType, _spawnable.TechType);
            _craftDataHandler.Received(1).SetItemSize(createdTechType, customSize);
            Assert.IsTrue(_spawnable.IsPatched);
        }

        private class TestSpawnable : Spawnable
        {
            public bool GetItemSpriteInvoked { get; private set; } = false;
            public Vector2int? DifferentSize { get; set; }

            public TestSpawnable(List<string> recordedEvents)
                : base("classId", "friendlyName", "description")
            {
                OnStartedPatching = () => { recordedEvents.Add("OnStartedPatching"); };
                OnFinishedPatching = () => { recordedEvents.Add("OnFinishedPatching"); };
            }

            public TestSpawnable(string classId, string friendlyName, string description)
                : base(classId, friendlyName, description)
            {
            }

            public override UnityEngine.GameObject GetGameObject()
            {
                throw new NotImplementedException();
            }

            protected override Sprite GetItemSprite()
            {
                this.GetItemSpriteInvoked = true;
                return null;
            }

            public override Vector2int SizeInInventory
            {
                get
                {
                    if (this.DifferentSize.HasValue)
                        return this.DifferentSize.Value;
                    else
                        return base.SizeInInventory;
                }
            }
        }
    }
}
