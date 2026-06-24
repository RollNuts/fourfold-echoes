using UnityEditor;

namespace FourfoldEchoes.Editor.Mediator
{
    public static class FourfoldForgeMenuItems
    {
        [MenuItem("Tools/FOURFOLD/Forge/Run Sample Command", priority = 10)]
        public static void RunSampleCommand()
        {
            FourfoldForgeMediator.Run();
        }

        [MenuItem("Tools/FOURFOLD/Gate A/Build And Validate", priority = 20)]
        public static void BuildAndValidateGateA()
        {
            FourfoldUnitySpikeBuilder.BuildAndValidate();
        }

        [MenuItem("Tools/FOURFOLD/Gate A/Validate Generated Scene", priority = 21)]
        public static void ValidateGeneratedGateA()
        {
            FourfoldUnitySpikeBuilder.ValidateGeneratedScene();
        }
    }
}
