﻿using System;
using System.Collections.Generic;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
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
                List<SplatPrototype> splatPrototypes = new List<SplatPrototype>();

                var splatmap = materialToken["Splatmap"].ToObject<float[,,]>();
                var tileSize = materialToken["TileSize"].ToVector2();
                foreach (JToken textureToken in materialToken["Textures"].Children())
                {
                    var name = textureToken["Name"].ToObject<String>();

                    splatPrototypes.Add(new SplatPrototype()
                    {
                        texture = String.IsNullOrEmpty(name) ? defaultTexture : context.CreateOrGetAssetOrResource(name, () => textureToken["Content"].ToTexture()),
                        tileSize = tileSize
                    });
                }

                terrainData.splatPrototypes = splatPrototypes.ToArray();

                terrainData.SetAlphamaps(0, 0, splatmap);
            }


            //finally, create the terrain components
            Terrain terrain = gameObject.AddComponent<Terrain>();
            TerrainCollider collider = gameObject.AddComponent<TerrainCollider>();

            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;
        }
    }
}