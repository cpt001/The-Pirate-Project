namespace echo17.EndlessBook.Demo02
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using echo17.EndlessBook;

    /// <summary>
    /// View of an animating book on the front page
    /// </summary>
    public class PageView_Front : PageView
    {
        public EndlessBook book;

        public float pageTurnTime = 0.3f;

        public override void Activate()
        {
            base.Activate();

            book.SetPageNumber(1);

            // start the endless loop going back and forth in the book
            OnTurnCompleted(EndlessBook.StateEnum.OpenMiddle, EndlessBook.StateEnum.OpenMiddle, 1);
        }

        public override void Deactivate()
        {
            // stop turning pages when the page is deactivated
            book.StopTurningPages();

            base.Deactivate();
        }

        protected virtual void OnTurnCompleted(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int pageNumber)
        {
            // as soon as the turn is completed, go back the other direction
            book.TurnToPage(pageNumber == 1 ? book.LastPageNumber : 1, EndlessBook.PageTurnTimeTypeEnum.TimePerPage, pageTurnTime, onCompleted: OnTurnCompleted);
        }

    }
}
