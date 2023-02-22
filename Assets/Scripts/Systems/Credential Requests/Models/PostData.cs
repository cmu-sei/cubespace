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

namespace Systems.CredentialRequests.Models
{
    /// <summary>
    /// The data used by the server to get a team ID it can be associated with. 
    /// <para>
    /// This data includes the token used by a WebGL client and the name of the machine the server is running on.
    /// </para>
    /// </summary>
    public class PostData
    {
        // The token of a client, used by the server to retrieve a team ID when sent to the server
        public string user_token;
        // The name of the machine the server runs on
        public string server_container_hostname = Environment.MachineName;

        /// <summary>
        /// Constructor that takes a provided client token to create a PostData object.
        /// </summary>
        /// <param name="clientToken">The token used by a player in a WebGL client.</param>
        public PostData(string clientToken)
        {
            this.user_token = clientToken;
        }
    }
}
