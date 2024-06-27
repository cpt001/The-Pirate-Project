namespace echo17.EndlessBook.Demo02
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// The left page view handles all of the dragging logic, even though both pages will drag
    /// </summary>
    public class PageView_Map_Left : PageView_Map
    {
        /// <summary>
        /// The direction we are scrolling
        /// </summary>
        protected Vector3 scrollerDirection;

        /// <summary>
        /// How much timer is remaining for inertia slowdown
        /// </summary>
        protected float inertiaTimeRemaining;

        /// <summary>
        /// The transform of the map object
        /// </summary>
        public Transform mapTransform;

        /// <summary>
        /// How much inertia should we impart when the mouse is released
        /// </summary>
        public Vector2 inertiaMoveFactor;

        /// <summary>
        /// How much position offset should occur while dragging
        /// </summary>
        public Vector2 dragMoveFactor;

        /// <summary>
        /// How long should inertia occur after the mouse is released
        /// </summary>
        public float inertiaTime;

        /// <summary>
        /// The bounds of the map so we don't drag too far
        /// </summary>
        public Vector4 mapBounds;

        /// <summary>
        /// Called every frame
        /// </summary>
        protected void Update()
        {
            // if there is some inertia left
            if (inertiaTimeRemaining > 0)
            {
                // decrement the inertia timer and move the map
                inertiaTimeRemaining -= Time.deltaTime;
                IncrementPosition(new Vector3(scrollerDirection.x * inertiaMoveFactor.x, scrollerDirection.y * inertiaMoveFactor.y, 0) * inertiaTimeRemaining * Time.deltaTime);
            }
        }

        /// <summary>
        /// Called when the mouse is clicked
        /// </summary>
        public override void TouchDown()
        {
            // stop the inertia
            inertiaTimeRemaining = 0;
        }

        /// <summary>
        /// Called when a drag event occurs on the touchpad
        /// </summary>
        /// <param name="increment">Amount of offset since last frame</param>
        /// <param name="useInertia">Use inertia or not</param>
        public override void Drag(Vector2 increment, bool useInertia)
        {
            if (useInertia)
            {
                // using inertia, so we set the timer
                inertiaTimeRemaining = inertiaTime;
            }
            else
            {
                // no inertia, just move the map
                scrollerDirection = increment;
                inertiaTimeRemaining = 0;
                IncrementPosition(new Vector3(scrollerDirection.x * dragMoveFactor.x, scrollerDirection.y * dragMoveFactor.y, 0));
            }
        }

        /// <summary>
        /// Moves the map
        /// </summary>
        /// <param name="amount">Amount to move</param>
        protected virtual void IncrementPosition(Vector3 amount)
        {
            var position = mapTransform.localPosition + amount;

            if (position.x < mapBounds.x) position.x = mapBounds.x;
            if (position.x > mapBounds.y) position.x = mapBounds.y;
            if (position.y < mapBounds.z) position.y = mapBounds.z;
            if (position.y > mapBounds.w) position.y = mapBounds.w;

            mapTransform.localPosition = position;
        }
    }
}
