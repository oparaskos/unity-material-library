using HestiaMaterialImporter;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class HestiaMaterialImporter_Extensions_Texture2DExt
    {
        [Test]
        public void ItShouldInvertImage()
        {
            Texture2D original = Texture2D.whiteTexture;
            foreach (Color color in original.GetPixels())
            {
                Assert.AreEqual(1.0f, color.r);
                Assert.AreEqual(1.0f, color.g);
                Assert.AreEqual(1.0f, color.b);
                Assert.AreEqual(1.0f, color.a);
            }
            Texture2D inverted = original.Inverted();
            foreach(Color color in inverted.GetPixels())
            {
                Assert.AreEqual(0.0f, color.r);
                Assert.AreEqual(0.0f, color.g);
                Assert.AreEqual(0.0f, color.b);
                Assert.AreEqual(1.0f, color.a); // Alpha not inverted
            }
        }
    }
}
