namespace echo17.EndlessBook
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The page data used by each page.
    /// Currently this is just a material,
    /// but might be expanded at a later time.
    /// </summary>
    [Serializable]
    public class PageData
    {
        public Material material;

        static public PageData Default()
        {
            return new PageData()
            {
                material = null
            };
        }
    }
}