namespace SMLHelper.Tests.AssestClassTests
{
    using System;
    using System.Collections.Generic;
    using NSubstitute;
    using NUnit.Framework;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Interfaces;

    [TestFixture]
    internal class SpawnableTests
    {
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
            var recordedEvents = new List<string>();

            IPrefabHandler mockPrefabHandler = Substitute.For<IPrefabHandler>();
            ISpriteHandler mockSpriteHandler = Substitute.For<ISpriteHandler>();
            ITechTypeHandlerInternal mockTechTypeHandler = Substitute.For<ITechTypeHandlerInternal>();

            mockTechTypeHandler
                .AddTechType(Arg.Any<string>(), Arg.Do<string>((c) => recordedEvents.Add("AddTechType")), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(createdTechType);

            var spawnable = new TestSpawnable(recordedEvents)
            {
                PrefabHandler = mockPrefabHandler,
                SpriteHandler = mockSpriteHandler,
                TechTypeHandler = mockTechTypeHandler
            };

            mockPrefabHandler.RegisterPrefab(Arg.Do<Spawnable>((s) => recordedEvents.Add("RegisterPrefab")));
            mockSpriteHandler.RegisterSprite(Arg.Any<TechType>(), Arg.Do<Atlas.Sprite>((s) => recordedEvents.Add("RegisterSprite")));

            // ACT
            spawnable.Patch();

            // ASSERT
            Assert.AreEqual("OnStartedPatching", recordedEvents[0]);
            Assert.AreEqual("AddTechType", recordedEvents[1]);
            Assert.AreEqual("RegisterPrefab", recordedEvents[2]);
            Assert.AreEqual("RegisterSprite", recordedEvents[3]);
            Assert.AreEqual("OnFinishedPatching", recordedEvents[4]);

            Assert.AreEqual(createdTechType, spawnable.TechType);
            Assert.IsTrue(spawnable.GetItemSpriteInvoked);
            Assert.IsTrue(spawnable.IsPatched);
        }

        private class TestSpawnable : Spawnable
        {
            public bool GetItemSpriteInvoked { get; private set; } = false;

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

            protected override Atlas.Sprite GetItemSprite()
            {
                this.GetItemSpriteInvoked = true;
                return null;
            }
        }
    }
}
