/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

namespace Entities.Workstations.FlightEngineerParts
{
    /// <summary>
    /// A class representing the current angle of a dial and its target angle.
    /// </summary>
    public struct DialInfo
    {
        #region Variables
        /// <summary>
        /// The current angle of a dial.
        /// </summary>
        public int value;
        /// <summary>
        /// The target angle of a dial.
        /// </summary>
        public int target;
        #endregion

        #region Contructor
        /// <summary>
        /// Constructor initializing this dial information.
        /// </summary>
        /// <param name="value">The angle the dial is currently set to.</param>
        /// <param name="target">The angle the dial should be turned to.</param>
        public DialInfo(int value, int target)
        {
            this.value = value;
            this.target = target;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks whether the dial has been turned towards its target angle.
        /// </summary>
        /// <returns>Whether the dial has reached its target.</returns>
        public bool IsValueAtTarget()
        {
            return target == WorkstationDial.FormatAngle(value);
        }
        #endregion
    }
}

