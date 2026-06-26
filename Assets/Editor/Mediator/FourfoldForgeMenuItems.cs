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

        [MenuItem("Tools/FOURFOLD/Product/Validate All", false, 15)]
        public static void ValidateProduct()
        {
            FourfoldProductValidator.RunAll();
        }

        [MenuItem("Tools/FOURFOLD/Product/Validate D021 Contract", false, 16)]
        public static void ValidateD021Contract()
        {
            FourfoldD021ProductContractVerifier.VerifyD021Contract();
        }

        [MenuItem("Tools/FOURFOLD/Product/Validate D022 Contract", false, 17)]
        public static void ValidateD022Contract()
        {
            FourfoldD022ProductContractVerifier.VerifyD022Contract();
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

        [MenuItem("Tools/FOURFOLD/Art/P0/Build And Validate", false, 40)]
        public static void BuildAndValidateProductionArtP0()
        {
            FourfoldProductionArtP0Builder.BuildAndValidate();
        }

        [MenuItem("Tools/FOURFOLD/Art/P0/Validate", false, 41)]
        public static void ValidateProductionArtP0()
        {
            FourfoldProductionArtP0Builder.ValidateAssets();
        }

        [MenuItem("Tools/FOURFOLD/Art/P0/Capture Preview", false, 42)]
        public static void CaptureProductionArtP0Preview()
        {
            FourfoldProductionArtP0Builder.CapturePreview();
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
