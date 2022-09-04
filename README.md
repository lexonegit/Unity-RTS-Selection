# Fork

### v 0.0.2

Extracted CameraDirectionMovement and CameraRotation from CameraMovement then removed it.

Added CameraPan, CameraZoom, CameraEdgeScroll, and CursorLock features and scripts.


### v 0.0.1

https://www.reddit.com/r/Unity3D/comments/wzvzzm/comment/in0sgrx/

- Encapsulated the classes, so not everything serialized is public when it doesn't need to be.

- Reduced complexity in selection script down to 100 lines from 300 (Mostly by extracting out functionality into single responsibility classes) New Mesh Builder class and New Selection Box class.

- Added Hover Support

- Added a new GameData scriptable object for global settings and input configuration.

The changes are mostly just moving the logic around and cleaning up minor parts; selection logic is mostly the same implementation.

Thanks for sharing your work; I hope you can check this fork out!PS: Thanks for explaining why a BoxCastAll doesn't work! I hadn't considered the impact of a perspective camera as my previous project was ortho camera in editor and VR builds.PS:PS: I tried doing a _rigidbody.SweepAll() to try and work around the quirky coroutine yielding to fixed update, but that really wasn't a great path. Kind of sad there isn't a Overlapping raycast option for rigidbodies

https://i.imgur.com/gmV4xIX.mp4




# Unity RTS Selection

- Select *units* in the style of various RTS games (Age of Empires, etc...)
- Also supports single selecting and adding or removing from current selection
- Clean and efficient raycast based implementation
- Works for both perspective and orthographic cameras

<img src="https://user-images.githubusercontent.com/18125997/187076649-d7674b41-85af-4979-ad58-7fb7e7a4832b.gif" width="60%">

## Getting started

- Check out the included example scene to see how it all works

## Credit

The core idea (mesh collider approach) is based on <a href="https://youtu.be/OL1QgwaDsqo?t=26">this video</a>
