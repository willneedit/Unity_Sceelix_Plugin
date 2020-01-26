﻿using System;
using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Terrain")]
    public class TerrainProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            //if a Terrain already exists, don't overwrite it
            if (gameObject.GetComponent<Terrain>() != null)
                return;

            var heights = jtoken["Heights"].ToObject<float[,]>();
            var resolution = jtoken["Resolution"].ToObject<int>();
            var sizes = jtoken["Size"].ToVector3();

            // AltspaceVR specific: Copy the default material to avoid Altspace using a cut down version
            Material terrainMat = context.CreateOrGetAssetOrResource("Sceelix Terrain Default.mat", () =>
            {
                Shader shad = Shader.Find("Nature/Terrain/Standard");
                Material mat = new Material(shad);
                return mat;
            });

            TerrainData newTerrain = context.CreateOrGetAssetOrResource<TerrainData>("Sceelix Terrain.asset", () =>
            {
                //initialize the terrain data instance and set height data
                //unfortunately unity terrain maps have to be square and the sizes must be powers of 2
                TerrainData terrainData = new TerrainData();

                terrainData.heightmapResolution = resolution;
                terrainData.alphamapResolution = resolution;
                terrainData.size = sizes;
                terrainData.SetHeights(0, 0, heights);

                var materialToken = jtoken["Material"];
                if (materialToken != null)
                {
                    var defaultTexture = Texture2D.whiteTexture.ToMipmappedTexture();
                    List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

                    var tileSize = materialToken["TileSize"].ToVector2();
                    foreach (JToken textureToken in materialToken["Textures"].Children())
                    {
                        var name = textureToken["Name"].ToObject<String>();

                        TerrainLayer layer = context.CreateOrGetAssetOrResource("Layer_" + name + ".asset",
                            () => new TerrainLayer()
                            {
                                diffuseTexture = String.IsNullOrEmpty(name) 
                                    ? defaultTexture
                                    : context.CreateOrGetAssetOrResource(name + ".asset", () => textureToken["Content"].ToTexture()),
                                tileSize = tileSize
                            });

                        terrainLayers.Add(layer);
                    }

                    terrainData.terrainLayers = terrainLayers.ToArray();
                    terrainData.RefreshPrototypes();
                }

                return terrainData;
            });

            

            // UNITY BUG: Doesn't allow terrain assets to be initialized in one go, we need to
            // save and reload the asset first, then we can add the splat map and save it.
            var splatmap = jtoken["Material"]["Splatmap"].ToObject<float[,,]>();
            newTerrain.SetAlphamaps(0, 0, splatmap);
            context.RefreshAssets();

            //finally, create the terrain components
            Terrain terrain = gameObject.AddComponent<Terrain>();
            TerrainCollider collider = gameObject.AddComponent<TerrainCollider>();

            terrain.terrainData = newTerrain;
            terrain.materialTemplate = terrainMat;
            collider.terrainData = newTerrain;
        }
    }
}