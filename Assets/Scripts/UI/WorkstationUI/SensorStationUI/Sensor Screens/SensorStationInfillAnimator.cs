/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UI.ColorPalettes;

namespace UI.SensorScreen.SensorScreenComponents
{
    /// <summary>
    /// The animator at the sensor station that fills in "pie slices" on the screen.
    /// </summary>
    public class SensorStationInfillAnimator : SensorScreen
    {
        /// <summary>
        /// The text to show when scanning.
        /// </summary>
        private const string SCANNING_TEXT = "SCANNING";

        /// <summary>
        /// The number of animation ticks per second.
        /// </summary>
        public float animationTicksPerSec = 5f;
        /// <summary>
        /// The array of pie slices.
        /// </summary>
        [SerializeField]
        private InfillPieSlice[] _pieSlices;
        /// <summary>
        /// The text object displaying the process at the sensor station.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI _text;

        /// <summary>
        /// The current animation playing.
        /// </summary>
        private Coroutine _currentAnim = null;

        #region Unity functions
        /// <summary>
        /// Unity event function that scans the text.
        /// </summary>
        private void Start()
        {
            if (_pieSlices.Length != 8)
            {
                Debug.LogError("Incorrect number of pie slices set for sensor station in inspector!!");
            }
            
            InfillPieSlice[] orderedPieSlices = new InfillPieSlice[8];
            
            foreach (InfillPieSlice p in _pieSlices)
            {
                orderedPieSlices[(int)p.direction] = p;
            }

            if (orderedPieSlices.Any((p) => p.innerImages.images == null))
            {
                Debug.LogError("Pie Slices set incorrectly in editor. Must have more than one assigned to a particular direction");
            }

            _text.text = SCANNING_TEXT;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Sets all infill values within the pie slice.
        /// </summary>
        /// <param name="enabled">Whether to enable all images.</param>
        /// <param name="color">The color to give each infill.</param>
        private void SetAllInfills(bool enabled, Color? color = null)
        {
            foreach (InfillPieSlice p in _pieSlices)
            {
                if (color == null)
                {
                    p.SetAllImages(enabled, Color.clear);
                }
                else
                {
                    p.SetAllImages(enabled, (Color) color);
                }
            }
        }

        /// <summary>
        /// Stops the current animation and clears existing infill values.
        /// </summary>
        /// <param name="color">The color to fill in each pie slice with.</param>
        private void StopAnimationAndClearInfills(Color? color = null)
        {
            if (_currentAnim != null)
            {
                StopCoroutine(_currentAnim);
                _currentAnim = null;
            }
            SetAllInfills(false, color);  
        }
        #endregion

        #region Public animate functions
        /// <summary>
        /// Activates the scan animation.
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            ActivateScanAnimation();
        }

        /// <summary>
        /// Stops the scan animation.
        /// </summary>
        public override void Deactivate()
        {
            StopAnimation();
            base.Deactivate();
        }

        /// <summary>
        /// Activates the scan animation.
        /// </summary>
        public void ActivateScanAnimation()
        {
            AnimatePieSliceCircle(new PieSliceZone[3] { PieSliceZone.inner, PieSliceZone.middle, PieSliceZone.outer }, ColorPalette.GetColor(PaletteColor.Powered));
        }

        /// <summary>
        /// Stops the scan animation.
        /// </summary>
        public void StopAnimation()
        {
            StopAnimationAndClearInfills();
        }

        /// <summary>
        /// Animates an infill of an individual pie slice.
        /// </summary>
        /// <param name="dir">The direction the animate the pie slice in.</param>
        /// <param name="color">The color to fill in the pie slice with.</param>
        public void AnimateIndividualPieSlice(PieSliceDirection dir, Color color)
        {
            StopAnimationAndClearInfills();
            _currentAnim = StartCoroutine(IndividualPieSliceAnim(_pieSlices[(int) dir], color));
        }

        /// <summary>
        /// Animates the infill of the pie slice circle.
        /// </summary>
        /// <param name="zones">Each pie slice.</param>
        /// <param name="color">The color to use </param>
        public void AnimatePieSliceCircle(PieSliceZone[] zones, Color color)
        {
            StopAnimationAndClearInfills(color);
            _currentAnim = StartCoroutine(PieSliceCirleAnim(zones));
        }

        /// <summary>
        /// Animates the sonar.
        /// </summary>
        /// <param name="color">The color to animate the sonar with.</param>
        public void AnimateSonar(Color color)
        {
            StopAnimationAndClearInfills(color);
            _currentAnim = StartCoroutine(SonarAnim());
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// An animation for filling in an individual pie slice.
        /// </summary>
        /// <param name="slice">The slice of the pie to fill in.</param>
        /// <param name="color">The color used to fill in the slice.</param>
        /// <returns>A yield statement while waiting for a number of seconds.</returns>
        private IEnumerator IndividualPieSliceAnim(InfillPieSlice slice, Color color)
        {
            slice.color = color;
            WaitForSeconds _waitForSecondsFunc = new WaitForSeconds(1f / animationTicksPerSec);

            while (true)
            {
                slice.outerImages.allEnabled = false;
                slice.innerImages.allEnabled = true;
                yield return _waitForSecondsFunc;

                slice.innerImages.allEnabled = false;
                slice.middleImages.allEnabled = true;
                yield return _waitForSecondsFunc;

                slice.middleImages.allEnabled = false;
                slice.outerImages.allEnabled = true;
                yield return _waitForSecondsFunc;
            }
        }

        /// <summary>
        /// The animation of each pie slice filling up.
        /// </summary>
        /// <param name="zones">The zones of the pie slice to fill in.</param>
        /// <returns>A yield statement while trying to fill in </returns>
        private IEnumerator PieSliceCirleAnim(PieSliceZone[] zones)
        {
            WaitForSeconds _waitForSecondsFunc = new WaitForSeconds(1f / animationTicksPerSec);
            InfillPieSlice _prevPieSlice = _pieSlices[0];

            while (true)
            {
                for (int i = 0; i < _pieSlices.Length; i++)
                {
                    foreach (PieSliceZone zone in zones)
                    {
                        switch (zone)
                        {
                            case PieSliceZone.inner:
                                _prevPieSlice.innerImages.allEnabled = false;
                                _pieSlices[i].innerImages.allEnabled = true;
                                break;
                            case PieSliceZone.middle:
                                _prevPieSlice.middleImages.allEnabled = false;
                                _pieSlices[i].middleImages.allEnabled = true;
                                break;
                            case PieSliceZone.outer:
                                _prevPieSlice.outerImages.allEnabled = false;
                                _pieSlices[i].outerImages.allEnabled = true;
                                break;
                        }
                    }
                    _prevPieSlice = _pieSlices[i];
                    yield return _waitForSecondsFunc;
                }
            }
        }

        /// <summary>
        /// Animates the sonar while scanning the location by filling in the slices.
        /// </summary>
        /// <returns>A yield while waiting for the sonar to finish scanning.</returns>
        private IEnumerator SonarAnim()
        {
            WaitForSeconds _waitForSecondsFunc = new WaitForSeconds(1f / animationTicksPerSec);
            PieSliceZone[] _zones = new PieSliceZone[3] { PieSliceZone.inner, PieSliceZone.middle, PieSliceZone.outer };

            while (true)
            {
                foreach (PieSliceZone zone in _zones)
                {
                    for (int i = 0; i < _pieSlices.Length; i++)
                    {
                        switch (zone)
                        {
                            case PieSliceZone.inner:
                                _pieSlices[i].outerImages.allEnabled = false;
                                _pieSlices[i].innerImages.allEnabled = true;
                                break;
                            case PieSliceZone.middle:
                                _pieSlices[i].innerImages.allEnabled = false;
                                _pieSlices[i].middleImages.allEnabled = true;
                                break;
                            case PieSliceZone.outer:
                                _pieSlices[i].middleImages.allEnabled = false;
                                _pieSlices[i].outerImages.allEnabled = true;
                                break;
                        }
                    }
                    yield return _waitForSecondsFunc;
                }
            }
        }
        #endregion
    }

    #region Structs and enums
    /// <summary>
    /// The slice of the "pie" image that is filled in by the animation.
    /// </summary>
    [System.Serializable]
    public struct InfillPieSlice
    {
        /// <summary>
        /// The parent container of the pie slice.
        /// </summary>
        public GameObject parentObject;
        /// <summary>
        /// The direction the infill animation is moving in.
        /// </summary>
        public PieSliceDirection direction;
        /// <summary>
        /// The innermost images of the pie.
        /// </summary>
        public MultiPartImage innerImages;
        /// <summary>
        /// The middlemost images of the pie.
        /// </summary>
        public MultiPartImage middleImages;
        /// <summary>
        /// The outermost images of the pie.
        /// </summary>
        public MultiPartImage outerImages;
        /// <summary>
        /// The color to use.
        /// </summary>
        private Color _color;

        /// <summary>
        /// The color of this image. Derives from a private variable.
        /// </summary>
        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                innerImages.color = value;
                middleImages.color = value;
                outerImages.color = value;
                _color = value;
            }
        }

        /// <summary>
        /// Sets whether images are enabled and the color used on this screen.
        /// </summary>
        /// <param name="enabled">Whether the inner, outer, and middle images are enabled.</param>
        /// <param name="newColor">The new color to make the infill image use.</param>
        public void SetAllImages(bool enabled, Color newColor)
        {
            innerImages.allEnabled = enabled;
            middleImages.allEnabled = enabled;
            outerImages.allEnabled = enabled;

            color = newColor;
        }
    }

    /// <summary>
    /// A structure for a multi-part image.
    /// </summary>
    [System.Serializable]
    public struct MultiPartImage
    {
        /// <summary>
        /// The image used 
        /// </summary>
        public Image[] images;
        /// <summary>
        /// Whether all images within the infill image are enabled.
        /// </summary>
        private bool _allEnabled;
        /// <summary>
        /// Whether all images within the infill image are enabled. Derives from a private variable.
        /// </summary>
        public bool allEnabled
        {
            get
            {
                return _allEnabled;
            }
            set
            {
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].enabled = value;
                }
                _allEnabled = value;
            }
        }

        /// <summary>
        /// The color used for this infill animation.
        /// </summary>
        private Color _color;
        
        /// <summary>
        /// The color used for this infill animation. Derives from a private variable.
        /// </summary>
        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].color = value;
                }
                _color = value;
            }
        }
    }

    /// <summary>
    /// The directions the pie slice on the screen highlights in.
    /// </summary>
    public enum PieSliceDirection
    {
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }

    /// <summary>
    /// The zone of the pie slice highlighted.
    /// </summary>
    public enum PieSliceZone
    {
        inner,
        middle,
        outer
    }
    #endregion
}

