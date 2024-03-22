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
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Systems.GameBrain
{
	/// <summary>
	/// Static class that is used to make web requests.
	/// </summary>
	public static class NetUtility
	{
		/// <summary>
		/// A data type used to represent the type of HTTP method to use in a web request.
		/// </summary>
		public enum HTTPMethod
        {
			GET = 0,
			HEAD = 1,
			POST = 2,
			PUT = 3,
			DELETE = 4
        }

		/// <summary>
		/// A coroutine that initiates and waits for a web request usign the provided HTTP method.
		/// </summary>
		/// <param name="uri">The URI to access in the web request.</param>
		/// <param name="response">The response received from the web request.</param>
		/// <param name="httpMethod">The HTTP method to use in this request.</param>
		/// <param name="token">The token to send in this request.</param>
		/// <param name="requestBody">The body of the request.</param>
		/// <param name="contentType">The type of content to receive.</param>
		/// <returns>A yield return while waiting for the web request to finish.</returns>
		public static IEnumerator WebRequest(string uri, Action<string> response, HTTPMethod httpMethod = HTTPMethod.GET, string token = null, string requestBody = null, string contentType = null)
		{
			// The default method used is GET
			UnityWebRequest webRequest = UnityWebRequest.Get(uri);
			switch (httpMethod)
            {
				// No further logic needed if the HTTP method needed is GET
				case HTTPMethod.GET:
					break;
				// If the HTTP method used is a POST method instead, set the request header and upload handler using the other parameters
				case HTTPMethod.POST:
					webRequest = UnityWebRequest.Post(uri, "");
					webRequest.SetRequestHeader("content-type", contentType);
					webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestBody));
					break;
				// If the method is not one of the ones cased above, we cannot proceed with the request
				default:
					Debug.LogWarning("Unsupported web request type provided. The type provided should only be GET or POST.");
					break;
			}

			// Initiate a web request
			using (webRequest)
			{
				// If the token has been provided, we need to include it in the headers
				if (token != null)
				{
					webRequest.SetRequestHeader("accept", "text/plain");
					webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
				}

				// Request and wait for the desired page
				yield return webRequest.SendWebRequest();

				// Get the last page in case an error occurs so we can debug
				string[] pages = uri.Split('/');
				string page = pages[pages.Length - 1];

				// Handle failure or success
				switch (webRequest.result)
				{
					case UnityWebRequest.Result.ConnectionError:
					case UnityWebRequest.Result.DataProcessingError:
                        //Debug.LogError($"{page}: Error: {webRequest.error}");
                        Debug.LogError($"{uri}: Error: {webRequest.error}");
                        yield break;
					case UnityWebRequest.Result.ProtocolError:
						//Debug.LogError($"{page}: HTTP Error: {webRequest.error}");
                        Debug.LogError($"{uri}: HTTP Error: {webRequest.error}");
                        yield break;
					case UnityWebRequest.Result.Success:
						response.Invoke(webRequest.downloadHandler.text);
						break;
				}
			}
		}
	}
}
