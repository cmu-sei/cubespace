/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using Systems.GameBrain;

namespace Systems.CredentialRequests.Models
{
    /// <summary>
    /// A class defining the response received from a call to get a token.
    /// </summary>
    [Serializable]
    public class TokenResponse : IDeserializableFromJSON<TokenResponse>
    {
        // The access token received
        public string access_token;
        // When the access token expires
        public int expires_in;
        // The type of token received
        public string token_type;
        // The scope of the token
        public string scope;

        /// <summary>
        /// General method used to perform additional logic as needed.
        /// </summary>
        public virtual void Initiate()
        {
            // Blank, as there's no logic needed
        }
    }
}

