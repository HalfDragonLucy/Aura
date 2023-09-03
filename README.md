
# AuraDDX

![](https://github.com/HalfDragonLucy/AuraDDX/blob/master/fav.png)

**Table of Contents**
- [Introduction](#introduction)
- [Program Overview](#program-overview)
- [Documentation](#documentation)
  - [Installation](#installation)
  - [Usage](#usage)
  - [Command Line Arguments](#command-line-arguments)
  - [File Extension Registration](#file-extension-registration)
  - [Viewing DDX Images](#viewing-ddx-images)
  - [Cleanup](#cleanup)
- [License](#license)

## Introduction

AuraDDX Viewer is a Windows application designed for viewing DDX image files. It offers functionality for registering and unregistering the program to open DDX files and converting and displaying images in PNG format.

## Program Overview

The program is structured into several components:

- **Extension Management (`AuraDDX.Extension`)**: Handles the registration and unregistration of file extensions for the application.
- **File Structure Integrity (`AuraDDX.Integrity`)**: Ensures the integrity of the application's file structure by creating necessary directories and checking for the existence of essential files, such as `texconv.exe`.
- **DirectX Image Conversion (`AuraDDX.DirectX`)**: Provides functionality for converting images to the PNG format using the `texconv.exe` utility.
- **Viewer Application (`AuraDDX.Viewer`)**: The main Windows Forms application responsible for viewing DDX images and managing file extensions.

## Documentation

### Installation

1. Make sure you have [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. Download the latest release; this program comes with a `setup.exe` for easy installation.
3. Run the `setup.exe` installer.
4. Follow the installation instructions, and when prompted, click 'Yes' to create a shortcut on your desktop.
5. Enjoy opening DDX Images.

### Usage

The AuraDDX Viewer can be used both from the command line or by simply opening a DDX file.


Replace `<path_to_ddx_image>` with the path to the DDX image file you want to view.

#### Graphical User Interface (GUI)

1. Run the `AuraDDXViewer.exe` executable.
2. If the program is launched without any command line arguments, it will display a registration menu to associate DDX files with the program.
3. If you provide a DDX image file as a command line argument, the program will convert and display the image.

### Command Line Arguments

- `<path_to_ddx_image>`: The path to the DDX image file you want to view. Must be provided without any other flags or parameters.

### File Extension Registration

- When you launch the program without any command line arguments, it will ask if you want to register or unregister the program to open DDX files.
- If you choose to register, the program will associate itself with the `.ddx` file extension, allowing you to open DDX images by double-clicking them in File Explorer.
- If you choose to unregister, the program will disassociate itself from the `.ddx` file extension.

### Viewing DDX Images

- When you provide a DDX image file as a command line argument, the program will convert it to PNG format using `texconv.exe` and display the resulting image.
- The converted PNG image is displayed in the program's GUI.

### Cleanup

- When the program's GUI is closed, it automatically deletes temporary PNG files created during image conversion.

## License

This project is licensed under the [MIT License](https://github.com/HalfDragonLucy/AuraDDX/blob/master/LICENSE.txt).