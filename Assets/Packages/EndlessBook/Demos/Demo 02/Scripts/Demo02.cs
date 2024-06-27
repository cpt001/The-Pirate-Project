namespace echo17.EndlessBook.Demo02
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// The type of action to occur from a page view
    /// </summary>
    public enum BookActionTypeEnum
    {
        ChangeState,
        TurnPage
    }

    /// <summary>
    /// Delegate that handles the action taken by a page view
    /// </summary>
    /// <param name="actionType">The type of action to perform</param>
    /// <param name="actionValue">The value of the action (state or page number)</param>
    public delegate void BookActionDelegate(BookActionTypeEnum actionType, int actionValue);

    /// <summary>
    /// More complex demo showing several of the capabilities of the EndlessBook.
    /// Please note that these algorithms are not the only way you can set up your book,
    /// but they provide a useful guideline and learning material.
    /// 
    /// Some pages use render textures that are creating using mini-scenes. These
    /// are toggled on when a page becomes visible, and toggled back off when the page is hidden.
    /// 
    /// This demo also shows how you can attach extra objects to your books, like the 
    /// book cover corner protectors that follow the front and back covers.
    /// </summary>
    public class Demo02 : MonoBehaviour
    {
        /// <summary>
        /// Make sure the audio is off so that we don't get an open sound at the beginning
        /// </summary>
        protected bool audioOn = false;

        /// <summary>
        /// Whether pages are being flipped
        /// </summary>
        protected bool flipping = false;

        /// <summary>
        /// The book
        /// </summary>
        public EndlessBook book;

        /// <summary>
        /// How fast the state change animations should be
        /// </summary>
        public float openCloseTime = 0.3f;

        /// <summary>
        /// What the time calculation should be for turning a group of pages (like table of contents jump)
        /// </summary>
        public EndlessBook.PageTurnTimeTypeEnum groupPageTurnType;

        /// <summary>
        /// The time to take to turn a single page
        /// </summary>
        public float singlePageTurnTime;

        /// <summary>
        /// The time to take to turn a group of pages
        /// </summary>
        public float groupPageTurnTime;

        /// <summary>
        /// What page is the table of contents on?
        /// </summary>
        public int tableOfContentsPageNumber;

        /// <summary>
        /// The sound to make when the book opens
        /// </summary>
        public AudioSource bookOpenSound;

        /// <summary>
        /// The sound to make when the book closes
        /// </summary>
        public AudioSource bookCloseSound;

        /// <summary>
        /// The sounds for each of the page components' turn
        /// </summary>
        public AudioSource pageTurnSound;

        /// <summary>
        /// The sound to make when multiple pages are turning
        /// </summary>
        public AudioSource pagesFlippingSound;

        /// <summary>
        /// The delay to incur when playing the page flipping sound
        /// </summary>
        public float pagesFlippingSoundDelay;

        /// <summary>
        /// The touchpad to handle interaction with the book and pages
        /// </summary>
        public TouchPad touchPad;

        /// <summary>
        /// The mini-scenes that are rendered to textures for the book pages
        /// </summary>
        public PageView[] pageViews;

        void Start()
        {
            // turn off all the mini-scenes since no pages are visible
            TurnOffAllPageViews();

            // set up touch pad handlers
            touchPad.touchDownDetected = TouchPadTouchDownDetected;
            touchPad.touchUpDetected = TouchPadTouchUpDetected;
            touchPad.tableOfContentsDetected = TableOfContentsDetected;
            touchPad.dragDetected = TouchPadDragDetected;

            // set the book closed
            OnBookStateChanged(EndlessBook.StateEnum.ClosedFront, EndlessBook.StateEnum.ClosedFront, -1);

            // turn on the audio now that the book state is set the first time,
            // otherwise we'd hear a noise and no change would occur
            audioOn = true;
        }

        /// <summary>
        /// Called when the book's state changes
        /// </summary>
        /// <param name="fromState">Previous state</param>
        /// <param name="toState">Current state</param>
        /// <param name="pageNumber">Current page number</param>
        protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int pageNumber)
        {
            switch (toState)
            {
                case EndlessBook.StateEnum.ClosedFront:
                case EndlessBook.StateEnum.ClosedBack:

                    // play the closed sound
                    if (audioOn)
                    {
                        bookCloseSound.Play();
                    }

                    // turn off page mini-scenes
                    TurnOffAllPageViews();

                    break;

                case EndlessBook.StateEnum.OpenMiddle:

                    if (fromState != EndlessBook.StateEnum.OpenMiddle)
                    {
                        // play open sound
                        bookOpenSound.Play();
                    }
                    else
                    {
                        // stop the flipping sound
                        flipping = false;
                        pagesFlippingSound.Stop();
                    }

                    // turn off the front and back page mini-scenes
                    TogglePageView(0, false);
                    TogglePageView(999, false);

                    break;

                case EndlessBook.StateEnum.OpenFront:
                case EndlessBook.StateEnum.OpenBack:

                    // play the open sound
                    bookOpenSound.Play();

                    break;
            }

            // turn on the touchpad
            ToggleTouchPad(true);
        }

        /// <summary>
        /// Toggle the touchpad on and off
        /// </summary>
        /// <param name="on">Whether the touchpad is on</param>
        protected virtual void ToggleTouchPad(bool on)
        {
            // left page should only be available if the book is not in the ClosedFront state
            touchPad.Toggle(TouchPad.PageEnum.Left, on && book.CurrentState != EndlessBook.StateEnum.ClosedFront);

            // right page should only be available if the book is not in the ClosedBack state
            touchPad.Toggle(TouchPad.PageEnum.Right, on && book.CurrentState != EndlessBook.StateEnum.ClosedBack);

            // only use the table of contents "button" if not on the first group of pages
            touchPad.ToggleTableOfContents(on && book.CurrentLeftPageNumber > 1);
        }

        /// <summary>
        /// Deactivates all the page mini-scenes
        /// </summary>
        protected virtual void TurnOffAllPageViews()
        {
            for (var i = 0; i < pageViews.Length; i++)
            {
                if (pageViews[i] != null)
                {
                    pageViews[i].Deactivate();
                }
            }
        }

        /// <summary>
        /// Turns a page mini-scene on or off
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <param name="on">Whether the mini-scene is on or off</param>
        protected virtual void TogglePageView(int pageNumber, bool on)
        {
            var pageView = GetPageView(pageNumber);

            if (pageView != null)
            {
                if (pageView != null)
                {
                    if (on)
                    {
                        pageView.Activate();
                    }
                    else
                    {
                        pageView.Deactivate();
                    }
                }
            }
        }

        /// <summary>
        /// Handler for when a page starts to turn.
        /// We play a sound, turn of the touchpad, and toggle
        /// page view mini-scenes.
        /// </summary>
        /// <param name="page">The page the starting turning</param>
        /// <param name="pageNumberFront">The page number of the front of the page</param>
        /// <param name="pageNumberBack">The page number of the back of hte page</param>
        /// <param name="pageNumberFirstVisible">The page number of the first visible page in the book</param>
        /// <param name="pageNumberLastVisible">The page number of the last visible page in the book</param>
        /// <param name="turnDirection">The direction the page is turning</param>
        protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            // play page turn sound if not flipping through multiple pages
            if (!flipping)
            {
                pageTurnSound.Play();
            }

            // turn off the touch pad
            ToggleTouchPad(false);

            // turn on the front and back page views of the page if necessary
            TogglePageView(pageNumberFront, true);
            TogglePageView(pageNumberBack, true);

            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    // turn on the last visible page view if necessary
                    TogglePageView(pageNumberLastVisible, true);

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    // turn on the first visible page view if necessary
                    TogglePageView(pageNumberFirstVisible, true);

                    break;
            }
        }

        /// <summary>
        /// Handler for when a page stops turning.
        /// We toggle the page views for the mini-scenes off for the relevent pages
        /// </summary>
        /// <param name="page">The page the starting turning</param>
        /// <param name="pageNumberFront">The page number of the front of the page</param>
        /// <param name="pageNumberBack">The page number of the back of hte page</param>
        /// <param name="pageNumberFirstVisible">The page number of the first visible page in the book</param>
        /// <param name="pageNumberLastVisible">The page number of the last visible page in the book</param>
        /// <param name="turnDirection">The direction the page is turning</param>
        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    // turn off the two pages that are now hidden by this page
                    TogglePageView(pageNumberFirstVisible - 1, false);
                    TogglePageView(pageNumberFirstVisible - 2, false);

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    // turn off the two pages that are now hidden by this page
                    TogglePageView(pageNumberLastVisible + 1, false);
                    TogglePageView(pageNumberLastVisible + 2, false);

                    break;
            }
        }

        /// <summary>
        /// Turns to the table of contents
        /// </summary>
        protected virtual void TableOfContentsDetected()
        {
            TurnToPage(tableOfContentsPageNumber);
        }

        /// <summary>
        /// Handles whether a mouse down was detected on the touchpad
        /// </summary>
        /// <param name="page">The page that was hit</param>
        /// <param name="hitPointNormalized">The normalized hit point on the page</param>
        protected virtual void TouchPadTouchDownDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized)
        {
            if (book.CurrentState == EndlessBook.StateEnum.OpenMiddle)
            {
                PageView pageView;

                switch (page)
                {
                    case TouchPad.PageEnum.Left:

                        // get the left page view if available
                        pageView = GetPageView(book.CurrentLeftPageNumber);

                        if (pageView != null)
                        {
                            // call touchdown on the page view
                            pageView.TouchDown();
                        }

                        break;

                    case TouchPad.PageEnum.Right:

                        // get the right page view if available
                        pageView = GetPageView(book.CurrentRightPageNumber);

                        if (pageView != null)
                        {
                            // call the touchdown on the page view
                            pageView.TouchDown();
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Handles the touch up event from the touchpad
        /// </summary>
        /// <param name="page">The page that was hit</param>
        /// <param name="hitPointNormalized">The normalized hit point on the page</param>
        /// <param name="dragging">Whether we were dragging</param>
        protected virtual void TouchPadTouchUpDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized, bool dragging)
        {
            switch (book.CurrentState)
            {
                case EndlessBook.StateEnum.ClosedFront:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Right:

                            // transition from the ClosedFront to the OpenFront states
                            OpenFront();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenFront:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the OpenFront to the ClosedFront states
                            ClosedFront();

                            break;

                        case TouchPad.PageEnum.Right:

                            // transition from the OpenFront to the OpenMiddle states
                            OpenMiddle();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenMiddle:

                    PageView pageView;

                    if (dragging)
                    {
                        // get the left page view if available.
                        // in this demo we only have one group of pages that handle the drag: the map.
                        // instead of having logic for dragging on both pages, we'll just handle it on the left
                        pageView = GetPageView(book.CurrentLeftPageNumber);

                        if (pageView != null)
                        {
                            // call the drag method on the page view
                            pageView.Drag(Vector2.zero, true);
                        }

                        return;
                    }

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // get the left page view if available
                            pageView = GetPageView(book.CurrentLeftPageNumber);

                            if (pageView != null)
                            {
                                // cast a ray into the page and exit if we hit something (don't turn the page)
                                if (pageView.RayCast(hitPointNormalized, BookAction))
                                {
                                    return;
                                }
                            }

                            break;

                        case TouchPad.PageEnum.Right:

                            // get the right page view if available
                            pageView = GetPageView(book.CurrentRightPageNumber);

                            if (pageView != null)
                            {
                                // cast a ray into the page and exit if we hit something (don't turn the page)
                                if (pageView.RayCast(hitPointNormalized, BookAction))
                                {
                                    return;
                                }
                            }

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.OpenBack:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the OpenBack to the OpenMiddle states
                            OpenMiddle();

                            break;

                        case TouchPad.PageEnum.Right:

                            // transition from the OpenBack to the ClosedBack states
                            ClosedBack();

                            break;
                    }

                    break;

                case EndlessBook.StateEnum.ClosedBack:

                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:

                            // transition from the ClosedBack to the OpenBack states
                            OpenBack();

                            break;
                    }

                    break;

            }

            switch (page)
            {
                case TouchPad.PageEnum.Left:

                    if (book.CurrentLeftPageNumber == 1)
                    {
                        // if on the first page, transition from the OpenMiddle to the OpenFront states
                        OpenFront();
                    }
                    else
                    {
                        // not on the first page, so just turn back one page
                        book.TurnBackward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                    }

                    break;

                case TouchPad.PageEnum.Right:

                    if (book.CurrentRightPageNumber == book.LastPageNumber)
                    {
                        // if on the last page, transition from the OpenMiddle to the OpenBack states
                        OpenBack();
                    }
                    else
                    {
                        // not on the last page, so just turn forward a page
                        book.TurnForward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the drag event from the touchpad
        /// </summary>
        /// <param name="page">The page that was dragged on</param>
        /// <param name="touchDownPosition">The position of the touch</param>
        /// <param name="currentPosition">The current position</param>
        /// <param name="incrementalChange">The change of the touch positions between frames</param>
        protected virtual void TouchPadDragDetected(TouchPad.PageEnum page, Vector2 touchDownPosition, Vector2 currentPosition, Vector2 incrementalChange)
        {
            // only handle drag in the OpenMiddle state
            if (book.CurrentState == EndlessBook.StateEnum.OpenMiddle)
            {
                // get the page view if available
                var pageView = GetPageView(book.CurrentLeftPageNumber);

                if (pageView != null)
                {
                    // drag
                    pageView.Drag(incrementalChange, false);
                }
            }
        }

        /// <summary>
        /// Handler for a raycast hit on a page view
        /// </summary>
        /// <param name="actionType">The type of action to perform</param>
        /// <param name="actionValue">The value of the action (state or page number)</param>
        protected virtual void BookAction(BookActionTypeEnum actionType, int actionValue)
        {
            switch (actionType)
            {
                case BookActionTypeEnum.ChangeState:

                    // set the book state
                    SetState((EndlessBook.StateEnum)System.Convert.ToInt16(actionValue));

                    break;

                case BookActionTypeEnum.TurnPage:

                    // table of contents actions

                    if (actionValue == 999)
                    {
                        // go to the back page (OpenBack state)
                        OpenBack();
                    }
                    else
                    {
                        // turn to a page
                        TurnToPage(System.Convert.ToInt16(actionValue));
                    }

                    break;
            }
        }

        /// <summary>
        /// Gets the page view mini-scene of a page number
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <returns></returns>
        protected virtual PageView GetPageView(int pageNumber)
        {
            // search for a page view.
            // 0 = front page,
            // 999 = back page
            return pageViews.Where(x => x.name == string.Format("PageView_{0}", (pageNumber == 0 ? "Front" : (pageNumber == 999 ? "Back" : pageNumber.ToString("00"))))).FirstOrDefault();
        }

        /// <summary>
        /// Set the ClosedFront state
        /// </summary>
        protected virtual void ClosedFront()
        {
            SetState(EndlessBook.StateEnum.ClosedFront);
        }

        /// <summary>
        /// Set the OpenFront state
        /// </summary>
        protected virtual void OpenFront()
        {
            // toggle the front page view
            TogglePageView(0, true);

            SetState(EndlessBook.StateEnum.OpenFront);
        }

        /// <summary>
        /// Set the OpenMiddle state
        /// </summary>
        protected virtual void OpenMiddle()
        {
            // toggle the left and right page views
            TogglePageView(book.CurrentLeftPageNumber, true);
            TogglePageView(book.CurrentRightPageNumber, true);

            SetState(EndlessBook.StateEnum.OpenMiddle);
        }

        /// <summary>
        /// Set the OpenBack state
        /// </summary>
        protected virtual void OpenBack()
        {
            // toggle the back page view
            TogglePageView(999, true);

            SetState(EndlessBook.StateEnum.OpenBack);
        }

        /// <summary>
        /// Set the ClosedBack state
        /// </summary>
        protected virtual void ClosedBack()
        {
            SetState(EndlessBook.StateEnum.ClosedBack);
        }

        /// <summary>
        /// Set the state
        /// </summary>
        /// <param name="state">The state to set to</param>
        protected virtual void SetState(EndlessBook.StateEnum state)
        {
            // turn of the touch pad
            ToggleTouchPad(false);

            // set the state
            book.SetState(state, openCloseTime, OnBookStateChanged);
        }

        /// <summary>
        /// Turns to a page
        /// </summary>
        /// <param name="pageNumber"></param>
        protected virtual void TurnToPage(int pageNumber)
        {
            var newLeftPageNumber = pageNumber % 2 == 0 ? pageNumber - 1 : pageNumber;

            // play the flipping sound if more than a single page is turning
            if (Mathf.Abs(newLeftPageNumber - book.CurrentLeftPageNumber) > 2)
            {
                flipping = true;
                pagesFlippingSound.PlayDelayed(pagesFlippingSoundDelay);
            }

            // turn to page
            book.TurnToPage(pageNumber, groupPageTurnType, groupPageTurnTime,
                            openTime: openCloseTime,
                            onCompleted: OnBookStateChanged,
                            onPageTurnStart: OnPageTurnStart,
                            onPageTurnEnd: OnPageTurnEnd);
        }
    }
}