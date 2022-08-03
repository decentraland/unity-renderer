using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class ECSComponentsGroupShould
    {
        public class Comp1
        {
            public int value;
        }

        public class Comp2
        {
            public int value;
        }

        public class Comp3
        {
            public int value;
        }

        public class Comp4
        {
            public int value;
        }

        private enum CompId
        {
            Comp1, Comp2, Comp3, Comp4
        }

        private ECSComponentsManager componentsManager;

        [SetUp]
        public void SetUp()
        {
            var factory =
                new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>()
                {
                    {
                        (int)CompId.Comp1,
                        ECSComponentsFactory.CreateComponentBuilder(o => (Comp1)o,
                            () => Substitute.For<IECSComponentHandler<Comp1>>())
                    },
                    {
                        (int)CompId.Comp2,
                        ECSComponentsFactory.CreateComponentBuilder(o => (Comp2)o,
                            () => Substitute.For<IECSComponentHandler<Comp2>>())
                    },
                    {
                        (int)CompId.Comp3,
                        ECSComponentsFactory.CreateComponentBuilder(o => (Comp3)o,
                            () => Substitute.For<IECSComponentHandler<Comp3>>())
                    },
                    {
                        (int)CompId.Comp4,
                        ECSComponentsFactory.CreateComponentBuilder(o => (Comp4)o,
                            () => Substitute.For<IECSComponentHandler<Comp4>>())
                    },
                };
            componentsManager = new ECSComponentsManager(factory);
        }

        [Test]
        public void CreateGroupsCorrectly()
        {
            IParcelScene scene0 = Substitute.For<IParcelScene>();
            IParcelScene scene1 = Substitute.For<IParcelScene>();
            IDCLEntity entityWithBothComponents = Substitute.For<IDCLEntity>();
            IDCLEntity entityWithBothComponents2 = Substitute.For<IDCLEntity>();
            IDCLEntity entityWithOneOfTheComponents = Substitute.For<IDCLEntity>();
            IDCLEntity entityWithTheOtherOneOfTheComponents = Substitute.For<IDCLEntity>();

            entityWithBothComponents.entityId.Returns(0);
            entityWithBothComponents2.entityId.Returns(1);
            entityWithOneOfTheComponents.entityId.Returns(2);
            entityWithTheOtherOneOfTheComponents.entityId.Returns(3);

            // create components group
            var compGroup = componentsManager.CreateComponentGroup<Comp1, Comp2>((int)CompId.Comp1, (int)CompId.Comp2);
            Assert.AreEqual(1, componentsManager.componentsGroups.Count);

            // add components for `entityWithBothComponents`
            componentsManager.GetOrCreateComponent((int)CompId.Comp1, scene0, entityWithBothComponents);
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene0, entityWithBothComponents);
            Assert.AreEqual(1, compGroup.group.Count);

            // add just one of the component for `entityWithBothComponents2`
            componentsManager.GetOrCreateComponent((int)CompId.Comp1, scene1, entityWithBothComponents2);
            Assert.AreEqual(1, compGroup.group.Count);

            // add component for the other entities
            componentsManager.GetOrCreateComponent((int)CompId.Comp1, scene1, entityWithOneOfTheComponents);
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene1, entityWithTheOtherOneOfTheComponents);
            Assert.AreEqual(1, compGroup.group.Count);

            // add missing component to  `entityWithBothComponents2`
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene1, entityWithBothComponents2);
            Assert.AreEqual(2, compGroup.group.Count);
        }

        [Test]
        public void ComponentDataKeptUpdatedCorrectly()
        {
            IParcelScene scene0 = Substitute.For<IParcelScene>();
            IDCLEntity entity0 = Substitute.For<IDCLEntity>();
            IDCLEntity entity1 = Substitute.For<IDCLEntity>();

            entity0.entityId.Returns(0);
            entity1.entityId.Returns(1);

            // create components group
            var compGroup = componentsManager.CreateComponentGroup<Comp3, Comp4>((int)CompId.Comp3, (int)CompId.Comp4);

            // add components for `entity0`
            var entity0Comp4 = new Comp4() { value = 42 };
            var entity0Comp3 = new Comp3() { value = 12 };
            componentsManager.DeserializeComponent((int)CompId.Comp4, scene0, entity0, entity0Comp4);
            componentsManager.DeserializeComponent((int)CompId.Comp3, scene0, entity0, entity0Comp3);

            // add components for `entity1`
            var entity1Comp4 = new Comp4() { value = 7473 };
            var entity1Comp3 = new Comp3() { value = 895464 };
            componentsManager.DeserializeComponent((int)CompId.Comp4, scene0, entity1, entity1Comp4);
            componentsManager.DeserializeComponent((int)CompId.Comp3, scene0, entity1, entity1Comp3);

            Assert.AreEqual(2, compGroup.group.Count);
            Assert.AreEqual(entity0Comp3.value, compGroup.group[0].componentData1.model.value);
            Assert.AreEqual(entity0Comp4.value, compGroup.group[0].componentData2.model.value);
            Assert.AreEqual(entity1Comp3.value, compGroup.group[1].componentData1.model.value);
            Assert.AreEqual(entity1Comp4.value, compGroup.group[1].componentData2.model.value);

            // update components
            int entity0Comp4Value = 4335345;
            int entity0Comp3Value = 448989843;
            int entity1Comp4Value = 894782;
            int entity1Comp3Value = 589393;
            componentsManager.DeserializeComponent((int)CompId.Comp4, scene0, entity0, new Comp4() { value = entity0Comp4Value });
            componentsManager.DeserializeComponent((int)CompId.Comp3, scene0, entity0, new Comp3() { value = entity0Comp3Value });
            componentsManager.DeserializeComponent((int)CompId.Comp4, scene0, entity1, new Comp4() { value = entity1Comp4Value });
            componentsManager.DeserializeComponent((int)CompId.Comp3, scene0, entity1, new Comp3() { value = entity1Comp3Value });

            Assert.AreEqual(2, compGroup.group.Count);
            Assert.AreEqual(entity0Comp3Value, compGroup.group[0].componentData1.model.value);
            Assert.AreEqual(entity0Comp4Value, compGroup.group[0].componentData2.model.value);
            Assert.AreEqual(entity1Comp3Value, compGroup.group[1].componentData1.model.value);
            Assert.AreEqual(entity1Comp4Value, compGroup.group[1].componentData2.model.value);
        }
        
        [Test]
        public void RemoveEntityFromGroupCorrectly()
        {
            IParcelScene scene0 = Substitute.For<IParcelScene>();
            IDCLEntity entity0 = Substitute.For<IDCLEntity>();
            IDCLEntity entity1 = Substitute.For<IDCLEntity>();

            entity0.entityId.Returns(0);
            entity1.entityId.Returns(1);

            // create components group
            var compGroup = componentsManager.CreateComponentGroup<Comp1, Comp2>((int)CompId.Comp1, (int)CompId.Comp2);

            // add components for `entity0`
            componentsManager.GetOrCreateComponent((int)CompId.Comp1, scene0, entity0);
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene0, entity0);
            Assert.AreEqual(1, compGroup.group.Count);

            // add components for `entity1`
            componentsManager.GetOrCreateComponent((int)CompId.Comp1, scene0, entity1);
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene0, entity1);
            Assert.AreEqual(2, compGroup.group.Count);

            // remove one component from `entity0`
            componentsManager.RemoveComponent((int)CompId.Comp2, scene0, entity0);
            Assert.AreEqual(1, compGroup.group.Count);
            Assert.AreEqual(entity1, compGroup.group[0].entity);
            
            // re-add component to `entity0`
            componentsManager.GetOrCreateComponent((int)CompId.Comp2, scene0, entity0);
            Assert.AreEqual(2, compGroup.group.Count);
            
            // remove all components from `entity1`
            componentsManager.RemoveAllComponents(scene0, entity1);
            Assert.AreEqual(1, compGroup.group.Count);
            Assert.AreEqual(entity0, compGroup.group[0].entity);
        }        
    }
}