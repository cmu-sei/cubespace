/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

mergeInto(LibraryManager.library, {

	// Opens a window with the given link at the specified station.
	OpenWindow: function(link, stationID, windowName)
	{
		if (document.win != null && !document.win.closed)
		{
			document.win.close();
		}

    	if (document.getElementById('vm-window') != null)
    	{
			document.getElementById('vm-window').remove(); 
			document.getElementById('close-button').remove();
		}

		var url = Pointer_stringify(link);
		document.win = window.open(url, windowName);
		document.onmouseup = null;
	},
	 
	// Closes a window at the given station.
	CloseWindow: function(stationID)
	{
		if (document.win != null && !document.win.closed)
		{
			document.win.close();
			window.unityInstance.SendMessage('ExternalMessageManager', 'OnCloseVMWindow', stationID);
		}

    	if (document.getElementById('vm-window') != null)
    	{
			document.getElementById('vm-window').remove(); 
			document.getElementById('close-button').remove();
			window.unityInstance.SendMessage('ExternalMessageManager', 'OnCloseVMWindow', stationID);
		}
	},
	
	// Opens a window with the given link in an embedded window at the specified station
	OpenEmbeddedWindow: function(link, stationID)
	{
		if (document.win != null && !document.win.closed)
		{
			document.win.close();
		}

    	if (document.getElementById('vm-window') != null) {
			document.getElementById('vm-window').remove(); 
			document.getElementById('close-button').remove();
		}

		var url = Pointer_stringify(link);
	
		//Create the Window
		var iframe    = document.createElement('iframe');
		iframe.id     = "vm-window";
		iframe.src    = url;
		iframe.width  = "93%";
		iframe.height = "84%" 
		iframe.frameborder = "1";
		iframe.style       = "position: absolute; top:5%; left: 3%";	
		var divReference   = document.getElementById('unity-canvas');
		divReference.after(iframe);
		
		// Create the Close Button
		var closeButton = document.createElement('button');
		closeButton.id     = "close-button";
		closeButton.style  = "position: absolute; top: 3%; left: 90%;";
		closeButton.onclick = function () 
			{ 
				document.getElementById('vm-window').remove(); 
				document.getElementById('close-button').remove();
				window.unityInstance.SendMessage('ExternalMessageManager', 'OnCloseVMWindow', stationID);
			};
		closeButton.innerHTML = "Close";
		iframe.after(closeButton);
	},
});