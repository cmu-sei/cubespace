# Cubespace
Cubespace Application (In-browser client + Linux headless server)

---

## Running the Game from Unity and Important Notes

To run the game, open the scene Init_1-16.unity and run it from there.

Important Notes:
- The Gamebrain interface used affects the requests made. This interface is set in the Workstations scene on the ShipStateManager's ShipGameBrainUpdater script's Game Brain Interface field. One of two objects from this scene can be placed here:
    - Local Game Brain Interface
        - Reads from a specified text file that provides a JSON structure to run the game. Note that the data sent here is sent with every poll.
        - Most Gamebrain methods will be spoofed; their callback methods will be called, but the data will not be changed.
        - Set this when testing locally, but set to the below interface before pushing.
    - Net Game Brain Interface
        - Sends requests to a Gamebrain resource to get JSON for the game.
        - Gamebrain methods here will return legitimate data from Gamebrain, but only if this game is running where a Gamebrain resource exists. For all intents and purposes, you won't need to touch this object except to set it as the used Game Brain Interface before pushing to a build pipeline.
        - Set this before pushing to GitLab.
- Running this locally may require modifying the Websocket Transport used (via the NetworkManager). The port is commented out so the game can run on the Kubernetes cluster; running a server instance and a client instance locally will likely require this, so if testing in this manner, uncomment the port.

---

## Building the Game from Unity using the Codebase

Before proceeding, please make sure you have a Unity account and license, whether a personal account or professional one, and that you download the project in "CODEBASE LINK".

1. Download and install Unity LTS version 2021.3.4f1 (available here: https://unity3d.com/unity/qa/lts-releases?page=2), and add the following modules prior to installation:
    - Linux Build Support (Mono)
    - Linux Dedicated Server Build Support
    - WebGL Build Support
2. Open the project using Unity.
3. Go to File > Build Settings.
4. Select WebGL from the left hand list, then select Build and choose a file location.
5. Once WebGL finishes building, select Dedicated Server from the left hand list, then select Linux from the Target Platform dropdown if not already selected.
6. Select Build and choose a file location.


## Turning the WebGL Build into a Docker Image

Ensure the build is placed on a Linux environment before starting.
1. Get Dockerfile.WebGL from the root project folder.
2. Place Dockerfile.WebGL inside the build directory.
3. Run the following commands from a command shell within the build directory:
`sudo docker build -f Dockerfile.WebGL -t unityclient:{version_number} .`


## Turning the Dedicated Server Build into a Docker Image

Ensure the build is placed on a Linux environment before starting.
1. Download Dockerfile.StandaloneLinux64 from the root project folder.
2. Place Dockerfile.StandaloneLinux64 inside the build directory.
3. Run the following commands from a command shell within the build directory:
`sudo docker build -f Dockerfile.StandaloneLinux64 -t unityserver:{version_number} .`


## Running the Builds in Docker

### Client
`sudo docker container run --name UnityClient -p 5000:80 -it unityclient`

### Server
`sudo docker container run --name UnityServer -p 7778:7778 -it unityserver`

Note that the port number can be specified following the -p flag. These port numbers were originally specified for the Foundry appliance.

---

## Configuring the Server

The Linux server can be configured with a series of command line arguments, which are necessary for it to retrieve data, as these represent URIs. Format these as:
`-argumentName "argumentValue"`
and end the argument with a `/`.

### Command Line Arguments
- `-identityURI`: The link to the Identity server, used to retrieve a token.
- `-gamebrainURI`: The link to the Gamebrain server, used to connect to Gamebrain.
- `-uriBase`: The domain where this game resides, used to retrieve a JSON Web Key for the headless client.
- `-clientID`: The OAuth identifier for the headless client.
- `-clientSecret`: The secret stored in Identity for all headless clients (StandaloneLinux64 builds).
- `-dev`: Enables developer shortcuts and hotkeys. This takes no additional values and is only really useful in-editor.
- `-debug`: Enables verbose logging. This takes no additional values.

#### Hotkeys (dev mode only)
- A: Abort launch sequence
- H: Hide HUD
- J: Jump to location (if already chosen)
- V: Skip jump video/transmission


