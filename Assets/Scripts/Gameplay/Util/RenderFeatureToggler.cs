using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

namespace Util
{

    
    [ExecuteAlways]
    public class RenderFeatureToggler : MonoBehaviour
    {
        public bool pixelate;
 
        private void Update()
        {
            PixelArtFeaturePass.doPixelate = Application.isPlaying || pixelate;
        }
    }
}