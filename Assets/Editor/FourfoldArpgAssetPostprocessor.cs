using System.IO;
using UnityEditor;
using UnityEngine;

namespace FourfoldEchoes.Editor
{
    public sealed class FourfoldArpgAssetPostprocessor : AssetPostprocessor
    {
        private static bool InArt(string path, string token)
        {
            return path.Replace('\\', '/').ToLowerInvariant().Contains(token);
        }

        private void OnPreprocessModel()
        {
            var importer = (ModelImporter)assetImporter;
            var path = assetPath.Replace('\\', '/').ToLowerInvariant();

            if (!path.StartsWith("assets/art/"))
            {
                return;
            }

            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.importBlendShapes = path.Contains("/characters/");
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.generateSecondaryUV = path.Contains("/environment/") || path.Contains("/buildings/");

            if (path.Contains("/characters/player/") || path.Contains("/characters/npc/"))
            {
                importer.animationType = ModelImporterAnimationType.Human;
            }
            else if (path.Contains("/enemies/"))
            {
                importer.animationType = ModelImporterAnimationType.Generic;
            }
            else
            {
                importer.animationType = ModelImporterAnimationType.None;
            }
        }

        private void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            var path = assetPath.Replace('\\', '/').ToLowerInvariant();
            var filename = Path.GetFileNameWithoutExtension(path);

            if (!path.StartsWith("assets/art/"))
            {
                return;
            }

            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.ToNearest;

            if (path.Contains("/ui/icons/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = true;
                return;
            }

            if (filename.EndsWith("_n"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = true;
                return;
            }

            if (filename.EndsWith("_orm") || filename.EndsWith("_msk"))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.mipmapEnabled = true;
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = !path.Contains("/vfx/");
            importer.alphaIsTransparency = filename.EndsWith("_emi") || path.Contains("/vfx/");
        }

        private void OnPreprocessAudio()
        {
            var importer = (AudioImporter)assetImporter;
            var path = assetPath.Replace('\\', '/').ToLowerInvariant();

            if (!path.StartsWith("assets/art/audio/"))
            {
                return;
            }

            var settings = importer.defaultSampleSettings;
            if (path.Contains("/bgm/") || path.Contains("/music/"))
            {
                importer.forceToMono = false;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.75f;
            }
            else
            {
                importer.forceToMono = true;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
                settings.quality = 1.0f;
            }

            importer.defaultSampleSettings = settings;
        }
    }
}
