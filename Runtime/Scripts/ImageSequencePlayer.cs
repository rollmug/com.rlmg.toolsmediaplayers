namespace rlmg.Tools.MediaPlayers
{
    using System;
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// A 'movie' player component for playback of an image sequence in the Canvas
    /// </summary>
    public class ImageSequencePlayer : MonoBehaviour
    {
        /// <summary>
        /// Array containing loaded frames
        /// </summary>
        public Sprite[] frames;

        /// <summary>
        /// If true, Update will update the currFrame count and shown image
        /// </summary>
        public bool VideoMoving = true;

        /// <summary>
        /// time within the duration of the image sequence
        /// without looping, will hover around the start or end of the duration
        /// once the video plays through and is still playing
        /// </summary>
        private float currTimecode = 0f;

        /// <summary>
        /// Rate at which display will update
        /// Given by 1 sec / frameRate
        /// So 30fps will yield 0.0333f
        /// </summary>
        public float updateRate = 0.02f;

        /// <summary>
        /// should the video once it reaches its start or end
        /// </summary>
        public bool doLoop = false;
        public bool doReverse = false;

        /// <summary>
        /// Helper int for onFirstFrameReached and onLastFrameReached UnityEvents
        /// Tracks what the past frame was, to compare with the current frame
        /// </summary>
        private int pastFrame = -1;

        /// <summary>
        /// Called once if the first frame is reached after playing in reverse from a later frame
        /// </summary>
        public UnityEvent onFirstFrameReached;

        /// <summary>
        /// Called once if the last frame is reached after playing from an earlier frame
        /// </summary>
        public UnityEvent onLastFrameReached;

        /// <summary>
        /// current frame, set in Update loop to be currTimecode / Abs(updateRate)
        /// so that, at a currTimecode of 1 sec, for an updateRate of 0.02
        /// currFrame denotes the 50th frame
        /// </summary>
        private int currFrame = 0;
        public Int32 CurrentFrameNum
        {
            get
            {
                return currFrame;
            }
            set
            {
                SetFrame(value);
            }
        }

        /// <summary>
        /// Count of all frames
        /// </summary>
        public float TotalFrameNum { get { return frames == null ? 0 : frames.Length; } }

        private bool didSetPercentThisFrame = false;

        /// <summary>
        /// Target RawImage display for instance
        /// </summary>
        public RawImage rawImage;

        /// <summary>
        /// Target Image display for instance
        /// </summary>
        public Image uiImage;

        //	public MeshRenderer movieMesh;
        //private Material movieMaterial;
        //	private Texture movieTexture;

        /// <summary>
        /// Target SpriteRenderer display for instance
        /// </summary>
        public SpriteRenderer spriteRenderer;

        private float targetPercent = 0f;

        /// <summary>
        /// Calculated duration of image sequence played at current updateRate
        /// </summary>
        public float duration
        {
            get
            {
                if (frames != null)
                    return updateRate * frames.Count();
                else
                    return 0f;
            }
        }

        /// <summary>
        /// Is player on its last frame?
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (frames != null)
                    return CurrentFrameNum >= frames.Length - 1;
                else
                    return false;
            }
        }


        /// <summary>
        /// Seek to given percentage progress through image sequence
        /// </summary>
        /// <param name="percent"></param>
        public void SetPercent(float percent)
        {
            Percent = percent;
        }

        /// <summary>
        /// Progress through image sequence
        /// </summary>
        public float Percent
        {
            get
            {
                return targetPercent;
            }
            set
            {
                targetPercent = value;

                didSetPercentThisFrame = true;
            }
        }

        /// <summary>
        /// Seek to given frame
        /// </summary>
        /// <param name="targetFrame"></param>
        public void SetFrame(int targetFrame)
        {
            if (frames == null || frames.Length == 0)
                return;

            if (targetFrame > frames.Length - 1)
                targetFrame = frames.Length - 1;

            currFrame = targetFrame;

            if (frames[targetFrame] != null)
            {
                if (rawImage != null)
                    rawImage.texture = frames[targetFrame].texture;

                if (uiImage != null)
                    uiImage.sprite = frames[targetFrame];

                if (spriteRenderer != null)
                    spriteRenderer.sprite = frames[targetFrame];
            }

            currTimecode = ((float)currFrame / (float)(frames.Length - 1)) * duration;

            pastFrame = -1;
        }

        /// <summary>
        /// Seek to last frame
        /// </summary>
        public void SetLastFrame()
        {
            int targetFrame = frames.Length - 1;
            SetFrame(targetFrame);
        }

        /// <summary>
        /// Stops Update loop from updating currFrame or display
        /// </summary>
        public void Pause()
        {
            VideoMoving = false;
        }

        /// <summary>
        /// Continues Update loop in updating currFrame and display
        /// </summary>
        public void Play()
        {
            VideoMoving = true;
        }


        /// <summary>
        /// Changes direction of video playback
        /// </summary>
        /// <param name="value">If true, will playback in reverse.</param>
        public void SetReverse(bool value)
        {
            doReverse = value;
        }

        protected virtual void Update()
        {
            if (frames == null || frames.Length < 1)
                return;

            if (VideoMoving && !didSetPercentThisFrame)
            {
                // if (updateRate >= 0)
                //     currFrame++;
                // else
                //     currFrame--;

                currFrame = Mathf.FloorToInt(currTimecode / Mathf.Abs(updateRate));

                if (currFrame > frames.Length - 1)
                {
                    //Debug.Log("reached end of image sequence.   currFrame = " + currFrame + "   doLoop = " + doLoop);

                    if (doLoop)
                    {
                        pastFrame = currFrame;

                        currFrame = 0;
                        currTimecode = 0f;
                        //currTimecode -= frames.Length * updateRate;
                    }
                    else
                    {
                        if (pastFrame <= frames.Length - 1)
                            onLastFrameReached.Invoke();

                        pastFrame = currFrame;

                        currFrame = frames.Length - 1;
                        currTimecode = frames.Length * updateRate;
                    }
                }
                else if (currFrame < 0)
                {
                    if (doLoop)
                    {
                        pastFrame = currFrame;

                        currFrame = frames.Length - 1;
                        currTimecode = frames.Length * Mathf.Abs(updateRate);
                        //currTimecode += frames.Length * updateRate;
                    }
                    else
                    {
                        if (pastFrame >= 0)
                        {
                            onFirstFrameReached.Invoke();
                        }

                        pastFrame = currFrame;

                        currFrame = 0;
                        currTimecode = 0f;
                    }
                }
                else
                {
                    pastFrame = currFrame;
                }

                if (rawImage != null)
                    rawImage.texture = frames[currFrame] != null ?
                        frames[currFrame].texture :
                        null;

                if (uiImage != null)
                    uiImage.sprite = frames[currFrame];

                if (spriteRenderer != null)
                    spriteRenderer.sprite = frames[currFrame];

                targetPercent = currFrame / frames.Length;

                if (updateRate > 0)
                {
                    if (doReverse)
                        currTimecode -= Time.deltaTime;
                    else
                        currTimecode += Time.deltaTime;
                }

                else if (updateRate < 0)
                {
                    if (doReverse)
                        currTimecode += Time.deltaTime;
                    else
                        currTimecode -= Time.deltaTime;
                }

            }
            else
            {
                float currPercent = 1f * currFrame / frames.Length;

                //print("curr: " + currPercent + ", targ: " + targetPercent);

                //float percent = Mathf.Lerp(currPercent, targetPercent, Time.deltaTime * 8f);
                float percent = targetPercent;

                int index = Mathf.FloorToInt(frames.Length * percent);

                currFrame = Mathf.Clamp(index, 0, frames.Length - 1);

                if (frames[currFrame] != null)
                {
                    if (rawImage != null)
                        rawImage.texture = frames[currFrame].texture;

                    if (uiImage != null)
                        uiImage.sprite = frames[currFrame];

                    if (spriteRenderer != null)
                        spriteRenderer.sprite = frames[currFrame];
                }

                currTimecode = currFrame / frames.Length;

                pastFrame = currFrame;
            }
        }

        private void LateUpdate()
        {
            didSetPercentThisFrame = false;
        }
    }

    public class mySorter : IComparer
    {
        int IComparer.Compare(object a, object b)
        {
            string nameA = ((Texture2D)a).name;
            string nameB = ((Texture2D)b).name;

            int indexA, indexB = 0;

            //int.TryParse(nameA.Substring(nameA.IndexOf(" ") + 1), out indexA);
            //int.TryParse(nameB.Substring(nameB.IndexOf(" ") + 1), out indexB);

            int.TryParse(nameA.Substring(nameA.IndexOf(" ", StringComparison.CurrentCulture) + 1), out indexA);
            int.TryParse(nameB.Substring(nameB.IndexOf(" ", StringComparison.CurrentCulture) + 1), out indexB);

            return (indexA.CompareTo(indexB));
        }
    }

}