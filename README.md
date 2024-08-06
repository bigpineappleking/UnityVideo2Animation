# Unity Video2Animation
#### A Unity video to animation conversion tool using MMpose
This is an open source Unity video to animation clip converrsion tool made by Elsie Wang. The idea is to create an accessible tool for indie game makers (such as myself) to create more specific animations by recording live action videos and convert them to Unity animation. Please beaware that at the current stage, **a custom python envrionment setup for the usage of MMpose is required**, contact me if any help is needed.

![Demo](https://github.com/bigpineappleking/UnityVideo2Animation/blob/main/Display/results.gif)

## How to use
* Generate data by following [MMpose 3D video demo](https://github.com/open-mmlab/mmpose/blob/main/demo/docs/en/3d_human_pose_demo.md)

* Download the entire Unity project or **MMpose2Video.unitypackage** only.

* Go to Tool -> MMpose Video2Animation

* **Open Data** to load MMpose 3D keypoints data

* Type in **Animation Save Directory** as animation clip file name, _.anim should not be included_

* Hit **Create Animation**

* View converted animation in Animation window, keypoints should be set to **Clamped Auto**

* Voilà! Enjoy your (perhaps) first motion captured Unity animation

* Edit the animation to add more details

![Tutorial](https://github.com/bigpineappleking/UnityVideo2Animation/blob/main/Display/tutorial.gif)

## Notice
* [Mixamo](https://www.mixamo.com/#/) rig is used for this tool. Please rig your model through Mixamo, custom rigging is currently not supported.

* For any questions / advice, contact me through email or raise an issue here.

* Executable MMpose WIP



