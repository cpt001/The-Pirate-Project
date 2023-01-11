namespace echo17.EndlessBook
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// State change event sent after a state change has completed.
    /// This is passed in the SetState method.
    /// </summary>
    /// <param name="fromState">The previous state</param>
    /// <param name="toState">The current state</param>
    /// <param name="pageNumber">The page number the book is currently on</param>
    public delegate void StateChangedDelegate(EndlessBook.StateEnum fromState,
                                                EndlessBook.StateEnum toState,
                                                int pageNumber);

    /// <summary>
    /// Page turn event sent at and after each page turn occurs.
    /// This allows you to handle updating your scene if necessary.
    /// </summary>
    /// <param name="page">The page component of the animated page that is turning</param>
    /// <param name="pageNumberFront">The page number on the front side of the page</param>
    /// <param name="pageNumberBack">The page number on the back side of the page</param>
    /// <param name="pageNumberFirstVisible">The page number of the first visible page in the book. Not related to this current page, but still useful</param>
    /// <param name="pageNumberLastVisible">The page number of the last visible page in the book. Not related to this current page, but still useful</param>
    /// <param name="turnDirection">The direction the page is turning</param>
    public delegate void PageTurnDelegate(Page page,
                                            int pageNumberFront,
                                            int pageNumberBack,
                                            int pageNumberFirstVisible,
                                            int pageNumberLastVisible,
                                            Page.TurnDirectionEnum turnDirection);

	/// <summary>
    /// Event that is called when the final turn page animation completes after dragging a page manually
    /// </summary>
    /// <param name="leftPageNumber">The left page number after the turn</param>
    /// <param name="rightPageNumber">The right page number after the turn</param>
	public delegate void TurnPageDragCompleted(int leftPageNumber, int rightPageNumber);

    /// <summary>
    /// This class handles state changes and turning of the pages, setting the appropriate materials.
    /// </summary>
    public class EndlessBook : MonoBehaviour
    {
        /// <summary>
        /// Mappings set up on the animated book and all static standins.
        /// These mappings are used to update the materials on all meshes
        /// when a material is set in the inspector or through code.
        /// </summary>
        [Serializable]
        public class MaterialMapping
        {
            public MaterialEnum materialType;
            public List<MaterialRendererMapping> mappings;
        }

        /// <summary>
        /// Mappings set up to change the material by renderer.
        /// The parent of these mappings is the MaterialMapping struct.
        /// </summary>
        [Serializable]
        public struct MaterialRendererMapping
        {
            public GameObject parentGameObject;
            public Renderer renderer;
            public int index;
        }

        /// <summary>
        /// Information used internally to handle all the page turns
        /// in order.
        /// </summary>
        public struct TurnToPageData
        {
            /// <summary>
            /// The page number to turn to
            /// </summary>
            public int pageNumber;

            /// <summary>
            /// The time to take to turn to the page,
            /// based on the turn time type
            /// </summary>
            public float time;

            /// <summary>
            /// How much time to allocate between turning each page,
            /// calculated by the time, the number of visible pages,
            /// and how many pages need to be turned.
            /// </summary>
            public float delayBetweenPageTurns;

            /// <summary>
            /// The delay time remaining to turn the next page
            /// </summary>
            public float delayTimeRemaining;

            /// <summary>
            /// The time that the page animation should be played in
            /// </summary>
            public float pageTurnTime;

            /// <summary>
            /// The number of pages remaining to start to be turned
            /// </summary>
            public int pagesLeftToTurn;

            /// <summary>
            /// The number of pages that still need to complete turning
            /// </summary>
            public int pagesLeftToComplete;

            /// <summary>
            /// The pointer to the last visible page.
            /// This loops back, recycling as needed
            /// </summary>
            public int lastActivePageIndex;

            /// <summary>
            /// The last page number that is currently visible.
            /// This is the far right page if turning forward,
            /// the far left page if turning backward
            /// </summary>
            public int lastPageNumber;

            /// <summary>
            /// The first page number that is currently visible.
            /// This is the far left page if turning forward,
            /// the far right page if turning backward
            /// </summary>
            public int farPageNumber;

            /// <summary>
            /// The direction the pages are turning
            /// </summary>
            public Page.TurnDirectionEnum turnDirection;

            /// <summary>
            /// The way the turn time is calculated:
            /// by each page, or total turn time
            /// </summary>
            public PageTurnTimeTypeEnum turnTimeType;

            /// <summary>
            /// The handler for when the page turning is completed
            /// </summary>
            public StateChangedDelegate onCompleted;

            /// <summary>
            /// The handler for when the page begins to turn
            /// </summary>
            public PageTurnDelegate onPageTurnStart;

            /// <summary>
            /// The handler for when the page has completed turning
            /// </summary>
            public PageTurnDelegate onPageTurnEnd;
        }

        /// <summary>
        /// Queued value of the standin. This is used in case
        /// the book is changing states so as not to interfere
        /// </summary>
        protected struct StandinQualityQueue
        {
            public StateEnum state;
            public StandinQualityEnum quality;
        }

        /// <summary>
        /// The animations that can be playing on the book
        /// </summary>
        protected enum AnimationClipName
        {
            ClosedFrontToOpenMiddle,
            OpenMiddleToClosedBack,
            ClosedFrontToOpenFront,
            OpenFrontToOpenMiddle,
            OpenMiddleToOpenBack,
            OpenBackToClosedBack,
            ClosedFrontToClosedBack,
            OpenFrontToClosedBack,
            OpenFrontToOpenBack,
            ClosedFrontToOpenBack
        }

        protected bool hasInitialized = false;

        /// <summary>
        /// Array of clip lengths by animation
        /// </summary>
        protected float[] animationClipLengths;

        /// <summary>
        /// The new state to set the book to
        /// </summary>
        protected StateEnum newState;

        /// <summary>
        /// The handler for when the book state has completed
        /// </summary>
        protected StateChangedDelegate onCompletedAction;

        /// <summary>
        /// The turn to page data used as the pages are turning
        /// </summary>
        protected TurnToPageData turnToPage;

        /// <summary>
        /// Whether the state is being changed
        /// </summary>
        protected bool isChangingState;

        /// <summary>
        /// Whether the pages are being turned
        /// </summary>
        protected bool isTurningPages;

        protected bool isDraggingPage;

        /// <summary>
        /// Queued value for the maximum number of pages to turn.
        /// This is used in case the max is set while the pages are turning
        /// </summary>
        protected int queueMaxPagesTurning;

        /// <summary>
        /// Whether the standin quality is queued for update
        /// </summary>
        protected bool queueStandingQuality;

        /// <summary>
        /// Queued value for the standin quality.
        /// This is used in case the standin quality is changed while the state is being changed.
        /// </summary>
        protected StandinQualityQueue queueStandinQualityData;

        /// <summary>
        /// Hash pointer to the controller's animation speed
        /// </summary>
        protected int AnimationSpeedHash = Animator.StringToHash("AnimationSpeed");

        /// <summary>
        /// Animation controller of the book
        /// </summary>
        [SerializeField]
        protected Animator bookController = null;


        protected SkinnedMeshRenderer skinnedMeshRenderer = null;

        /// <summary>
        /// The current state of the book.
        /// Use property CurrentState publicly.
        /// </summary>
        [SerializeField]
        protected StateEnum currentState;

        /// <summary>
        /// The transform of the standins group
        /// </summary>
        [SerializeField]
        protected Transform standinsTransform = null;

        /// <summary>
        /// The transform of the pages group
        /// </summary>
        [SerializeField]
        protected Transform pagesTransform = null;

        /// <summary>
        /// The current qualities of each standin
        /// </summary>
        [SerializeField]
        protected StandinQualityEnum[] standinQualities;

        /// <summary>
        /// Group of static meshes used when the book is not animating
        /// </summary>
        [SerializeField]
        public GameObject[] standins;

        /// <summary>
        /// The materials currently set for the various book pieces
        /// </summary>
        [SerializeField]
        protected Material[] materials;

        /// <summary>
        /// Mappings of all the materials to the various meshes,
        /// including the animated book and standins.
        /// </summary>
        [SerializeField]
        public List<MaterialMapping> materialMappings;

        /// <summary>
        /// List of materials that are assigned to each page
        /// </summary>
        [SerializeField]
        protected List<PageData> pageData = new List<PageData>();

        /// <summary>
        /// List of the turning page gameobjects
        /// </summary>
        [SerializeField]
        protected List<Page> pages = null;

        /// <summary>
        /// The current page number.
        /// Use the CurrentPageNumber property publicly.
        /// </summary>
        [SerializeField]
        protected int currentPageNumber;

        /// <summary>
        /// The total number of pages that can be turning at one time.
        /// Higher numbers will allow each page to turn slower,
        /// but incur the overhead of displaying a lot of materials
        /// (and possibly rendered scenes) at once.
        /// </summary>
        [SerializeField]
        protected int maxPagesTurningCount = 5;

        /// <summary>
        /// Whether the book uses Time.deltaTime or Time.unscaledDeltaTime
        /// </summary>
        [SerializeField] protected DeltaTimeEnum deltaTime;

        /// <summary>
        /// If the pageData list is not an even number of materials,
        /// this value will be used to populate the final even page number.
        /// This is also the default page material used when adding a new page.
        /// </summary>
        [SerializeField]
        protected Material pageFillerMaterial;

        /// <summary>
        /// If turning by dragging, this is the direction of the turn
        /// </summary>
        protected Page.TurnDirectionEnum turnPageDragDirection;

        /// <summary>
        /// If turning by dragging, this is the final page of the book once the turn is completed
        /// </summary>
        protected int turnPageDragFinalPage;

        /// <summary>
        /// If turning by dragging, this is the normalized time of the drag
        /// </summary>
        protected float turnPageDragNormalizedTime;

        /// <summary>
        /// If turning by dragging, this is called when the turn page final animation is completed
        /// </summary>
        protected TurnPageDragCompleted turnPageDragCompleted;

        /// <summary>
        /// The names of the right and left materials
        /// </summary>
        public const string BookPageRightMaterialName = "BookPageRight";
        public const string BookPageLeftMaterialName = "BookPageLeft";

        /// <summary>
        /// The five possible states the book can be in
        /// </summary>
        public enum StateEnum
        {
            ClosedFront = 0,
            OpenFront = 1,
            OpenMiddle = 2,
            OpenBack = 3,
            ClosedBack = 4
        }

        /// <summary>
        /// The quality level of the OpenMiddle standin
        /// High = suitable from all angles. Full vertex and material usage.
        /// Medium = suitable from most top angles. The front and back pages geometry is missing as are the materials assigned to them.
        /// Low = suitable only from a top orthographic camera. In addition to the medium level cuts, the page sides are also removed.
        /// </summary>
        public enum StandinQualityEnum
        {
            High = 0,
            Low = 1,
            Medium = 2
        }

        /// <summary>
        /// The five materials that make up the book.
        /// Note that depending on what OpenMiddle standin quality is being used, the front and back materials may not be used.
        /// </summary>
        public enum MaterialEnum
        {
            BookCover = 0,
            BookPageBack = 1,
            BookPageFront = 2,
            BookPageLeft = 3,
            BookPageRight = 4
        }

        /// <summary>
        /// The direction the page is turning
        /// </summary>
        public enum PageTurnEnum
        {
            Forward,
            Backward
        }

        /// <summary>
        /// The method to use to calculate the time to turn
        /// TotalTurnTime = The total time it takes to get to the page
        /// TimePerPage = The time it takes to turn a single page
        /// </summary>
        public enum PageTurnTimeTypeEnum
        {
            TotalTurnTime,
            TimePerPage
        }

        public enum DeltaTimeEnum
        {
            deltaTime,
            unscaledDeltaTime
        }

        public DeltaTimeEnum DeltaTime { get { return deltaTime; } set { deltaTime = value; } }

        /// <summary>
        /// The current state of the book
        /// </summary>
        public StateEnum CurrentState { get { return currentState; } }

        /// <summary>
        /// The current page number of the book.
        /// Page number does not start with the front page, only the first
        /// pageData element. Back page is also not included in this.
        /// Front and Back are states and not page numbers.
        /// </summary>
        public int CurrentPageNumber { get { return currentPageNumber; } }

        /// <summary>
        /// The current page number of the left page of the book
        /// </summary>
        public int CurrentLeftPageNumber { get { return LeftPageNumber(currentPageNumber); } }

        /// <summary>
        /// The current page number of the right of the book
        /// </summary>
        public int CurrentRightPageNumber { get { return RightPageNumber(currentPageNumber); } }

        /// <summary>
        /// Returns if the current page is in the first group (page 1 or 2)
        /// </summary>
        public bool IsFirstPageGroup { get { return CurrentLeftPageNumber == 1; } }

        /// <summary>
        /// Returns if the current page is in the last group (last page or the one before it)
        /// </summary>
        public bool IsLastPageGroup { get { return CurrentRightPageNumber == RightPageNumber(pageData.Count); } }

        /// <summary>
        /// The last page number in the book
        /// </summary>
        public int LastPageNumber { get { return pageData.Count; } }

        /// <summary>
        /// The total number of pages that can turn at one time.
        /// Higher numbers will allow each page to turn slower,
        /// but incur the overhead of displaying a lot of materials
        /// (and possibly rendered scenes) at once.
        /// </summary>
        public int MaxPagesTurningCount { get { return maxPagesTurningCount; } }

        /// <summary>
        /// The filler material used if the page count is not even.
        /// This is also the default value used when adding new pages.
        /// </summary>
        public Material PageFillerMaterial { get { return pageFillerMaterial; } }

        /// <summary>
        /// Are the pages being turned?
        /// </summary>
        public bool IsTurningPages { get { return isTurningPages; } }

        /// <summary>
        /// Is a page being dragged manually?
        /// </summary>
        public bool IsDraggingPage { get { return isDraggingPage; } }

		/// <summary>
        /// Wrapper for the normalized time.
		/// This is the time based on the direction of the turn.
		/// Forward: from right to left
		/// Backward: from left to right
        /// </summary>
		public float TurnPageDragNormalizedTime
		{
			get
			{
				return turnPageDragNormalizedTime;
			}
		}


        // Some of these methods are public, but should not be used
        // by your scripts. They are made public to allow some access by
        // the inspector.

        #region Protected and Hidden Public

        void Awake()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            // cache the animation clip lengths
            if (bookController != null)
            {
                animationClipLengths = new float[System.Enum.GetNames(typeof(AnimationClipName)).Length];

                var ac = bookController.runtimeAnimatorController;
                for (var i = 0; i < ac.animationClips.Length; i++)
                {
                    var index = (int)(AnimationClipName)System.Enum.Parse(typeof(AnimationClipName), ac.animationClips[i].name);
                    animationClipLengths[index] = ac.animationClips[i].length;
                }

                // cache the skinned mesh renderer
                skinnedMeshRenderer = bookController.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // Set up the turning page index and handlers
            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].Index = i;
                pages[i].pageTurnCompleted = PageTurnCompleted;
            }

            hasInitialized = true;
        }

        /// <summary>
        /// The state has completed being set
        /// </summary>
        public virtual void BookAnimationCompleted()
        {
            var oldState = currentState;
            currentState = newState;

            // turn off the animated book
            if (bookController != null)
            {
                bookController.gameObject.SetActive(false);
            }

            // turn on the standin
            if (standins[(int)currentState] != null)
            {
                standins[(int)currentState].SetActive(true);
            }

            isChangingState = false;

            // if the standin quality has been queued, go ahead and swap out the objects now
            if (queueStandingQuality)
            {
                queueStandingQuality = false;
                SetStandinQuality(queueStandinQualityData.state, queueStandinQualityData.quality);
            }

            // fire the completion handler
            if (onCompletedAction != null) { onCompletedAction(oldState, currentState, currentPageNumber); }
        }

        /// <summary>
        /// Make sure the settings are valid
        /// </summary>
        public virtual void CheckSettings()
        {
            // make sure the standin qualities are the same as the quality enum

            var stateNames = System.Enum.GetNames(typeof(StateEnum));

            if (standinQualities == null)
            {
                standinQualities = new StandinQualityEnum[stateNames.Length];
            }
            else if (standinQualities.Length != stateNames.Length)
            {
                var list = standinQualities.ToList();
                if (list.Count > stateNames.Length)
                {
                    list.RemoveRange(stateNames.Length, list.Count - stateNames.Length);
                }
                else
                {
                    for (var i = list.Count; i < stateNames.Length; i++)
                    {
                        list.Add(StandinQualityEnum.High);
                    }
                }
                standinQualities = list.ToArray();
            }

            // make sure the materials are the same as the material enum

            var materialNames = System.Enum.GetNames(typeof(MaterialEnum));

            if (materials == null)
            {
                materials = new Material[materialNames.Length];
            }
            else if (materials.Length != materialNames.Length)
            {
                var list = materials.ToList();
                if (list.Count > materialNames.Length)
                {
                    list.RemoveRange(materialNames.Length, list.Count - materialNames.Length);
                }
                else
                {
                    for (var i = list.Count; i < materialNames.Length; i++)
                    {
                        list.Add(null);
                    }
                }
                materials = list.ToArray();
            }
        }

        /// <summary>
        /// Returns the standin quality of a particular state
        /// </summary>
        /// <param name="state">The state to check</param>
        /// <returns></returns>
        public virtual StandinQualityEnum GetStandinQuality(StateEnum state)
        {
            return standinQualities[(int)state];
        }

        /// <summary>
        /// Returns the material based on the material enum
        /// </summary>
        /// <param name="mat">Material enum</param>
        /// <returns></returns>
        public virtual Material GetMaterial(MaterialEnum mat)
        {
            return materials[(int)mat];
        }

        /// <summary>
        /// Loads a standing from the resources folder
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual GameObject LoadStandin(StateEnum state)
        {
            var goName = string.Format("BookStandin{0}_{1}", state.ToString(), standinQualities[(int)state].ToString());
            var go = GameObject.Instantiate(Resources.Load<GameObject>(goName), standinsTransform);
            go.name = goName;
            var layer = standinsTransform.gameObject.layer;
            foreach (var t in go.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = layer;
            }
            return go;
        }

        /// <summary>
        /// This calculates the turnToPage data
        /// </summary>
        /// <param name="oldState">The state turning from</param>
        /// <param name="newState">The state turning to</param>
        /// <param name="newPageNumber">The new page number to turn to</param>
        protected virtual void TurnToPageInternal(StateEnum oldState, StateEnum newState, int newPageNumber)
        {
            // Reset the page turn completions (in case we manually turned the page before this)
            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].Index = i;
                pages[i].pageTurnCompleted = PageTurnCompleted;
            }

            var currentRightPageNumber = RightPageNumber(newPageNumber);

            // get the page count between the current page and the page to turn to
            var pageDiff = RightPageNumber(turnToPage.pageNumber) - currentRightPageNumber;
            var pageCount = Mathf.Abs(pageDiff);

            // the actual physical pages to turn will be half the page difference since each page has a front and back
            turnToPage.turnDirection = Mathf.Sign(pageDiff) == 1f ? Page.TurnDirectionEnum.TurnForward : Page.TurnDirectionEnum.TurnBackward;
            turnToPage.pagesLeftToTurn = pageCount / 2;
            turnToPage.pagesLeftToComplete = turnToPage.pagesLeftToTurn;

            // calculate turn time
            float totalTurnTime = 1f;
            float timePerPage = 1f;
            switch (turnToPage.turnTimeType)
            {
                case PageTurnTimeTypeEnum.TimePerPage:

                    timePerPage = turnToPage.time;
                    totalTurnTime = timePerPage * ((((float)turnToPage.pagesLeftToTurn - 1f) / (float)maxPagesTurningCount) + 1f);

                    break;

                case PageTurnTimeTypeEnum.TotalTurnTime:

                    totalTurnTime = turnToPage.time;
                    timePerPage = (turnToPage.turnTimeType == PageTurnTimeTypeEnum.TimePerPage ? turnToPage.time : turnToPage.time / ((((float)turnToPage.pagesLeftToTurn - 1f) / (float)maxPagesTurningCount) + 1f));

                    break;
            }

            // calculate the delay between each page turn and the speed to play the turn animation
            turnToPage.delayBetweenPageTurns = (totalTurnTime - timePerPage) / ((float)turnToPage.pagesLeftToTurn - 1f);
            turnToPage.pageTurnTime = timePerPage;

            // set up the initial page values
            turnToPage.lastActivePageIndex = -1;
            turnToPage.farPageNumber = turnToPage.turnDirection == Page.TurnDirectionEnum.TurnForward ? LeftPageNumber(currentPageNumber) : currentRightPageNumber;
            turnToPage.lastPageNumber = currentRightPageNumber;
            turnToPage.delayTimeRemaining = 0;

            isTurningPages = true;
        }

        /// <summary>
        /// This starts the page to turn by dragging
        /// </summary>
        /// <param name="direction">The direction of the turn</param>
        public virtual bool TurnPageDragStart(Page.TurnDirectionEnum direction)
		{
            isDraggingPage = true;

			// cache the direction
			turnPageDragDirection = direction;

			int bookLeftPage;
			int bookRightPage;
			int pageFrontPage;
			int pageBackPage;

			if (direction == Page.TurnDirectionEnum.TurnForward)
			{
				// exit the turning if we are turning forward and already on the last page group of the book
				if (IsLastPageGroup)
				{
                    // turn dragging was not successful
                    isDraggingPage = false;
					return false;
				}

				// get the book's left and right page numbers
				bookLeftPage = LeftPageNumber(currentPageNumber);
				bookRightPage = RightPageNumber(currentPageNumber + 2);

				// get the turning page's front and back page numbers
				pageFrontPage = RightPageNumber(currentPageNumber);
				pageBackPage = LeftPageNumber(currentPageNumber + 2);
			}
			else
			{
				// exit the turning if we are turning backward and already on the first page group of the book
				if (IsFirstPageGroup)
				{
                    // turn dragging was not successful
                    isDraggingPage = false;
                    return false;
				}

				// get the book's left and right page numbers
				bookLeftPage = LeftPageNumber(currentPageNumber - 2);
				bookRightPage = RightPageNumber(currentPageNumber);

				// get the turning page's front and back page numbers
				pageFrontPage = RightPageNumber(currentPageNumber - 2);
				pageBackPage = LeftPageNumber(currentPageNumber);
			}

			// set the materials of the book's pages
			SetMaterial(MaterialEnum.BookPageLeft, GetPageMaterial(bookLeftPage));
            SetMaterial(MaterialEnum.BookPageRight, GetPageMaterial(bookRightPage));

			// activate a turning page and tell it to begin turning with zero speed
			pages[0].gameObject.SetActive(true);
			pages[0].pageTurnCompleted = null;
			pages[0].Turn(direction, 0,	GetPageMaterial(pageFrontPage), GetPageMaterial(pageBackPage));

			// turn dragging was successful
			return true;
		}

        /// <summary>
        /// This drags the page manually. Only call this after calling TurnPageDragStart
        /// </summary>
        /// <param name="normalizedTime">The normalized time of the page turn animation</param>
        public virtual void TurnPageDrag(float normalizedTime)
		{
			// if the turn direction is forward, reverse the normalized time
			turnPageDragNormalizedTime = turnPageDragDirection == Page.TurnDirectionEnum.TurnForward ? 1f - normalizedTime : normalizedTime;

			// set the turning page's normalized time
			pages[0].SetPageNormalizedTime(turnPageDragNormalizedTime);
		}

        /// <summary>
        /// This stops the turn page dragging. Only call this after calling TurnPageDragStart
        /// </summary>
        /// <param name="stopSpeed">The speed of the animation after the page is allowed to animate to its final position</param>
        public virtual void TurnPageDragStop(float stopSpeed, TurnPageDragCompleted turnPageDragCompleted, bool reverse = false)
		{
			this.turnPageDragCompleted = turnPageDragCompleted;

			// calculate the final page of the book after the turn is completed
			if (reverse)
			{
				turnPageDragFinalPage = currentPageNumber;
			}
			else
			{
				turnPageDragFinalPage = currentPageNumber + (turnPageDragDirection == Page.TurnDirectionEnum.TurnForward ? 2 : -2);
			}

			// if the page is turned at least a little
			if (turnPageDragNormalizedTime > 0)
			{
				// set the page turn completion action
				pages[0].pageTurnCompleted = TurnPageDragTurnCompleted;

				// tell the turn page to complete its animation.
				pages[0].PlayRemainder(stopSpeed, reverse);
			}
			else
			{
				// we already completed the turn, so no final animation is necessary.
				// just call the completed action.
				TurnPageDragTurnCompleted(pages[0]);
			}
		}

        /// <summary>
        /// This is called when the turn page completes its final animation
        /// </summary>
        /// <param name="page">The page that completes the animation</param>
        protected virtual void TurnPageDragTurnCompleted(Page page)
		{
            isDraggingPage = false;

			// set the final book page number
			SetPageNumber(turnPageDragFinalPage);

			if (turnPageDragCompleted != null)
			{
				// fire the turn completed event
				turnPageDragCompleted(CurrentLeftPageNumber, CurrentRightPageNumber);
			}
		}

        /// <summary>
        /// Called when a page has completed its turn animation
        /// </summary>
        /// <param name="page"></param>
        public virtual void PageTurnCompleted(Page page)
        {
            // set the new far page number
            turnToPage.farPageNumber += (turnToPage.turnDirection == Page.TurnDirectionEnum.TurnForward ? 1 : -1) * 2;
            turnToPage.pagesLeftToComplete--;

            // if the handler is available, call it
            if (turnToPage.onPageTurnEnd != null)
            {
                if (turnToPage.turnDirection == Page.TurnDirectionEnum.TurnForward)
                {
                    turnToPage.onPageTurnEnd(page, turnToPage.farPageNumber - 1, turnToPage.farPageNumber, turnToPage.farPageNumber, turnToPage.lastPageNumber, turnToPage.turnDirection);
                }
                else
                {
                    turnToPage.onPageTurnEnd(page, turnToPage.farPageNumber, turnToPage.farPageNumber + 1, LeftPageNumber(turnToPage.lastPageNumber), turnToPage.farPageNumber, turnToPage.turnDirection);
                }
            }

            // if have completed all page turns
            if (turnToPage.pagesLeftToComplete == 0)
            {
                isTurningPages = false;

                // if we queued the maximum number of pages that could turn,
                // we can now update the pages.
                if (queueMaxPagesTurning > 0)
                {
                    SetMaxPagesTurningCount(queueMaxPagesTurning);
                    queueMaxPagesTurning = 0;
                }

                // set the current page and its materials
                SetPageNumber(turnToPage.pageNumber);

                // call the completed delegate if necessary
                if (turnToPage.onCompleted != null) turnToPage.onCompleted(StateEnum.OpenMiddle, StateEnum.OpenMiddle, currentPageNumber);
            }
            else
            {
                // still have pages to turn
                // Update the left (first visible) and right (last visible) pages of the book.

                switch (turnToPage.turnDirection)
                {
                    case Page.TurnDirectionEnum.TurnForward: SetMaterial(MaterialEnum.BookPageLeft, page.PageBackMaterial); break;
                    case Page.TurnDirectionEnum.TurnBackward: SetMaterial(MaterialEnum.BookPageRight, page.PageFrontMaterial); break;
                }
            }
        }

        /// <summary>
        /// called every frame
        /// </summary>
        protected virtual void Update()
        {
            // check if turning pages
            if (isTurningPages)
            {
                // if there are still pages left to turn
                if (turnToPage.pagesLeftToTurn > 0)
                {
                    // count down the delay between page turns
                    switch (deltaTime)
                    {
                        case DeltaTimeEnum.deltaTime:
                            turnToPage.delayTimeRemaining -= Time.deltaTime;
                            break;

                        case DeltaTimeEnum.unscaledDeltaTime:
                            turnToPage.delayTimeRemaining -= Time.unscaledDeltaTime;
                            break;
                    }
    
                    // if the delay timer is zero
                    if (turnToPage.delayTimeRemaining <= 0)
                    {
                        // increment the page index, looping around if we
                        // are at the end to recycle.
                        turnToPage.lastActivePageIndex++;
                        if (turnToPage.lastActivePageIndex > maxPagesTurningCount)
                        {
                            turnToPage.lastActivePageIndex = 0;
                        }

                        // call the TurnPage method on the last page
                        TurnPage(pages[turnToPage.lastActivePageIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Turns one page
        /// </summary>
        /// <param name="page">The page component to turn</param>
        protected virtual void TurnPage(Page page)
        {
            // decrement the number of pages left to turn
            turnToPage.pagesLeftToTurn--;

            // reset the delay timer
            turnToPage.delayTimeRemaining = turnToPage.delayBetweenPageTurns;

            // set the last page number now that this page is turning
            turnToPage.lastPageNumber += (turnToPage.turnDirection == Page.TurnDirectionEnum.TurnForward ? 1 : -1) * 2;

            // set the front and back page numbers depending on the turn direction.
            // also set the first and last visible page numbers of the book.

            int pageNumberFront = -1;
            int pageNumberBack = -1;
            int pageNumberFirstVisible = -1;
            int pageNumberLastVisible = -1;

            switch (turnToPage.turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    pageNumberFront = turnToPage.lastPageNumber - 2;
                    pageNumberBack = turnToPage.lastPageNumber - 1;
                    pageNumberFirstVisible = turnToPage.farPageNumber;
                    pageNumberLastVisible = turnToPage.lastPageNumber;

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    pageNumberFront = turnToPage.lastPageNumber;
                    pageNumberBack = turnToPage.lastPageNumber + 1;
                    pageNumberFirstVisible = turnToPage.lastPageNumber - 1;
                    pageNumberLastVisible = turnToPage.farPageNumber;

                    break;
            }

            // call the turn start handler if necessary
            if (turnToPage.onPageTurnStart != null)
            {
                turnToPage.onPageTurnStart(page, pageNumberFront, pageNumberBack, pageNumberFirstVisible, pageNumberLastVisible, turnToPage.turnDirection);
            }

            // set the materials and start the animation
            page.Turn(turnToPage.turnDirection, turnToPage.pageTurnTime, GetPageMaterial(pageNumberFront), GetPageMaterial(pageNumberBack));

            // set the book's left (first visible) and right (last visible) page materials
            switch (turnToPage.turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:

                    SetMaterial(MaterialEnum.BookPageRight, GetPageMaterial(pageNumberLastVisible));

                    break;

                case Page.TurnDirectionEnum.TurnBackward:

                    SetMaterial(MaterialEnum.BookPageLeft, GetPageMaterial(pageNumberFirstVisible));

                    break;
            }
        }

        /// <summary>
        /// Creates a page when the max page count is changed
        /// </summary>
        protected virtual Page CreatePage()
        {
            var page = GameObject.Instantiate(Resources.Load<Page>("Page"), pagesTransform);
            page.name = "Page";
            page.pageTurnCompleted = PageTurnCompleted;
            var layer = pagesTransform.gameObject.layer;
            foreach (var t in page.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = layer;
            }
            page.gameObject.SetActive(false);
            pages.Add(page);

            return page;
        }

        /// <summary>
        /// Gets the material of the pageData. If none is set
        /// it returns the filler material.
        /// </summary>
        /// <param name="pageNumber">The page number to access</param>
        /// <returns></returns>
        protected virtual Material GetPageMaterial(int pageNumber)
        {
            if (pageNumber <= pageData.Count)
            {
                return pageData[pageNumber - 1].material;
            }
            else
            {
                return pageFillerMaterial;
            }
        }

        /// <summary>
        /// The right hand page of the group that includes the page number
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <returns></returns>
        protected virtual int RightPageNumber(int pageNumber)
        {
            return (pageNumber % 2 == 0 ? pageNumber : pageNumber + 1);
        }

        /// <summary>
        /// The left hand page of the group that includes the page number
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <returns></returns>
        protected virtual int LeftPageNumber(int pageNumber)
        {
            return (pageNumber % 2 == 1 ? pageNumber : pageNumber - 1);
        }

        /// <summary>
        /// Whether the page number is in the currently visible group of pages
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <returns></returns>
        protected virtual bool IsInCurrentPageGroup(int pageNumber)
        {
            return RightPageNumber(pageNumber) == RightPageNumber(currentPageNumber);
        }

        /// <summary>
        /// Warning log for when an invalid page number is used
        /// </summary>
        protected virtual void LogInvalidPageNumber()
        {
            Debug.LogWarning("Invalid page number. Must be in the range [1.." + pageData.Count + "]");
        }

        /// <summary>
        /// Reset all the material mappings
        /// </summary>
        public virtual void RemapMaterials()
        {
            materialMappings.ForEach(m => m.mappings.Clear());

            if (bookController != null)
            {
                RemapGameObjectMaterials(bookController.gameObject);
            }

            for (var i = 0; i < standins.Length; i++)
            {
                RemapGameObjectMaterials(standins[i]);
            }
        }

        /// <summary>
        /// Remaps the materials for a particular game object (animated or standin)
        /// </summary>
        /// <param name="go"></param>
        protected virtual void RemapGameObjectMaterials(GameObject go)
        {
            if (go == null) return;

            bool wasActive = go.activeSelf;
            go.gameObject.SetActive(true);

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                AddMaterialMappings(r.transform.parent.gameObject);
            }

            go.SetActive(wasActive);
        }

        /// <summary>
        /// Adds material mappings for a gameobject
        /// </summary>
        /// <param name="go"></param>
        protected virtual void AddMaterialMappings(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;

                for (var j = 0; j < materials.Length; j++)
                {
                    var materialIndex = (int)Enum.Parse(typeof(MaterialEnum), materials[j].name);

                    if (!materialMappings[materialIndex].mappings.Any(x => x.parentGameObject == go && x.renderer == renderer && x.index == j))
                    {
                        var mapping = new MaterialRendererMapping()
                        {
                            parentGameObject = go,
                            renderer = renderer,
                            index = j
                        };

                        materialMappings[materialIndex].mappings.Add(mapping);
                    }
                }
            }
        }

        #endregion

        // These methods can be used publicly by your scripts

        #region Public Methods

        /// <summary>
        /// Sets the state of the book, animating it if time is greater than zero
        /// </summary>
        /// <param name="state">The state to set to</param>
        /// <param name="animationTime">The amount of time to take to animate</param>
        /// <param name="onCompleted">The handler to call when the state has completed setting</param>
        /// <param name="stopTurningPages">Optionally stop turning the pages before changing the state, otherwise the method will exit if pages are turning</param>
        public virtual void SetState(StateEnum state,
            float animationTime = 1f,
            StateChangedDelegate onCompleted = null,
            bool stopTurningPages = false
            )
        {
            if (!hasInitialized)
            {
              Initialize();
            }

            if (stopTurningPages)
            {
                StopTurningPages();
            }

            // make sure we are not already at the state, changing state, or turning pages
            if (state == currentState || isChangingState || isTurningPages) return;

            // start the animation in a coroutine so that we can hide the
            // skinned mesh renderer for a frame
            StartCoroutine(_SetState(state, animationTime, onCompleted));
        }

        protected virtual IEnumerator _SetState(
            StateEnum state,
            float animationTime = 1f,
            StateChangedDelegate onCompleted = null
            )
        {
            isChangingState = true;

            newState = state;

            onCompletedAction = onCompleted;

            // if the animation time is at or below zero
            if (animationTime <= 0)
            {
                // hide the standin
                standins[(int)currentState].SetActive(false);

                // immediately set the state
                BookAnimationCompleted();
            }
            else
            {
                // show the animated book
                bookController.gameObject.SetActive(true);

                // hide the skinned mesh renderer so it does not flicker if the initial book
                // state is not ClosedFront and animating backward
                skinnedMeshRenderer.enabled = false;

                // set the triggers in the book controller so that it follows the correct path
                bookController.SetTrigger("Current" + currentState.ToString());
                bookController.SetTrigger("New" + newState.ToString());

                // get the animation clip that will be playing so that we can extract its length
                AnimationClipName clipName = AnimationClipName.ClosedFrontToOpenMiddle;
                switch (currentState)
                {
                    case StateEnum.ClosedFront:

                        switch (newState)
                        {
                            case StateEnum.OpenFront: clipName = AnimationClipName.ClosedFrontToOpenFront; break;
                            case StateEnum.OpenMiddle: clipName = AnimationClipName.ClosedFrontToOpenMiddle; break;
                            case StateEnum.OpenBack: clipName = AnimationClipName.ClosedFrontToOpenBack; break;
                            case StateEnum.ClosedBack: clipName = AnimationClipName.ClosedFrontToClosedBack; break;
                        }

                        break;

                    case StateEnum.OpenFront:

                        switch (newState)
                        {
                            case StateEnum.ClosedFront: clipName = AnimationClipName.ClosedFrontToOpenFront; break;
                            case StateEnum.OpenMiddle: clipName = AnimationClipName.OpenFrontToOpenMiddle; break;
                            case StateEnum.OpenBack: clipName = AnimationClipName.OpenFrontToOpenBack; break;
                            case StateEnum.ClosedBack: clipName = AnimationClipName.OpenFrontToClosedBack; break;
                        }

                        break;

                    case StateEnum.OpenMiddle:

                        switch (newState)
                        {
                            case StateEnum.ClosedFront: clipName = AnimationClipName.ClosedFrontToOpenMiddle; break;
                            case StateEnum.OpenFront: clipName = AnimationClipName.OpenFrontToOpenMiddle; break;
                            case StateEnum.OpenBack: clipName = AnimationClipName.OpenMiddleToOpenBack; break;
                            case StateEnum.ClosedBack: clipName = AnimationClipName.OpenMiddleToClosedBack; break;
                        }

                        break;

                    case StateEnum.OpenBack:

                        switch (newState)
                        {
                            case StateEnum.ClosedFront: clipName = AnimationClipName.ClosedFrontToOpenBack; break;
                            case StateEnum.OpenFront: clipName = AnimationClipName.OpenFrontToOpenBack; break;
                            case StateEnum.OpenMiddle: clipName = AnimationClipName.OpenMiddleToOpenBack; break;
                            case StateEnum.ClosedBack: clipName = AnimationClipName.OpenBackToClosedBack; break;
                        }

                        break;

                    case StateEnum.ClosedBack:

                        switch (newState)
                        {
                            case StateEnum.ClosedFront: clipName = AnimationClipName.ClosedFrontToClosedBack; break;
                            case StateEnum.OpenFront: clipName = AnimationClipName.OpenFrontToClosedBack; break;
                            case StateEnum.OpenMiddle: clipName = AnimationClipName.OpenMiddleToClosedBack; break;
                            case StateEnum.OpenBack: clipName = AnimationClipName.OpenBackToClosedBack; break;
                        }

                        break;
                }

                // set the speed of the animation: clip length / desired length = speed
                bookController.SetFloat(AnimationSpeedHash, animationClipLengths[(int)clipName] / animationTime);

                // yield for a frame so that the animation can begin without flickering
                yield return null;

                // hide the standin
                standins[(int)currentState].SetActive(false);

                // turn on the skinned mesh renderer on the second frame
                skinnedMeshRenderer.enabled = true;

                yield return null;
            }
        }

        /// <summary>
        /// Sets the current page number. This does not animate,
        /// just sets the page immediately
        /// </summary>
        /// <param name="pageNumber">The page number to set</param>
        public virtual void SetPageNumber(int pageNumber)
        {
            // set the current page number
            currentPageNumber = pageNumber;

            // set the left and right materials of the book
            SetMaterial(MaterialEnum.BookPageLeft, GetPageMaterial(LeftPageNumber(pageNumber)));
            SetMaterial(MaterialEnum.BookPageRight, GetPageMaterial(RightPageNumber(pageNumber)));
        }

        /// <summary>
        /// Turn one page forward
        /// </summary>
        /// <param name="time">The time to turn a single page</param>
        /// <param name="onCompleted">The handler to call when the turn has completed. State change delegate</param>
        /// <param name="onPageTurnStart">The handler to call when the page starts to turn. Page turn delegate</param>
        /// <param name="onPageTurnEnd">The handler to call when the page stops turning. Page turn delegate</param>
        public virtual void TurnForward(float time,
            StateChangedDelegate onCompleted = null,
            PageTurnDelegate onPageTurnStart = null,
            PageTurnDelegate onPageTurnEnd = null
            )
        {
            // only do this call if the book is in the OpenMiddle state and not on the last group of pages
            if (currentState != StateEnum.OpenMiddle || IsLastPageGroup) return;

            // call the turn to page method
            TurnToPage(CurrentLeftPageNumber + 2, PageTurnTimeTypeEnum.TimePerPage, time, onCompleted: onCompleted, onPageTurnStart: onPageTurnStart, onPageTurnEnd: onPageTurnEnd);
        }

        /// <summary>
        /// Turn one page backward
        /// </summary>
        /// <param name="time">The time to turn a single page</param>
        /// <param name="onCompleted">The handler to call when the turn has completed. State change delegate</param>
        /// <param name="onPageTurnStart">The handler to call when the page starts to turn. Page turn delegate</param>
        /// <param name="onPageTurnEnd">The handler to call when the page stops turning. Page turn delegate</param>
        public virtual void TurnBackward(float time,
            StateChangedDelegate onCompleted = null,
            PageTurnDelegate onPageTurnStart = null,
            PageTurnDelegate onPageTurnEnd = null
            )
        {
            // only do this call if the book is in the OpenMiddle state and not on the first group of pages
            if (currentState != StateEnum.OpenMiddle || IsFirstPageGroup) return;

            TurnToPage(CurrentLeftPageNumber - 2, PageTurnTimeTypeEnum.TimePerPage, time, onCompleted: onCompleted, onPageTurnStart: onPageTurnStart, onPageTurnEnd: onPageTurnEnd);
        }

        /// <summary>
        /// Turn to a specific page
        /// </summary>
        /// <param name="pageNumber">The page number to turn to</param>
        /// <param name="turnType">The way the time is calculated: either by total time, or time per page</param>
        /// <param name="time">The time to use along with turnType</param>
        /// <param name="openTime">If the book is not in the OpenMiddle state, the amount of time to animate the state change</param>
        /// <param name="onCompleted">The handler to call when the turn has completed. State change delegate</param>
        /// <param name="onPageTurnStart">The handler to call when the page starts to turn. Page turn delegate</param>
        /// <param name="onPageTurnEnd">The handler to call when the page stops turning. Page turn delegate</param>
        public virtual void TurnToPage(int pageNumber, PageTurnTimeTypeEnum turnType, float time,
            float openTime = 1f,
            StateChangedDelegate onCompleted = null,
            PageTurnDelegate onPageTurnStart = null,
            PageTurnDelegate onPageTurnEnd = null
            )
        {
            // only do this call if not already turning pages, changing state, and the not already in the page group specified
            if (isTurningPages || isChangingState || IsDraggingPage) return;

            if (currentState == StateEnum.OpenMiddle && IsInCurrentPageGroup(pageNumber))
            {
                if (pageNumber == currentPageNumber) return;
                SetPageNumber(pageNumber);
                return;
            }

            // make sure the page number requested is valid
            if (pageNumber < 1 || pageNumber > pageData.Count)
            {
                LogInvalidPageNumber();
                return;
            }

            // set up the turn to page data to be used internally
            turnToPage = new TurnToPageData()
            {
                pageNumber = pageNumber,
                turnTimeType = turnType,
                time = time,
                onCompleted = onCompleted,
                onPageTurnStart = onPageTurnStart,
                onPageTurnEnd = onPageTurnEnd
            };

            // if the state is not OpenMiddle, first set to that state
            if (currentState != StateEnum.OpenMiddle)
            {
                SetState(StateEnum.OpenMiddle, openTime, (IsInCurrentPageGroup(pageNumber) ? onCompleted : TurnToPageInternal));

                // exit and wait for the state set to finish
                return;
            }

            // already in the current page group, so set the page number and materials
            if (IsInCurrentPageGroup(pageNumber))
            {
                SetPageNumber(pageNumber);
                return;
            }

            // call the internal page turn method.
            // this is also called when the state change is completed if the current state is not OpenMiddle.
            TurnToPageInternal(StateEnum.OpenMiddle, StateEnum.OpenMiddle, currentPageNumber);
        }

        /// <summary>
        /// Immediately stops turning the pages
        /// </summary>
        public virtual void StopTurningPages()
        {
            if (!isTurningPages) return;

            isTurningPages = false;

            // turn off all pages
            for (var i = 0; i < pages.Count; i++)
            {
                pages[i].gameObject.SetActive(false);
            }

            // set the page number immediately of the page the book was turning to
            SetPageNumber(turnToPage.pageNumber);
        }

        /// <summary>
        /// Set the quality of the state standin
        /// </summary>
        /// <param name="state">The state of the standin</param>
        /// <param name="quality">The quality of the standin</param>
        public virtual void SetStandinQuality(StateEnum state, StandinQualityEnum quality)
        {
            // currently only supporting OpenMiddle quality levels
            if (state != StateEnum.OpenMiddle) return;

            var i = (int)state;

            // if we are already changing the state, queue the quality change
            // until we are done
            if (isChangingState)
            {
                queueStandingQuality = true;
                queueStandinQualityData = new StandinQualityQueue()
                {
                    state = state,
                    quality = quality
                };

                return;
            }

            // set the standin quality
            standinQualities[i] = quality;

            // remove the materials of the current standin
            materialMappings.ForEach(s => s.mappings.RemoveAll(x => x.parentGameObject == standins[i]));

            if (Application.isPlaying)
            {
                // destroy the standin if the application is playing,
                // queueing it for the end of the frame
                Destroy(standins[i]);
            }
            else
            {
                // destroy the standin immediately if the application is not playing
                DestroyImmediate(standins[i]);
            }

            // load the new standin from the resources folder
            standins[i] = LoadStandin(state);

            // add the new material mappings
            AddMaterialMappings(standins[i]);

            // set the standin as active or inactive depending on the current book state
            standins[i].SetActive(currentState == state);

            // Set the materials of the standin
            var materialNames = System.Enum.GetNames(typeof(MaterialEnum));
            for (var j = 0; j < materialNames.Length; j++)
            {
                if (materialNames[j] != BookPageRightMaterialName && materialNames[j] != BookPageLeftMaterialName)
                {
                    SetMaterial((MaterialEnum)j, materials[j]);
                }
            }

            if (state == StateEnum.OpenMiddle)
            {
                // set the page number, updating the book's materials
                SetPageNumber(currentPageNumber);
            }
        }

        /// <summary>
        /// Set the material of all objects (animated and standins)
        /// </summary>
        /// <param name="materialType">The type of material to change</param>
        /// <param name="material">The new material to set</param>
        public virtual void SetMaterial(MaterialEnum materialType, Material material)
        {
            // update the materials list.
            materials[(int)materialType] = material;

            // update all the material mappings with the new material
            foreach (var mapping in materialMappings[(int)materialType].mappings)
            {
                if (mapping.renderer != null)
                {
                    var sharedMaterials = mapping.renderer.sharedMaterials;
                    sharedMaterials[mapping.index] = material;
                    mapping.renderer.sharedMaterials = sharedMaterials;
                }
            }
        }

        /// <summary>
        /// Sets the maximum pages that can turn at one time.
        /// The lower the number, the faster each page will need to turn.
        /// The higher the number, the slower each page will need to turn, but
        /// more materials will be used. If those materials use render textures,
        /// this could also add more processing needed. Try to find a balance
        /// between a too few pages (fast and unnatural) to too many (choppy and intensive)
        /// for your project.
        /// </summary>
        /// <param name="newCount"></param>
        public virtual void SetMaxPagesTurningCount(int newCount)
        {
            // if the pages are currently turning, queue the new max count
            if (isTurningPages)
            {
                queueMaxPagesTurning = newCount;
                return;
            }

            // destroy existing pages
            for (var i = pagesTransform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Destroy(pagesTransform.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(pagesTransform.GetChild(i).gameObject);
                }
            }
            pages.Clear();

            // create new pages
            for (var i = 0; i < newCount + 1; i++)
            {
                var page = CreatePage();
                page.Index = i;
            }

            // update the max pages turning count
            maxPagesTurningCount = newCount;
        }

        /// <summary>
        /// Sets the last page filler material
        /// </summary>
        /// <param name="material"></param>
        public virtual void SetPageFillerMaterial(Material material)
        {
            pageFillerMaterial = material;

            // update the page materials if necessary
            SetPageNumber(currentPageNumber);
        }

        /// <summary>
        /// Gets the page date of a given page number
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        /// <returns></returns>
        public virtual PageData GetPageData(int pageNumber)
        {
            // make sure the page number is valid
            if (pageNumber < 1 || pageNumber > pageData.Count)
            {
                LogInvalidPageNumber();
                return PageData.Default();
            }

            return pageData[pageNumber - 1];
        }

        /// <summary>
        /// Adds page data to the book
        /// </summary>
        /// <returns></returns>
        public virtual PageData AddPageData()
        {
            // create new page data
            var newPage = new PageData()
            {
                material = pageFillerMaterial
            };

            // add it
            pageData.Add(newPage);

            // if the page is currently being displayed,
            // update the book's materials
            if (IsInCurrentPageGroup(pageData.Count))
            {
                SetPageNumber(currentPageNumber);
            }

            // return data
            return newPage;
        }

        /// <summary>
        /// Inserts page data before a page number
        /// </summary>
        /// <param name="pageNumber">Inserts before this page number</param>
        /// <returns></returns>
        public virtual PageData InsertPageData(int pageNumber)
        {
            // create page data
            var newPage = new PageData()
            {
                material = pageFillerMaterial
            };

            // insert before the page number specified
            pageData.Insert(pageNumber - 1, newPage);

            // if the page is currently being displayed,
            // update the book's materials
            if (IsInCurrentPageGroup(pageData.Count))
            {
                SetPageNumber(currentPageNumber);
            }

            // return data
            return newPage;
        }

        /// <summary>
        /// Sets the data of a page at a given page number
        /// </summary>
        /// <param name="pageNumber">The page number to change</param>
        /// <param name="data">The data to set</param>
        public virtual void SetPageData(int pageNumber, PageData data)
        {
            // make sure it is a valid page number
            if (pageNumber < 1 || pageNumber > pageData.Count)
            {
                LogInvalidPageNumber();
                return;
            }

            // set the data
            pageData[pageNumber - 1] = data;

            // if the page is currently being displayed,
            // update the book's materials
            if (IsInCurrentPageGroup(pageNumber))
            {
                SetPageNumber(currentPageNumber);
            }
        }

        /// <summary>
        /// Removes page data at a given page number
        /// </summary>
        /// <param name="pageNumber"></param>
        public virtual void RemovePageData(int pageNumber)
        {
            // make sure it is a valid page
            if (pageNumber < 1 || pageNumber > pageData.Count)
            {
                LogInvalidPageNumber();
                return;
            }

            // remove the data
            pageData.RemoveAt(pageNumber - 1);

            // if the page is currently being displayed,
            // update the book's materials
            if (currentPageNumber > pageData.Count)
            {
                SetPageNumber(currentPageNumber - 1);
            }
        }

        /// <summary>
        /// Swaps page data for a page in a direction
        /// </summary>
        /// <param name="pageNumber">The page number to move</param>
        /// <param name="direction">The direction to move</param>
        public virtual void MovePageData(int pageNumber, int direction)
        {
            direction = Math.Sign(direction);
            if (pageNumber < 1 || pageNumber > LastPageNumber)
            {
                LogInvalidPageNumber();
                return;
            }
            if (direction == 0 || (pageNumber == 1 && direction == -1) || (pageNumber == LastPageNumber && direction == 1)) return;

            var data = pageData[pageNumber - 1 + direction];
            pageData[pageNumber - 1 + direction] = pageData[pageNumber - 1];
            pageData[pageNumber - 1] = data;

            SetPageNumber(CurrentPageNumber);
        }

        #endregion
    }
}
