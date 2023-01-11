namespace echo17.EndlessBook.Demo02
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Simple touch pad colliders that handle input on the book pages.
    /// This is a crude component that should probably be replaced with something
    /// more sophisticated for your projects, but is sufficient for this demo.
    /// </summary>
    public class TouchPad : MonoBehaviour
    {
        /// <summary>
        /// Touchpad collider names
        /// </summary>
        protected const string PageLeftColliderName = "Page Left";
        protected const string PageRightColliderName = "Page Right";
        protected const string TableOfContentsColliderName = "TableOfContents Button";

        /// <summary>
        /// The minimum amount the mouse needs to move to be considered a drag event
        /// </summary>
        protected const float DragThreshold = 0.007f;

        /// <summary>
        /// The size of each page collider
        /// </summary>
        protected Rect[] pageRects;

        /// <summary>
        /// Whether we have touched down on the pad
        /// </summary>
        protected bool touchDown;

        /// <summary>
        /// The position if we have touched down
        /// </summary>
        protected Vector2 touchDownPosition;

        /// <summary>
        /// The last drag position used to calculate the increment between frames
        /// </summary>
        protected Vector2 lastDragPosition;

        /// <summary>
        /// Whether we are dragging
        /// </summary>
        protected bool dragging;

        /// <summary>
        /// One of two pages
        /// </summary>
        public enum PageEnum
        {
            Left,
            Right
        }

        /// <summary>
        /// The demo camera
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        /// The colliders for each page
        /// </summary>
        public Collider[] pageColliders;

        /// <summary>
        /// The upper left "button" used to go back to the table of contents
        /// </summary>
        public Collider tableOfContentsCollider;

        /// <summary>
        /// The mask of the touchpad colliders
        /// </summary>
        public LayerMask pageTouchPadLayerMask;

        /// <summary>
        /// Handler for when a touch down is detected
        /// </summary>
        public Action<PageEnum, Vector2> touchDownDetected;

        /// <summary>
        /// Handler for when a touch up is detected
        /// </summary>
        public Action<PageEnum, Vector2, bool> touchUpDetected;

        /// <summary>
        /// Handler for when a drag is detected
        /// </summary>
        public Action<PageEnum, Vector2, Vector2, Vector2> dragDetected;

        /// <summary>
        /// Handler for when the table of contents "button" is clicked
        /// </summary>
        public Action tableOfContentsDetected;

        void Awake()
        {
            // set up collider rects

            pageRects = new Rect[2];
            for (var i = 0; i < 2; i++)
            {
                pageRects[i] = new Rect(pageColliders[i].bounds.min.x, pageColliders[i].bounds.min.z, pageColliders[i].bounds.size.x, pageColliders[i].bounds.size.z);
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // left mouse button pressed
                DetectTouchDown(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                // left mouse button un-pressed
                DetectTouchUp(Input.mousePosition);
            }
            else if (touchDown && Input.GetMouseButton(0))
            {
                // dragging
                DetectDrag(Input.mousePosition);
            }
        }

        /// <summary>
        /// Turn a page collider on or off.
        /// Useful if we are in a state of the book that cannot handle one of the colliders,
        /// like ClosedFront cannot handle a left page interaction.
        /// </summary>
        /// <param name="page">The page collider to toggle</param>
        /// <param name="on">Whether to toggle on</param>
        public virtual void Toggle(PageEnum page, bool on)
        {
            // activate or deactive the collider
            pageColliders[(int)page].gameObject.SetActive(on);
        }

        /// <summary>
        /// Turn the table of contents collider on or off.
        /// Useful for when we are turning pages: we should turn it off,
        /// turning it back on when the turn has completed.
        /// </summary>
        /// <param name="on">Whether to toggle on</param>
        public virtual void ToggleTableOfContents(bool on)
        {
            // activate or deactive the collider
            tableOfContentsCollider.gameObject.SetActive(on);
        }

        /// <summary>
        /// Determine if a touch down occurred
        /// </summary>
        /// <param name="position">Position of mouse</param>
        protected virtual void DetectTouchDown(Vector2 position)
        {
            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // get the hit point if we can
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // touched down and stopped dragging
                touchDown = true;
                dragging = false;

                if (tableOfContents)
                {
                    // table of contents "button" clicked
                    tableOfContentsDetected();
                }
                else
                {
                    // page hit
                    touchDownPosition = hitPosition;
                    lastDragPosition = hitPosition;

                    if (touchDownDetected != null)
                    {
                        // handle page touched
                        touchDownDetected(page, hitPositionNormalized);
                    }
                }
            }
        }

        /// <summary>
        /// Determine if a drag occurred
        /// </summary>
        /// <param name="position">Position of mouse</param>
        protected virtual void DetectDrag(Vector2 position)
        {
            // exit if we don't have a handler for this
            if (dragDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // get the hit point if we can
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // get the offset from the last drag position
                var offset = hitPosition - lastDragPosition;

                // if the offset is more than the drag minimum
                if (offset.magnitude >= DragThreshold)
                {
                    // dragging is true, fire the handler and update the last position

                    dragging = true;
                    dragDetected(page, touchDownPosition, hitPosition, offset);
                    lastDragPosition = hitPosition;
                }
            }
        }

        /// <summary>
        /// Determine if a touch up event occurred
        /// </summary>
        /// <param name="position">Mouse position</param>
        protected virtual void DetectTouchUp(Vector2 position)
        {
            // exit if there is no handler
            if (touchUpDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // get the hit point if we can
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // no longer touching.
                touchDown = false;

                // call the handler
                touchUpDetected(page, hitPositionNormalized, dragging);
            }
        }

        /// <summary>
        /// Gets the hit point of the page collider
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="hitPosition">The absolute hit point on the page collider</param>
        /// <param name="hitPositionNormalized">The hit point normalized between 0 and 1 on both axis of the page collider</param>
        /// <param name="page">Which page was hit</param>
        /// <param name="tableOfContents">Whether the table of contents "button" was hit</param>
        /// <returns></returns>
        protected virtual bool GetHitPoint(Vector3 mousePosition, out Vector2 hitPosition, out Vector2 hitPositionNormalized, out PageEnum page, out bool tableOfContents)
        {
            hitPosition = Vector2.zero;
            hitPositionNormalized = Vector2.zero;
            page = PageEnum.Left;
            tableOfContents = false;

            // get a ray from the screen to the page colliders
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            // cast the ray against the collider mask
            if (Physics.Raycast(ray, out hit, 1000, pageTouchPadLayerMask))
            {
                // hit

                // determine which page was hit
                page = hit.collider.gameObject.name == PageLeftColliderName ? PageEnum.Left : PageEnum.Right;

                // determine if the table of contents "button" was hit
                tableOfContents = hit.collider.gameObject.name == TableOfContentsColliderName;

                // get the page index to use for the page rects
                var pageIndex = (int)page;

                // set the hit position using the x and z axis
                hitPosition = new Vector2(hit.point.x, hit.point.z);

                // normalize the hit position against the page rects
                hitPositionNormalized = new Vector2((hit.point.x - pageRects[pageIndex].xMin) / pageRects[pageIndex].width,
                                                        (hit.point.z - pageRects[pageIndex].yMin) / pageRects[pageIndex].height
                                                        );

                return true;
            }

            return false;
        }
    }
}