using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class SectionsControllerShould
    {
        private SectionsController controller;
        private SectionFactory_Mock sectionFactory_Mock;

        [SetUp]
        public void SetUp()
        {
            sectionFactory_Mock = new SectionFactory_Mock();
            controller = new SectionsController(sectionFactory_Mock, null);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void OpenSection()
        {
            bool openCallbackCalled = false;
            SectionBase sectionOpened = null;

            void OnSectionOpen(SectionBase section)
            {
                openCallbackCalled = true;
                sectionOpened = section;
            }

            controller.OnSectionShow += OnSectionOpen;
            controller.OpenSection(SectionsController.SectionId.SCENES_MAIN);

            Assert.IsTrue(openCallbackCalled);
            Assert.IsTrue(sectionFactory_Mock.sectionScenesMain.isVisible);
            Assert.AreEqual(sectionFactory_Mock.sectionScenesMain, sectionOpened);
        }

        [Test]
        public void SwitchOpenSection()
        {
            SectionBase openSection = null;
            SectionBase hiddenSection = null;

            void OnSectionOpen(SectionBase section)
            {
                openSection = section;
            }

            void OnSectionHide(SectionBase section)
            {
                hiddenSection = section;
            }

            controller.OnSectionShow += OnSectionOpen;
            controller.OnSectionHide += OnSectionHide;

            controller.OpenSection(SectionsController.SectionId.SCENES_MAIN);

            Assert.IsTrue(sectionFactory_Mock.sectionScenesMain.isVisible);
            Assert.AreEqual(sectionFactory_Mock.sectionScenesMain, openSection);

            controller.OpenSection(SectionsController.SectionId.SCENES_PROJECT);

            Assert.IsFalse(sectionFactory_Mock.sectionScenesMain.isVisible);
            Assert.IsTrue(sectionFactory_Mock.sectionScenesProjects.isVisible);

            Assert.AreEqual(sectionFactory_Mock.sectionScenesProjects, openSection);
            Assert.AreEqual(sectionFactory_Mock.sectionScenesMain, hiddenSection);
        }
    }

    class SectionFactory_Mock : ISectionFactory
    {
        public SectionBase sectionScenesMain;
        public SectionBase sectionScenesProjects;

        public SectionFactory_Mock()
        {
            sectionScenesMain = new Section_Mock();
            sectionScenesProjects =  new Section_Mock();
        }

        SectionBase ISectionFactory.GetSectionController(SectionsController.SectionId id)
        {
            SectionBase result = null;
            switch (id)
            {
                case SectionsController.SectionId.SCENES_MAIN:
                    result = sectionScenesMain;
                    break;
                case SectionsController.SectionId.SCENES_DEPLOYED:
                    break;
                case SectionsController.SectionId.SCENES_PROJECT:
                    result = sectionScenesProjects;
                    break;
                case SectionsController.SectionId.LAND:
                    break;
            }

            return result;
        }

        class Section_Mock : SectionBase
        {
            public override void SetViewContainer(Transform viewContainer)
            {
            }

            public override void Dispose()
            {
            }

            public override void OnShow()
            {
            }

            public override void OnHide()
            {
            }
        }
    }
}