using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Clones a material
    /// </summary>
    public class WeatherMakerMaterialCopy : IDisposable
    {
        private Material source;
        private Material copy;

        /// <summary>
        /// Update to a new material
        /// </summary>
        /// <param name="source">Source material</param>
        public void Update(Material source)
        {
            if (source != this.source)
            {
                Dispose();
                if (source == null)
                {
                    copy = null;
                }
                else
                {
                    copy = new Material(source);
                    copy.name += " (Clone)";
                }
                this.source = source;
            }
        }

        /// <summary>
        /// Dispose of copy, can be called multiple times
        /// </summary>
        public void Dispose()
        {
            if (copy != null)
            {
                GameObject.DestroyImmediate(copy);
                copy = null;
            }
            source = null;
        }

        /// <summary>
        /// Implicit cast to Material
        /// </summary>
        /// <param name="copy">Material</param>
        public static implicit operator Material(WeatherMakerMaterialCopy copy)
        {
            return copy.copy;
        }

        /// <summary>
        /// Original material
        /// </summary>
        public Material Original
        {
            get { return source; }
        }

        /// <summary>
        /// Current clone of material
        /// </summary>
        public Material Copy
        {
            get { return copy; }
        }
    }
}
