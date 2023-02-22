# Cubespace
Cubespace Application (In-browser client + Linux headless server)

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
