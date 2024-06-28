namespace echo17.EndlessBook.Demo02
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// The map spans two page views, but each one
    /// will inherit from this class to get continent hits
    /// </summary>
    public class PageView_Map : PageView
    {
        public TextMesh continentText;

        protected override bool HandleHit(RaycastHit hit, BookActionDelegate action)
        {
            continentText.text = hit.collider.gameObject.name;

            return true;
        }
    }
}
