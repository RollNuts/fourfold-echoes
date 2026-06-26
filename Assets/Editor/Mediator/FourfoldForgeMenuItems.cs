using UnityEditor;

namespace FourfoldEchoes.Editor.Mediator
{
    public static class FourfoldForgeMenuItems
    {
        [MenuItem("Tools/FOURFOLD/Forge/Run Sample Command", false, 10)]
        public static void RunSampleCommand()
        {
            FourfoldForgeMediator.Run();
        }

        [MenuItem("Tools/FOURFOLD/D-020/Build And Validate", false, 20)]
        public static void BuildAndValidateD020()
        {
            FourfoldD020SliceSceneBuilder.BuildAndValidate();
        }

        [MenuItem("Tools/FOURFOLD/D-020/Validate Generated Scene", false, 21)]
        public static void ValidateGeneratedD020()
        {
            FourfoldD020SliceSceneBuilder.ValidateGeneratedScene();
        }

        [MenuItem("Tools/FOURFOLD/D-020/Capture Evidence", false, 22)]
        public static void CaptureD020Evidence()
        {
            FourfoldUnityEvidenceCapture.CaptureD020Slice();
        }

        [MenuItem("Tools/FOURFOLD/Legacy Gate A/Build And Validate", false, 90)]
        public static void BuildAndValidateGateA()
        {
            FourfoldUnitySpikeBuilder.BuildAndValidate();
        }

        [MenuItem("Tools/FOURFOLD/Legacy Gate A/Validate Generated Scene", false, 91)]
        public static void ValidateGeneratedGateA()
        {
            FourfoldUnitySpikeBuilder.ValidateGeneratedScene();
        }
    }
}
