using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Rolling
{
    public class HeightMap
    {
        private float terrainScale;
        private float[,] heights;
        private Vector3 heightmapPosition;
        private float heightmapWidth;
        private float heightmapHeight;

        public HeightMap(float[,] heights, float terrainScale)
        {
            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            this.terrainScale = terrainScale;
            this.heights = heights;
            heightmapWidth = (heights.GetLength(0) - 1) * terrainScale;
            heightmapHeight = (heights.GetLength(1) - 1) * terrainScale;
            heightmapPosition.X = -(heights.GetLength(0) - 1) / 2 * terrainScale;
            heightmapPosition.Z = -(heights.GetLength(1) - 1) / 2 * terrainScale;
        }

        public bool IsOnHeightmap(Vector3 position)
        {
            Vector3 positionOnHeightmap = position - heightmapPosition;
            return (positionOnHeightmap.X > 0 &&
                positionOnHeightmap.X < heightmapWidth &&
                positionOnHeightmap.Z > 0 &&
                positionOnHeightmap.Z < heightmapHeight);
        }

        public float GetHeight(Vector3 position)
        {
            Vector3 positionOnHeightmap = position - heightmapPosition;
            int left = (int)positionOnHeightmap.X / (int)terrainScale;
            int top = (int)positionOnHeightmap.Z / (int)terrainScale;
            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % terrainScale) / terrainScale;
            float zNormalized = (positionOnHeightmap.Z % terrainScale) / terrainScale;
            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                heights[left, top],
                heights[left + 1, top],
                xNormalized);

            float bottomHeight = MathHelper.Lerp(
                heights[left, top + 1],
                heights[left + 1, top + 1],
                xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // position.
            return MathHelper.Lerp(topHeight, bottomHeight, zNormalized);
        }
    }

    public class HeightMapInfoReader : ContentTypeReader<HeightMap>
    {
        protected override HeightMap Read(ContentReader input,
            HeightMap existingInstance)
        {
            float terrainScale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            float[,] heights = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heights[x, z] = input.ReadSingle();
                }
            }
            return new HeightMap(heights, terrainScale);
        }
    }
}