/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

mergeInto(LibraryManager.library, 
{
	// Retrieves information from local storage to be used for token information and the game server link.
	GetTokenInfo: function () 
	{
		// read the teamId out of the querystring
		const urlParams = new URLSearchParams(window.location.search);
		console.info("UrlParams:", urlParams.toString());
		const teamId = urlParams.get("teamId");
		console.info("TeamId", teamId);

		// Get local storage items
		const gameData = window.localStorage.getItem(`externalGame:${teamId}`);
		console.info("Game data", gameData);
		const tokenUri = gameData.oidcLink;
		const tokenString = window.localStorage.getItem(tokenUri);
		const serverLink = gameData.gameServerUrl;
		console.info("serverLink", serverLink);

		// Send some variables to the Unity game through messages
		window.unityInstance.SendMessage("TokenHandler", "RecieveTokenInfo", "" + tokenString);
		window.unityInstance.SendMessage("TokenHandler", "ReceiveServerLink", "" + serverLink);
	}	
});
