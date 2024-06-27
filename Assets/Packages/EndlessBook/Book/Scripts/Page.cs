namespace echo17.EndlessBook
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Class to control a page object that animates a turn
    /// </summary>
    public class Page : MonoBehaviour
    {
        /// <summary>
        /// Array of clip lengths by animation
        /// </summary>
        protected float[] animationClipLengths;

        /// <summary>
        /// Page material names
        /// </summary>
        protected const string PageFrontMaterialName = "PageFront";
        protected const string PageBackMaterialName = "PageBack";

        /// <summary>
        /// Page controller hashes for faster updates
        /// </summary>
        protected int AnimationSpeedHash = Animator.StringToHash("AnimationSpeed");
        protected int AnimationDirectionForwardHash = Animator.StringToHash(TurnDirectionEnum.TurnForward.ToString());
        protected int AnimationDirectionBackwardHash = Animator.StringToHash(TurnDirectionEnum.TurnBackward.ToString());

        /// <summary>
        /// The controller of the page
        /// </summary>
        protected Animator controller;

        /// <summary>
        /// The renderer of the page
        /// </summary>
        protected Renderer pageRenderer;

        /// <summary>
        /// The material index of the front page material
        /// </summary>
        protected int pageFrontMaterialIndex;

        /// <summary>
        ///  The material index of the back page material
        /// </summary>
        protected int pageBackMaterialIndex;

        /// <summary>
        ///  Whether the page is reversing due to a manual turn drag that did not go past the middle point
        /// </summary>
        protected bool reversing;

        /// <summary>
        ///  The direction the page is manually being turned
        /// </summary>
        protected int turnDirectionHash;

        /// <summary>
        ///  The opposite direction of the manual turn
        /// </summary>
        protected int oppositeTurnDirectionHash;

        /// <summary>
        ///  The normalized time of the page animation
        /// </summary>
        protected float normalizedTime;

        /// <summary>
        /// Possible directions the page can turn
        /// </summary>
        public enum TurnDirectionEnum
        {
            TurnForward,
            TurnBackward
        }

        /// <summary>
        /// The handler to call when the page turn has completed
        /// </summary>
        public Action<Page> pageTurnCompleted;

        /// <summary>
        /// The index of the page. This is used by the book to
        /// recycle these pages and reuse them in sequence
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The front and back materials
        /// </summary>
        public Material PageFrontMaterial { get { return pageRenderer.sharedMaterials[pageFrontMaterialIndex]; } }
        public Material PageBackMaterial { get { return pageRenderer.sharedMaterials[pageBackMaterialIndex]; } }

        void Awake()
        {
            // cache some components

            controller = GetComponent<Animator>();
            pageRenderer = GetComponentInChildren<Renderer>();

            // cache the animation clip lengths
            if (controller != null)
            {
                animationClipLengths = new float[System.Enum.GetNames(typeof(TurnDirectionEnum)).Length];

                var ac = controller.runtimeAnimatorController;
                for (var i = 0; i < ac.animationClips.Length; i++)
                {
                    var index = (int)(TurnDirectionEnum)System.Enum.Parse(typeof(TurnDirectionEnum), ac.animationClips[i].name);
                    animationClipLengths[index] = ac.animationClips[i].length;
                }
            }

            // index the materials used

            var materials = pageRenderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                if (materials[i].name == PageFrontMaterialName)
                {
                    pageFrontMaterialIndex = i;
                }
                if (materials[i].name == PageBackMaterialName)
                {
                    pageBackMaterialIndex = i;
                }
            }
        }

		void Update()
		{
			// if the page animation is reversing, then we check to see if the animation is complete
			if (reversing)
			{
				var stateInfo = controller.GetCurrentAnimatorStateInfo(0);
				if (stateInfo.normalizedTime <= 0.0f)
				{
					// animation reached the beginning of the clip, so finish
					reversing = false;
					PageAnimationCompleted();
				}
			}
		}

        /// <summary>
        /// Starts the page turn animation
        /// </summary>
        /// <param name="direction">The direction to turn</param>
        /// <param name="time">The time to play the animation</param>
        /// <param name="frontMaterial">The front of the page material</param>
        /// <param name="backMaterial">The back of the page material</param>
        public virtual void Turn(TurnDirectionEnum direction, float time, Material frontMaterial, Material backMaterial)
        {
			// cache the turn direction
			turnDirectionHash = direction == TurnDirectionEnum.TurnForward ? AnimationDirectionForwardHash : AnimationDirectionBackwardHash;
			oppositeTurnDirectionHash = direction == TurnDirectionEnum.TurnForward ? AnimationDirectionBackwardHash : AnimationDirectionForwardHash;

            // turn on the page
            gameObject.SetActive(true);

            // update the materials
            var materials = pageRenderer.sharedMaterials;
            materials[pageFrontMaterialIndex] = frontMaterial;
            materials[pageBackMaterialIndex] = backMaterial;
            pageRenderer.sharedMaterials = materials;

            // set the page turn animation speed: clip length / desired length = speed
			if (time == 0)
			{
				// if the time is set to zero because the page is being manually dragged,
				// then set the animation speed to zero
				controller.SetFloat(AnimationSpeedHash, 0);
			}
			else
			{
				// animation is proceding normally, so set the speed
				controller.SetFloat(AnimationSpeedHash, animationClipLengths[(int)direction] / time);
			}

            // set the triggers to control which animation is playing
            controller.SetTrigger(turnDirectionHash);
        }

        /// <summary>
        /// Set the page normalized time if dragging manually
        /// </summary>
        /// <param name="normalizedTime">The normalized time of the animation</param>
        public virtual void SetPageNormalizedTime(float normalizedTime)
		{
			// cache the normalized time
			this.normalizedTime = normalizedTime;
			// set the normalized time of the animation clip
			controller.Play(turnDirectionHash, layer: -1, normalizedTime: normalizedTime);
		}

        /// <summary>
        /// If dragging manually, play the rest of the animation clip
        /// </summary>
        /// <param name="stopSpeed">The speed of the remainder animation</param>
        /// <param name="reverse">Whether to reverse the animation or not</param>
        public virtual void PlayRemainder(float stopSpeed, bool reverse = false)
		{
			reversing = reverse;

			// set the animation speed
			controller.SetFloat(AnimationSpeedHash, (reverse ? -1 : 1) * stopSpeed);
		}

        /// <summary>
        /// Called when the page animation has completed
        /// </summary>
        public virtual void PageAnimationCompleted()
        {
            // turn off the page
            gameObject.SetActive(false);

            // call the completion handler if necessary
            if (pageTurnCompleted != null) pageTurnCompleted(this);
        }
    }
}
