# Deploy
A self-contained program that packages and deploys files and allows scripts
 to run on local and remote nodes via SSH

## How to install
1. Download the latest version from [releases](releases)
2. Move the file `deploy.exe` to the installation folder
2. Run `deploy.exe --install`

## How to build
1. Clone this repository
2. Open a console on the repository root
3. Run `package.cmd`
4. The program will be located at the `target` folder

## Commands
- `deploy.exe --init` creates a new configuration file
- `deploy.exe --help` displays the available commands
- `deploy.exe --install` installs the command to the `%Path%`
- `deploy.exe {path-to-config-file}` runs the deployment:
  - The`.json` extension will be included if not specified
  - If no path is used, the program searches for a file named `deploy.json` in the current working directory
  - See [Path properties](#path-properties) for common properties

## Configuration file
The configuration file extension must be `.json`. All the paths in the configuration 
file have common properties (see [Path properties](#path-properties)). The `env` property is set as an 
environment variable for local (`%deploy_env%`) and remote (`$deploy_env`) events.
```json
{
    "hostname": "", // the SSH hostname (domain.com) or ip (192.168.0.1) [required]
    "port": 22, // the SSH port [default: 22]
    "username": "", // the SSH username [required]
    "privatekey": "", // the path to the SSH private key [required]

    "env": "", // the environment name (local, virtualbox, prod, ...)
    "sourcepath": "", // the file or folder to be packaged on the local node [required]
    "uploadpath": "/tmp", // the temporary folder for unpackaging on the remote node [required]
    "cleanuponexit": true, // remove temporary files and folders on exit [default: true]

    "localshell": { // events run on local shell (e.g. command.cmd, script.ps1, program.exe)
        "beforepackage": "", // run before packaging
        "afterpackage": "", // run after packaging
        "beforeupload": "", // run before upload
        "afterupload": "" // run after packaging
    },

    "remoteshell": { // events run on remote shell (e.g. script.sh, program)
        "beforepackage": "", // run before packaging
        "afterpackage": "", // run after packaging
        "beforeupload": "", // run before upload
        "afterupload": "" // run after packaging
    }
}
```

## Path properties
- The paths can be absolute or relative to the configuration file path
- Environment variables (`%UserProfile%`, `%Temp%`, ...) in the path are replaced
- If the path starts with `~`, the tilde will be replaced with `%UserProfile%` 
