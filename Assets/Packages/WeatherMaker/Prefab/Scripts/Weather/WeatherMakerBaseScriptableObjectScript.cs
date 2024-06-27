//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Base class for Weather Maker scriptable objects
    /// </summary>
    public class WeatherMakerBaseScriptableObjectScript : ScriptableObject
    {
        private bool wasDisabled;

        /// <summary>Whether this object is disabled</summary>
        [Tooltip("Whether this object is disabled")]
        public bool Disabled;

        /// <summary>
        /// Initialize
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Update
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        public virtual void OnDestroy()
        {

        }

        /// <summary>
        /// OnDisable
        /// </summary>
        public virtual void OnDisable()
        {
            wasDisabled = true;
            Disabled = true;
        }

        /// <summary>
        /// OnEnable
        /// </summary>
        public virtual void OnEnable()
        {
            if (wasDisabled)
            {
                wasDisabled = false;
                Disabled = false;
            }
        }
    }
}
