TouchKit
====

TouchKit aims to make touch handling in Unity more sane. Touches in TouchKit are actual objects as opposed to Structs like Unity uses by default. The advantage to this is that touch tracking becomes orders of magnitude more simple. You can retain a touch that began and since you are only holding on to a pointer (as opposed to a Struct) the properites of that touch will be updating as the touch changes. TouchKit doesn't save too much time for simple, single-tap processing. It's usefulness is in detecting and managing gestures (hence the original name before the lovely trademark owner complained: GestureKit).

TouchKit allows gesture recognizers to act on the entire screen or they can define a Rect in which to do detection. If a touch doesn't orginate in the Rect the touches won't be passed on to the recognizer (except for the Any Touch Recognizer).


Included Gesture Recognizers
---

TouchKit comes with a few built in recognizers to get you started and to serve as an example for how to make your own. Included are the following:
* **Tap Recognizer**: detects one or more taps from one or more fingers
* **Long Press Recognizer**: detects long-presses with configurable duration and allowed movement. Fires when the duration passes and when the gesture is complete.
* **Button Recognizer**: generic button recognizer designed to work with any 2D sprite system at all
* **Pan Recognizer**: detects a pan gesture (one or more fingers down and moving around the screen)
* **TouchPad Recognizer**: detects and tracks a touch in an area and maps the location from -1 to 1 on the x and y axis based on the touches distance from the center of the area
* **Swipe Recognizer**: detects swipes in the four cardinal directions
* **Pinch Recognizer**: detects pinches and reports back the delta scale
* **Rotation Recognizer**: detects two finger rotation and reports back the delta rotation
* **One Finger Rotation Recognizer**: pass it a target object's center position and it will report back the delta rotation of a single finger
* **Any Touch Recognizer**: fires enter/exit events whenever a touch enters/exist the boundary. the difference here is that it will allow a touch to begin outside of it's frame and then move into its frame. handy for directional buttons.


How Do I Use TouchKit?
-----

If you are just using the built in recognizers, check out the demo scene which shows how to use them. You will want to use Unity's script execution order (Edit -> Project Settings -> Script Execution Order) to ensure that TouchKit executes before your other scripts. This is just to ensure you have your input when the Update method on your listening objects runs.

When working with recognizers that are not full screen, the TKRect class is used to define the rectangle. TouchKit has an automatic scaling system built in that is turned on by default. What that means is that you set your TKRect sizes only once for any screen size and density. By default, the design time resolution (**TouchKit.designTimeResolution**) that TouchKit uses is 320 x 180. You can change that to whatever you want. When you create your TKRects you set the size and origin based on that exact screen size. At runtime, TouchKit will scale the rects based on the actual screen resolution.

If you want to make your own (and feel free to send pull requests if you make any generic recognizers!) all you have to do is subclass **TKAbstractGestureRecognizer** and implement the three methods: touchesBegan, touchesMoved and touchesEnded. Recognizers can be discrete (they only complete once like a tap or long touch) or continous (they can fire continuously like a pan). Recognizers use the **state** variable to determine if they are still active, completed or failed. If you set the state to **Recognized**, the completion event is fired automatically for you and the recognizer is reset in preparation for the next set of touches.

Starting with **touchesBegan**, if you find a touch that look interesting you can add it to the **_trackingTouches** List and set the **state** to **Began**. By adding the touch to the **_trackingTouches** List you are telling TouchKit that you want to receive all future touch events that happen with that touch. As the touch moves, **touchesMoved** will be called where you can look at the touches and decide if the gesture is still possible. If it isn't, set the state to **Failed** and TouchKit will reset the recognizer for you.

Using the Tap recognizer as an example, the flow would be like the following:
* **touchesBegan**: add the touch to **_trackingTouches** signifying we are watching it. Set the state to **Began** and record the current time (we don't want a press that is too long to be considered a tap)
* **touchesMoved**: check the deltaMovement of the touch and if it moves too far set the **state** to **Failed** signifying the gesture failed and we are done with the touch
* **touchesEnded**: if not too much time has elapsed we successfully recognized the gesture so we set **state** to **Recognized** which will fire the event for us. If too much time elapsed we set **state** to **Failed**



UI Touch Handling Replacement
----

TouchKit can be used for any and all touch input in your game. The **TKButtonRecognizer** class has been designed to work with any sprite solution for button touch handling. This lets you keep your input totally separate from your rendering. It implements the same setup that iOS does: a highlighted button expands its hit rect for better useability.


License
----
For any developers just wanting to use TouchKit in their games go right ahead.  You can use TouchKit in any and all games either modified or unmodified.  In order to keep the spirit of this open source project it is expressly forbid to sell or commercially distribute TouchKit outside of your games. You can freely use it in as many games as you would like but you cannot commercially distribute the source code either directly or compiled into a library outside of your game.

Feel free to include a "prime31 inside" logo on your about page, web page, splash page or anywhere else your game might show up if you would like.
[small](http://prime31.com/assets/images/prime31InsideSmall.png) or
[larger](http://prime31.com/assets/images/prime31Inside.png) or
[huge](http://prime31.com/assets/images/prime31InsideHuge.png)
