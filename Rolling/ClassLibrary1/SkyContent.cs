using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content;

namespace HeightmapPipeline
{
    [ContentSerializerRuntimeType("Rolling.Sky, Rolling")]
    public class SkyContent
    {
        public ModelContent Model;
        public TextureContent Texture;
    }
}