Weather Maker, (c) 2016 Digital Ruby, LLC
https://www.digitalruby.com
Created by Jeff Johnson

Current Version : 7.3.0

*** IMPORTANT ***
*** Please delete any existing Weather Maker folder and re-add everything from scratch if you are upgrading from earlier than 5.0.0 version. ***

For URP Weather Maker requires Unity 2021.2 or newer and URP 11 or newer.
HDRP is currently not supported.

*** IMPORTANT ***

For Change Log, See ChangeLog.txt.

Welcome to Weather Maker! I'm Jeff Johnson, and I've spent thousands of hours on this asset. I hope it serves you well for all your weather, volumetric clouds, sky, fog, water, volumetric light, audio and special weather effect needs.

Please send me your suggestions, feedback and bug reports to support@digitalruby.com.

Weather Maker is many assets in one. Rain/snow/sleet/hail, sky, night stars, sun/moon, volumetric clouds, volumetric light, audio manager / ambient audio, weather zones, fog, day/night cycle, thunder and lightning and much more. With dynamic profiles for everything, you can customize the look and feel of every scene.

Requirements
--------------------------------------------------
- URP requires URP 11 or newer on Unity 2021.2 or newer.
- URP VR is not supported due to complexity to support (I'm just one dev...)
- To enable URP: Click Window -> Weather Maker -> Enable URP
- Weather Maker requires Unity 2019.4 or newer.
- Weather Maker requires texture array support.
- Weather Maker does NOT support WebGL, OpenGL, OpenGL ES or DirectX 9.0.

Setup Instructions
--------------------------------------------------
Launcher Setup
- Most games use a launcher scene to store permanent objects and to kick off a main menu. In this case, your player
  and the weather maker prefab will go in the launcher scene.
- I've provided a launcher script, WeatherMakerLauncherScript.cs which you can use.
- Drag the WeatherMakerLaunchManager prefab into your launcher scene. It will live forever in all your scenes.
- Modify build settings to include your launcher scene, main menu scene and game scene. Make sure launcher is scene index 0.
- Make sure to set the game button and main menu button names on the launcher so that the launcher can wire up to the click events for those buttons.
- Make sure to set the main menu scene and game scene names on the launcher prefab to your scene names so they can load when the buttons are clicked.

Project / Scene Setup:
- Enable .NET standard or .NET 4.5 or newer api.
- You may need to delete the WeatherMaker folder and re-download the asset if you see errors, as scripts and shaders are renamed or moved from time to time.
- Delete Rain Maker from your project if it is loaded. It is not compatible in the same project with Weather Maker, and is not needed anyway.
- If you want clouds or sky sphere, change camera settings to clear as solid color, black (0,0,0,0). Camera set as skybox clear might render over top of clouds and sky sphere. Alternatively, set full screen cloud script to render after skybox and disable sky sphere to use your own custom skybox.
- Change light settings: Skybox material -> WeatherMakerSkyboxMaterial, sun source: Sun (from weather maker prefab).
- Change light settings: Environment lighting source: Color or Gradient, Environment reflections source: custom.
- Change player settings -> other settings -> rendering -> color space to linear if your platform supports it. Linear looks MUCH better than gamma.
- Add MainCamera tag to your main camera. Unity does this by default, but you should double check this.
- Make sure HDR is on for your camera(s). Weather Maker looks MUCH better with HDR.
- For flight simulator or high altitudes, a camera far plane of 100000+ is recommended, otherwise for mostly on ground game, a far plane of 10000 is good.
- For 3D, if you will be using storm lightning, set your main camera far plane to 10000+ or configure the lightning distances to be within your far plane (WeatherMakerPrefab -> Precipitation -> ThunderAndLightning -> Positioning)
- For 3D, if you are using tree billboarding, you can enable the full screen fog aware tree billboard shader by enabling WeatherMakerTreeBillboardShader.shader (see top of that file for instructions).
- Add null zones anywhere you want to block weather from rendering (caves, indoors, etc.). See null zone section down below.
- Add weather zones throughout your scene if you want different types of weather.
- Ensure screen space shadows are enabled in your graphics settings for best appearance.
- Without screen space shadows, Weather Maker will not cast shadows on full screen snow overlay and clouds will not cast shadows.
- For VR/AR it is recommended to use forward rendering due to Unity bugs with deferred rendering in VR/AR.
- Add WeatherMakerCelestialObject to any directional lights that you want to be a sun or moons. Set the IsSun property to tell Weather Maker whether it is a sun or moon.
- Post processing stack is highly recommended on your camera. Window -> Package Manager -> Post Processing.
	- Add post processing layer and volume to your main camera.
	- Set volume as global.
	- Choose SMAA or Temporal AA on the anti-aliasing side, or FXAA if you are targeting lower end platform.
	- Make sure to turn off deferred fog if you are using Weather Maker fog.
	- I like to setup temporal AA or SMAA, bloom, color grading, depth of field, ambient occlusion and vignette.

Prefab Setup:
- Drag the Prefab/WeatherMakerPrefab (or WeatherMakerPrefab2D for 2D) into your first scene. This object can be deactivated if you don't need Weather Maker for certain scenes.
- Set AllowSceneCamera to false if you don't want Weather Maker to render in scene view or you see UI corruption. Unity scene view gets quite buggy with command buffers sadly.
- Set IsPermanent to false on WeatherMakerScript (root of prefab) if you want to add a new Weather Maker prefab to each scene, otherwise please only add the prefab to your first scene.
- Add any additional allow cameras or allow camera names that you want weather maker to render in. See WeatherMakerScript at root of prefab object. Default is to allow the main camera.
- If you want global random weather, you can activate the GlobalWeatherZone object underneath the WeatherMakerPrefab object. See the weather zone tutorial video below.
- You must leave the following prefab objects and their scripts activated:
	- WeatherMakerPrefab
	- AudioManager
	- DayNightCycle (you can set day and night speed to 0 on the day/night profile to disable dynamic time of day)
	- LightManager (add any point/spot lights here that you want to apply to weather effects)
	- SkySphere script (can disable renderer)
	- FullScreenEffects -> Clouds (this renders sun, moon, sky, atmosphere and clouds)
- You can deactivate the other objects if you don't want them, but do not delete anything.
- Set the manager interfaces on WeatherMakerScript to your own scripts if you want to provide your own manager / rendering of precipitation, clouds, fog, wind, etc., or just tweak the particle systems right in the prefab.
- By default the prefab contains a sun (and moon for 3D). If you want additional celestial objects, add WeatherMakerCelestialObjectScript to your directional lights.
- Customize WeatherMakerPrefab -> DayNightCycle -> Sun -> Celestial Object Script -> Base Shadow Strength to 1 if you want darkest shadows. Change other properties as desired.
- If you want a simple test GUI, drag WeatherMakerConfigurationCanvasPrefab into the root of your scene. Be careful if you are using another canvas as they might conflict with each other.
- The prefab MUST have the following objects and scripts enabled for proper operation:
  - WeatherMakerPrefab root (WeatherMakerScript and WeatherMakerCommandBufferManagerScript)
  - WeatherMakerPrefab -> DayNightCycle (WeatherMakerDayNightCycleManagerScript)
  - WeatherMakerPrefab -> LightManager (WeatherMakerLightManagerScript)
  - WeatherMakerPrefab -> SkySphere (WeatherMakerSkySphereScript) - you can deactivate the sky sphere mesh renderer if you have your own skybox, but please leave everything else active.
- The 2D prefab includes a 2D and 3D camera in the prefab. If you need to customize 2D movement with your own special camera, deactivate the 2D camera, duplicate it and make your own outside the prefab.
- For 2D/orthographic, you can show sun, moon and volumetric clouds. See instructions in DemoScene2D.
- If you want clouds to render AFTER skybox, you can keep your main camera clear flag as skybox and then set WeatherMakerPrefab -> FullScreenEffects -> Clouds -> Render Queue to after skybox.
- If you are using your own skybox material and have deactivated the Weather Maker sky sphere renderer, please ensure any textures set in your sky profile are using the sphere texture mode (see the default sky profile night texture for an example).
- Setup the sky and atmosphere profile to look how you want, you can create new sky and atmosphere profiles. Enxure sky sphere ground color is to your liking. Assign your sky profile to the sky sphere, and assign your atmosphere profile to your sky profile.
- When you build for each platform, ensure you have set the ResourceContainer field on WeatherMakerScript properly. Only the objects in this profile will be included in the final build, so please ensure you have dragged all profiles, textures, etc. that you want to use from Weather Maker into this folder. 'Default' and 'Mobile' profiles are included for you by default to use, but you can duplicate, customize as needed and remove items to save space in your build.

Player Setup:
- Ensure the player object has an audio listener component that is enabled. This is how Weather Maker identifies a local player.
- Add a kinematic rigid body to the player object on the same game object as the audio listener.
- Add a tiny sphere collider (radius of 0.001) to the player object and make sure the 'trigger' checkbox is selected. Do this to the same object as the audio listener.
- Adding the rigid body and sphere collider ensures that OnTriggerEnter is properly called in Weather Maker scripts, specifically weather zones.
- Add a WeatherMakerSoundZoneScript to your player if you want to use sound zones (ambient sound). Add this to the same object as the audio listener.
- The Player prefab object is setup like this if you need an example (WeatherMaker/Prefab/WeatherMakerPlayer.prefab).
- Network player objects have a few additional requirements, please read on about mirror network integration.

URP (Universal Render Pipeline) Setup:
- Must use Unity 2021.2 or newer with URP 11 or newer.
- Create a new blank Unity project using URP 3D template.
- Import Weather Maker.
- Click Window -> Weather Maker -> Enable URP.
- Click on WeatherMakerURPForwardRenderer in project view and click the fix button in the inspector (if it exists).
- You may have to remove the second render feature and re-add the WeatherMakerURPRenderFeatureScript a second time. Make sure it appears after screen space shadow feature.

Network Setup with Mirror (Unity Networking Replacement):
- These instructions should be similar to other networking systems, but you will have to re-write WeatherMakerMirrorNetworkScript (pretty easy) for your networking platform and assign it to WeatherMakerScript.Instance.NetworkConnection.
- In project settings -> player settings -> other settings -> scripting runtime version is .NET 4.X and/or .NET standard 2.0 or newer. Do this BEFORE adding mirror to the project or things will get very crashy and buggy.
- Download Mirror from asset store: https://www.assetstore.unity3d.com/en/#!/content/129321?aid=1011lGnL
- Import Mirror into your project and restart Unity.
- On your player prefab:
	- Add to your player prefab a script like WeatherMaker/Prefab/Scripts/Other/WeatherMakerNetworkPlayerScript.cs. Your script should do the following:
		- For network players, disable (do NOT delete) any audio listeners on the network player object.
		- For network players, disable (do NOT delete) any sound zone scripts on the network player object.
		- For network players, disable (do NOT delete) any cameras on the network player object.
		- You only want to activate the above things for the local player.
	- Make sure your player controller script is checking for local player, like WeatherMakerPlayerControllerScript.cs, or using commands to a central server. You don't want a player controller executing on all the non-local players.
	- Ensure your player layer can collide with your weather zone layer. The default global weather zone layer is on the 'Default' layer but you can change it. WeatherMakerPrefab -> GlobalWeatherZone.
	- For optimal performance you might consider a player layer and a weather zone layer that can only collide with the player layer, but this is up to you.
	- See NetworkPlayer prefab for a player prefab example that includes a camera and audio listener in WeatherMaker -> Prefab -> NetworkPlayer.zip.
	- (Optional) Add a WeatherMakerIsPlayerScript to your player prefab if using networking. Ensure this is added to the same object with a trigger collider. If there is a NetworkIdentity component on the same object, the IsLocalPlayer bool will be set automatically.
- Add mirror network manager to your scene (uMMORPG does all the mirror setup automatically). Setup your offline/online scene and player prefab. Ensure max connections is set to the value you want.
- Add WeatherMakerMirrorNetworkScript script to an empty game object in your scene. Do not add any other objects to this object. This object must be at the root of your scene.
- Mirror works basically the same way as UNet did, only less buggy and much faster and with the full source code, these guys are awesome!
- Time of day and the global weather zone are synced from server to clients currently. Weather profiles will not be pixel for pixel the same on each client but will generally look the same for the type of weather.
- Lightning and other random effects are random on each client, but generally the same for the given weather profile.
- In order to sync properly, all instances running should be actual builds or all editors - you can't mix and match due to the way Unity serializes scriptable objects in editor mode and build mode.
- Please watch the uMMORPG tutorial in the tutorial video section of this readme file.
- For a server build (headless), make sure UNITY_SERVER is defined in scripting defines, this will cause the command buffers for Weather Maker to not execute.

For troubleshooting/performance problems, see the troubleshooting section near the bottom of this document.

Workflow
--------------------------------------------------
Weather Maker is built on scriptable object profiles. There is a boolean property called CloneProfiles that will clone profiles on play to avoid accidental changes. You can turn this off to make changes at runtime in the editor.

You can now edit clouds in editor mode as of Weather Maker 5.4.0. Simply drag in a cloud profile and start editing and configuring stuff, then set it back to the 'none' profile when you are done. If you see clouds that don't look quite right try opening the scene view or resizing the game Window. Unity is a little funny in editor mode about sending update events.

To change a profile during play mode, press play and then drag the profile in from the project view to the inspector and configure the profile as you like.

Some materials are cloned upon play to avoid accidental changes. I've tried to move most materials to material property blocks, but there may still be some cloned Material objects upon pressing play.

You can show or hide Weather Maker in the scene view by toggling the AllowSceneCamera property on WeatherMakerScript. Turn this off if you see scene view UI corruption, this is a bug in Unity with command buffers.

RenderToCubemap is fully supported, but only in the editor, not in the final build. 360 video, etc. should work fine, just restricted to the editor.

Tutorial Videos
--------------------------------------------------
Newest Weather Maker Videos...

- Universal Render Pipeline: https://youtu.be/6RLPa1CWQS8
- Open Weather Map API: https://youtu.be/7_r7oibCM68
- uMMORPG/Mirror Tutorial: https://youtu.be/BIFQqmG2Gws
- Volumetric Clouds:
	- Beautiful Sky Setup: https://youtu.be/1Xk2z858T1U
	- Complete Overview (40+ min): https://youtu.be/ZoULn0mYbt8
	- Ray March Optimizations: https://youtu.be/Dwk4vF_6ogs
	- Cloud Noise Editor: https://youtu.be/NxuJ5H2mZwY
	- Aurora Borealis / Northern Lights: https://youtu.be/J9QSebVLEO8
- Temporal Reprojection: https://youtu.be/JCzE7JfFPU8
- Performance Profiles: https://youtu.be/-Yoj_OjJL4g
- Complete Setup Guide: https://youtu.be/DHNnS6f85rM
- Quick Start Guide: https://youtu.be/5IACGZULiDw
- New Prefab Overview: https://youtu.be/hWJF5v0s4gc
- Weather Zones: https://youtu.be/11QoMD_qhHw
- Null Zones: https://youtu.be/ZPBum2YcgB8

Older Videos...

- Full Screen Snow: https://www.youtube.com/watch?v=b5j2wVHVxq0
- Water Full Overview: https://www.youtube.com/watch?v=oAQ4UFxa-X0
- Fog
    - Full Overview: https://www.youtube.com/watch?v=k-dC2EPd4no
    - Fog Shadow Map Lighting: https://www.youtube.com/watch?v=PTIC1oQzxno
    - Volumetric Lighting and Fog: https://www.youtube.com/watch?v=D9MUloqUQjU
    - Older Fog Videos
        - Full Screen Fog: https://www.youtube.com/watch?v=1_w9C8hWTXw
        - Fog Volumes: https://www.youtube.com/watch?v=jJ_tx0Vog0o
- Sound
    - Scene / Ambient Sounds: https://www.youtube.com/watch?v=LdnALY4eCU4
- Sky
    - Setup: https://www.youtube.com/watch?v=QE3VZHWkVec
    - Clouds: https://www.youtube.com/watch?v=1YM1Z7ap0FU
    - Procedural Sky: https://www.youtube.com/watch?v=sB7U-yz-i6k
    - Suns and Moons: https://www.youtube.com/watch?v=neVZMeljYIQ
    - Day/Night Cycle: https://www.youtube.com/watch?v=M6PTyr52a00
- Precipitation
    - Rain Inside: https://www.youtube.com/watch?v=zFT2KVoR3ro (deprecated, use null zone instead)
    - Ripple effect: https://www.youtube.com/watch?v=7V1ykljE9N8

Much Older Videos...

- 3D Demo: https://www.youtube.com/watch?v=25XEdmHFXQY
- 2D Demo: https://www.youtube.com/watch?v=oX0Sa2IC2D4
- Multiple Cameras: https://www.youtube.com/watch?v=6y5U37p4RpE

Full Code Documentation
--------------------------------------------------
Full code documentation for Weather Maker and my other assets may be found at https://unitydocs.digitalruby.com/

Performance
--------------------------------------------------
Weather Maker has performance in mind always. New in Weather Maker 5.0.0 are performance profiles. This allows a scriptable object to contain all the performance related optimizations for each Unity quality setting. Clouds, fog, precipitation and more all react to your performance settings.

I've made a profile to match each default Unity quality setting, but you can turn off the "auto performance profile" option on WeatherMakerScript.cs (root of prefab) if you have your own custom profile you want to use.

Please tune the properties on the performance profile if you are not getting acceptable frame rates.

Slimming down the build size
--------------------------------------------------
On WeatherMakerScript on the root of the WeatherMakerPrefab is a resource container profile. You can remove profiles, textures, sound files, etc. to reduce your final build size. Remove anything you know you aren't using and it will not be included in the final build.

You must apply changes to the prefab object itself in your scene or in the project view for the changes to take affect, otherwise Unity will use the resource container profile defined in the base prefab object.

For a 2D game, if you don't need to mix with the 3D Weather Maker, you can look at DemoScene2DNo3D. This scene contains a WeatherMakerPrefab2DNo3D that is heavily optimized and has stripped out weather zones, volumetric clouds and fog and othe big resource scripts and files. This will save you a ton on your final build size for 2D games.

Floating point origin offset
--------------------------------------------------
When dealing with large distances, 32 bit floats will lose precision, requiring adjustment of the camera and game object transforms back to the origin (0, 0, 0), but keeping the shaders and other certain effects aware of this offset to ensure a smooth transition.

On the root of WeatherMakerPrefab is the WeatherMakerCommandBufferManager script. On this script are some properties to help manage floating point origin offset:
- OriginOffsetDistance: the distance a camera must be from the origin (0,0,0) to trigger an origin offset event.
- OriginOffsetAutoAdjustAll: true to have Weather Maker adjust all the root transform objects in your game when there is an origin offset event.
- OriginOffsetChanged: if OriginOffsetAutoAdjustAll is false, you can hook into this event which receives a camera properties object to adjust the transform positions yourself when there is an origin offset event. You can also hook into this event to adjust your own shader code to use the origin offset.

- Call WeatherMakerCommandBufferManagerScript.Instance.GetCameraProperties to get the properties for a camera and tell you what the current origin offset is.
- Call WeatherMakerCommandBufferManagerScript.Instance.ResetOriginOffset to reset the origin offset to Vector3.zero for all cameras.
- Weather Maker shaders will use the origin offset to ensure noise values and other shader values take the origin offset into account, ensuring a smooth transition.
- The origin offset is really only suitable to single camera setups. With multiple cameras, it becomes difficult to manage all the game object positions relative to each camera.
- The origin offset logic only runs during play mode. During edit mode, no callbacks or transform adjustments will be made.
- The origin offset event has two origin offsets, the current and the cumulative. Use the current to reposition game objects if OriginOffsetAutoAdjustAll is false. Use the cumulative to adjust shader camera position.
- Please note that attempting to adjust the position of game objects marked as static will have no effect.

Weather Zones
--------------------------------------------------
New in Weather Maker 4.0.0 are weather zones. These allow different types of weather mapped to a collider. Combined with the new post processing stack v2, you can create some amazing effects.

Weather zones require a collider. I would suggest to make a child object of your collider object. Select the game object, right click and add an empty game object as a child. Then add a collider. Finally click Unity menu -> component -> weather maker -> weather zone.

Now you can setup a group profile (for random weather, i.e. temperate or snow) or you can simply assign a single weather profile for a static weather type.

If you are using post processing, create a new weather layer and assign it to the weather zone, and make sure your post processing layer has this layer set in the mask.

Weather zones require that an audio listener be attached to the camera game object, along with a tiny trigger collider (use a sphere trigger collider with radius 0.001). For networked, non-local players, simply disable the camera and the audio listener components (do not delete them).

WeatherMakeScript has a HasHadWeatherTransition bool that is false to start. If false, the first weather transition is instant. Subsequent transitions are normal. You can reset this bool to false to enable instant transition for the next weather transition.

Weather Api
--------------------------------------------------
Weather Maker makes it easy to sync up the weather profile from an external API. DemoSceneWeatherApi shows this in action using open weather map.

Weather Maker maps open weather map weather conditions ids to weather maker profiles. This is not 100% exact for all weather conditions but it does pretty well.

*IMPORTANT* You must set an API key for open weather map to work. You can easily get a free API key from open weather map here: https://openweathermap.org/appid

In order for this to work, you need to set either a lat/lon or a place name to query. You can lat/lon of -999 to use the day night cycle lat lon, or 999 to use the current location from location services.

*IMPORTANT* If the place name is specified on the WeatherMakerLocationScript, it overrides lat/lon.

If you have a different weather api service you want to use, you can implement the IWeatherMakerLocationWeatherApi interface, and set your implementation on WeatherMakerLocationWeatherScript.WeatherApi property. See the WeatherMakerOpenWeatherMapApi class for how this is done.

When implementing your own weather api, it is easiest to map weather conditions to weather maker profiles, but you can get very advanced and configure everything such as wind, fog, clouds, rain, lightning, etc. separately if you want. You also need to potentially get an api key for other weather api services as well.

Please see WeatherMakerLocationWeatherScript.cs for example code.

Location Services
--------------------------------------------------
The above weather api function uses location services, but that may not be desirable as it can cause popups on certain devices asking for permission.

To turn this off, you can comment out this file by putting a #if !ENABLE_THIS at the top and a #endif at the bottom: WeatherMakerLocationWeatherScript.cs

Null Zones
--------------------------------------------------
Sometimes you don't want precipitation, fog, snow overlay and water to render in certain areas.

Let's say you have an open building, cave, boat or other structure in your scene. You don't want the rain, ripples, fog, snow overlay or water rendering in this area - great time to add a null zone prefab.

Only sphere and box collider is supported at this time. Null zones can be scaled and rotated. You can use the null zone mask to determine what types of things to not render.

Let's say you want the snow overlay, but you don't want it to appear on the players hands, armor or weapon, horse, etc. Put a null zone around those objects and change the mask to only block the overlay, leaving precipitation, fog and water to render around it.

Use the NullZoneFade property to control how rendering happens near the edges of null zones. Move this to the max value (100) to disable the fade completely. Lower values fade more.

If null zone fade is <= 0 then special overlay handling will be done. As the camera nears the zone, overlay in the zone will fade out. This is most useful for open buildings. The speed of fade out is determined by abs(fade). Precipitation and fog will use abs(fade) with normal fade handling.

In order to customize the null zone, you will need to create a null zone profile. Right click in project, Weather Maker -> Null Zone Profile. Then assign the profile to your null zone script.

Dampening Zones
--------------------------------------------------
The WeatherMakerDampeningZone script can be used to reduce precipitation intensity, sound volume and light intensity when the player has moved into the collider area.

You can use the built in prefabs (WeatherMakerDampeningZone or WeatherMakerNullZoneClosed), or build your own. Ensure that a collider is added and markets as a trigger, and that the main camera has an audio listener and a tiny collider marked as trigger (a sphere collider with 0.001 radius).

Whenever the trigger is entered by an object with an active audio listener, the new dampening values are smoothly transitioned to from the current values. When the trigger is exited, the dampening values smoothly go back to 1.

Precipitation
--------------------------------------------------
Weather Maker comes with 4 precipitation types: rain, snow, hail and sleet. Each are very similar in their setup, but contain different material, textures and parameters.

All the precipitation scripts derive from WeatherMakerFallingParticleScript. Each has the concept of four stages: none, light, medium and heavy.

Precipitation objects are underneath the WeatherMakerPrefab object.

You can use a custom precipitation prefab / script. Set your precipitation profile to use custom precipitation and ensure you have assigned the custom precipitation scripts:
WeatherMakerScript.Instance.PrecipitationManager.CustomPrecipitationScript = yourCustomPrecipitationScript;

Properties:
- Loop Source Light, Medium and Heavy: The audio source to play as the prefab changes to different stages. Each sound fades in and out nicely.
- Sound Intensity Threshold: Change at what point the intensity will go to a new stage. Intensity of 0 is always the none stage.
- Intensity: Change the overall intensity of the precipitation.
- Intensity Multiplier: Change the intensity even more - watch out for performance problems if you go above 1.
- Secondary Intensity Multiplier: Change the secondary intensity even more - watch out for performance problems if you go above 1.
- Mist Intensity Multiplier: Make the mist less or more intense - again watch out for performance problems if you go above 1.
- Base Emission Rate: Base number of particles to emit per second
- Base Emission Rate Secondary: Base number of secondary particles to emit per second
- Base Emission Rate Mist: Base number of mist particles to emit per second
- Particle System: The main particle system.
- Particle System Secondary: The secondary particle system, if available
- Mist Particle System: The mist particle system.
- Explosion Particle System: Only used in 2D. For 3D, the explosion particle system is a child object of the particle system.
- Secondary Threshold: Once the intensity gets higher than this, secondary particles (if available) start to appear.
- Mist Threshold: Once the intensity gets higher than this, mist starts to appear.

3D Properties:
- IsFirstPerson: True if the precipitation will anchor around each rendered camera, false if it will be static (i.e. precipitation zone).
- Height: The down-pour particles will start at this height from the camera.
- Forward Offset: Offset the particle system this many world units in front of the camera.
- SecondaryHeight: Secondary particles will start at this height from the camera.
- SecondaryForwardOffset: Offset secondary particles this many world units in front of the camera.
- Mist Height: How high the mist will start falling from.
- For collisions, the weather maker prefab itself has a collisions property.
- Ripples:
	- Rain and sleet allow an animated ripple texture. Set AnimatedTextureRendererIntensityThreshold to be the full intensity where this should start showing up.
	- Set AnimatedTextureRendererPositionOffset to determine where the plane for the ripples offsets from the hit point closest underneath the camera.
	- Ripple effect will turn off if a collider is hit above the camera, i.e. the camera is indoors.

2D Properties:
- Height Multiplier: Particles will start at y value of the screen height + (world height * value). A value of 0.15 would make the particles start 15% higher than the screen height.
- Width Multiplier: The particle field will be this much wider than the screen width.
- Collision Mask: What should the particles collide with? The weather maker prefab can control this value globally so don't change this.
- Collision Life Time: About how long particle should live when they collide.
- Explosion Emission Life Time Maximum: When particles collide and have a life time higher than this value, they may emit an explosion.
- Mist Collision Velocity Multiplier: The mist speed is multiplied by this value whenever it collides with something.
- Mist Collision Life Time Multiplier: The mist life time is multiplied by this value whenever it collides with something.

Some snow precipitation particles uses local simulation space. Snow is tricky because if you use world space, the player can move fast enough to out run the snow if it is falling slowly. But you can set the particle systems to world space and try your luck. Set all to local or world depending on speed of player and situation.

Weather Manager
--------------------------------------------------
The weather manager was replaced in Weather Maker 4.0.0 with weather zones. On the WeatherMakerPrefab is a GlobalWeatherZone object. Simply replace the profile on this object with any weather profile group to change the type of weather for the entire game. Additional weather zones with different weather group profiles may be added throughout your game that override this global weather.

Precipitation Zones
--------------------------------------------------
See DemoScenePrecipitationZones. These allow static precipitation that does not move. You will most likely need to tune the particle emission properties, emission shape and size, and probably the sound settings to work for the scale and look of your scene and game.

The properties on the precipitation zone prefabs are identical to the first person precipitation objects in the Weather Maker prefab, with some tweaks and IsFirstPerson is set to false.

Full Screen Overlay Effects (Snow and Wetness)
--------------------------------------------------
WeatherMakerFullScreenOverlayScript.cs implements a full screen overlay effect. This will cause an overlay on the entire world.

This effect is integrated into a snow and wetness effect and is highly customizable, please review the tutorial video (link at top of this file) and all properties of the script for full details.

This effect can link up to the snow and wetness intensity. Set AutoIntensityMultiplier on WeatherMakerPrefab -> Full Screen Effects -> Snow/Wetness -> Auto Intensity Multiplier to desired value.

Full screen overlay looks best with deferred shading but will work on forward rendering as well.

If your grass looks funny with the overlays, you can enable the grass override shaders. Choose one of the WeatherMakerWavingGrass* shaders and swap the top line to the Unity namespace. Then in your terrain select the details section and click refresh.

Virtual Reality
--------------------------------------------------
You should use the VR performance profile in VR, otherwise you will get a lot of rendering artifacts, especially with clouds.

If your performance profile is left null, it will auto select VR if needed at runtime.

VR works best in Unity 2020 or newer. Multi-pass, single pass and single pass instanced are supported.

VR is only supported in standard pipeline. VR in URP is unusable due to Unity bugs.

Audio / Sound Volume Modifier
--------------------------------------------------
WeatherMakerAudioManagerScript.cs contains a VolumeModifierDictionary that determines the final volumes for sound.

Wind
--------------------------------------------------
New in Weather Maker 4.0.0 - use a wind profile. This is now a property on the wind script, which has all the same properties you are used to.

The wind script allows you to have wind randomly blow in your scene, which will affect the rain, snow, mist, etc. The WindScript is where the wind profile is set.

To enable or disable wind, set the WindIntensity property on WeatherMakerWindScript.

Wind affects fog velocity. This can be disabled by setting FogVelocityMultiplier to 0 on the wind script.

Wind direction is set either randomly, or using the transform of the Wind object. To disable random wind direction, set the WindMaximumChangeRotation property on the WeatherMakerWindScript to zeroes. You can then just set the rotation of the Wind object in the prefab to your wind direction.

Please see the properties in WeatherMakerWindScript.cs (wind profile) for a full list of what is available. Each property has a tooltip description that explains the property.

Sun
--------------------------------------------------
The weather maker light manager contains a Sun object. This object should stay enabled. It's also very important that the sun intensity not be changed by anything external to the Weather Maker scripts, otherwise artifacts may occur.

If you need to change the sun intensity, the light manager script (WeatherMakerLightManagerScript.Instance) has a DirectionalLightIntensityMultipliers property which is a dictionary of a key and mutliplier that will affect sun and moon light intensity.

Although the light manager has a Suns array, only one sun is currently supported. Multiple suns may be supported in the future.

The sun must not be deactivated.

Moon
--------------------------------------------------
Earths moon is on by default in 3D mode. I don't have it always facing the right way, but it still looks pretty good. Accurate moon phases for the lat, lon, date and time are included.

Moon phase increases moon lighting intensity as well.

The moon is a directional light. Moon base intensity is set on the day night script. Moon lighting power and tint color, etc. is set on the sky sphere script.

The moon material has a fade property that can control how the black part of the moon looks at night. A fade value of 1 completely fades our the black areas, 0 completely shows them.

You can deactivate the moon object if you don't want moons.

Clouds
--------------------------------------------------
In 3D, clouds are created using the cloud script. In 2D, 2D clouds are created using the LegacyCloudScript2D. See CloudToggleChanged in WeatherMakerConfigurationScript.cs for how to set clouds.

3D clouds used to be run on the sky sphere. This has changed and they are now a full screen command buffer using cloud profiles, which allows better performance and much nicer looking clouds using downsampling and temporal reprojection. Wherever you see sky sphere cloud properties being referenced, simply refer to the same properties in the cloud profile.

The cloud script now uses a cloud profile as of Weather Maker 3.0.0, all the cloud properties are in the cloud profile scriptable object and cloud layer profiles.

Cloud shadows are only available with perspective/3D cameras.

*NEW* Volumetric and flat clouds are now available in 2D, see DemoScene2D for details.

Volumetric Clouds
--------------------------------------------------
If you have WeatherMakerScript AutoCloneProfiles turned on, the cloud profile will be cloned when changed. Uncheck this to set your own profile during play mode and then customize without having it cloned, but be careful not to destroy your cloud profile which you have worked so hard to customize just right. I always suggest duplicating your profile and editing a copy of it.

You can generate identical clouds using the CloudSeed property on the full screen cloud script.

Volumetric Clouds use a weather map render texture. This is on the cloud profile. You can override the weather map with your own texture, otherwise a weather map is dynamically generated based on cloud profile settings.

The weather map uses r channel for cloud coverage, g for density and b channel for cloud type (0-1 where 0 is stratus and 1 is cumulus). Alpha channel is reserved. Your artist or designer can use a graphics editing program to make custom weather maps and combine the color channels as a jpg or png.

The volumetric clouds use a heavily optimized shader that performs ray-marching through a 3D texture. Directional, point and spot lights will apply to cloud lighting (such as lightning).

Even though the shader is optimized, volumetric clouds are still expensive. For performance you can downsample the clouds, along with temporal reprojection and LOD settings for noise sampling.

By default, temporal reprojection will use sharp for clouds, and fall back to blue if lightning is showing. You can disable this behavior by setting AutoSetTemporalReprojectionBlendMode to false on the full screen cloud script.

The volumetric clouds even support fly throughs (see the fly through profile, only the fly through profile supports fly through) and are a fully mapped spherical volume of clouds. You can configure your planet radius and cloud min / max height parameters to set the look and feel.

For fly-through, ensure your camera far plane is large and that you have a planet covering surface volume or terrain that extends well beyond the camera far plane. You will also want to set your ray offset to 0 and reduce your horizon fade if you are able to move above the clouds.

The volumetric cloud profile includes a flat layer mask, which allows flat layers to exist with the volumetric clouds. By default, layer 4 is allowed which is set to a cirrus cloud layer in the built in profiles. Volumetric clouds are always rendered after all the flat layers.

Your day/night cycle profile heavily influences volumetric cloud colors. Ensure that your ambient, ambient ground and ambient sky colors and intensities are to your liking as these will be applied to the clouds.

There are many properties that can be configured on the volumetric clouds. The tutorial video is the best way to see these properties in action (see link at top of this readme file).

The shape of the volumetric clouds depends heavily on the noise textures used. I've generated a few 3D textures out of the box for you to use. You can create your own textures by creating a 3D texture for the shape and detail textures. See more details here: http://bitsquid.blogspot.com/2016/07/volumetric-clouds.html.

Cloud shadows are supported with Unity screen space shadows, but be aware that shadows will fade at distance by default. This can be removed by doing the following:

Find UnityShadowLibrary.cginc in the Unity install directory (UnityInstallDirectory/Editor/Data/CGIncludes folder). Find this method and change it, then restart Unity.

half UnityComputeShadowFade(float fadeDist)
{  
    return 0.0;
    //return saturate(fadeDist * _LightShadowData.z + _LightShadowData.w);
}

Unity does not provide a render setting or other override, so you must do this everytime you install or update Unity. Please bug Unity to change this, it is really annoying.

In addition, Unity does a poor job of handling shadow distance when using screen space shadows. If you are building a flight simulator or other game where you need the cloud shadows to show at large distances, you will need to adjust your shadow distance to be higher in the quality settings.
 
You can easily create your own 3D noise textures for different types and styles of clouds. Window -> WeatherMaker -> Cloud Noise Editor.

The temporal reprojection material on the full screen clouds script has a blur mode property that can be set to 1 for very crisp, sharp clouds, but watch out for artifacts for fast moving clouds, or lightning at night in the clouds.

Known issues with volumetric clouds (I am working on them):
- Jittering clouds - this can happen with temporal reprojection and VR. Use temporal anti aliasing to help.
- Fly-through clouds are possible, but the horizon has some problems as you transition into the clouds. I am working on a fix, but you can also add mountains or terrain to your horizon to help.
- If there are more issues or bugs, I will try to fix them as I find them or you report them.

You can comment out line 26 of WeatherMakerCloudVolumetricShaderInclude.cginc to turn off all the volumetric code if you are not using it as this will save shader size and compile times.

*** Please view the volumetric cloud tutorial at the top of this readme file to get a complete overview of all properties and usage. ***

The sun and moon object in the Weather Maker prefab contain a shadow script with a shadow material that controls additional shadow settings such as intensity, detail texturing, etc.

The global "general" shadow of Weather Maker fog and clouds can be accessed via WeatherMakerLightManagerScript.Instance.GlobalShadow.

The full screen cloud script includes options for weather map and cloud post processing. See DemoSceneCloudPostProcessing for an example. You can apply materials to the weather map, and to the clouds.

Volumetric Clouds Probes
--------------------------------------------------
You can probe into the volumetric cloud density of any transform. Simply add a WeatherMakerCloudProbeScript to any transform. If destination transform is null, a single point probe is done. If destination is not null and not equal to source, a ray cast probe is done.

When you need to get the probe, call WeatherMakerFullScreenCloudScript.Instance.GetCloudProbe(camera, sourceTransform, targetTransform) and you get back a result. Pass null for targetTransform if it is the same as sourceTransform.

Probing for cloud densities requires computer shaders. Without compute shaders, GetCloudProbe will always return 0.0.

Only regular game cameras can probe. Reflection, cubemap, etc. cameras do not have cloud probe capability.

Aurora Borealis / Northern Lights
--------------------------------------------------
The weather maker full screen cloud script contains an aurora profile. This allows for aurora borealis / northern lights. If you notice an impact on performance, you can lower aurora sample count in the performance profile.

Aurora sample count and sub-sample counts are controlled by the performance profile, however if the profile sample and sub-sample counts are less than the performance profile, they will be used instead of the performance profile sample counts.

Meteor Shower
--------------------------------------------------
A meteor shower prefab is included and can be placed in the root of the scene to create wonderful looking meteor showers. A meteor shower profile allows easy customization of common properties, or you can modify the particle system directly.

Lightning
--------------------------------------------------
- Located in WeatherMakerPrefab/ThunderAndLightningPrefab and WeatherMakerPrefab/ThunderAndLightningPrefab/LightningBoltPrefab.
- I've included the core of my Procedural Lightning asset (http://u3d.as/f1c) to power Weather Maker lightning.

ThunderAndLightningPrefab Setup
Ideally this script will work as is. Lightning will be created randomly in a hemisphere around the main camera. Lightning can also be forced visible, which means the script will try to make the bolt appear in the camera field of view.

Lightning light effects are less visible during the day and brighter at night. For best results, ensure that you create clouds before turning the lightning on.

This script has the concept of "Intense" vs. "Normal" lightning. Intense lightning will be created close to the player, be brighter and play a random intense sound very soon after the lightning strike, which will be loud. Normal lightning is further away, plays quieter sounds and those sounds take longer before playing.

WeatherMakerThunderAndLightning Properties:
- Lightning Bolt Script: The lightning bolt script used to generate the lightning. Leave as the default.
- Camera: The camera lightning should be created around. Default is the main camera.
- Lightning Interval Time Range: Random range in seconds between lightning.
- Lightning Intense Probability: Percent change that lightning will be intense (close and loud).
- Thunder Sounds Normal: List of sounds for normal lightning.
- Thunder Sounds Intense: List of sounds for intense lightning.
- Start Y Base: The base y value for lightning to start at.
- Start X Variance: Vary the x start position
- Start Y Variance: Vary the y start position
- Start Z Variance: Vary the z start position
- End X Variance: Vary the x end position
- End Y Variance: Vary the y end position. Lightning will ray-trace to the ground, so the end position may change.
- End Z Variance: Vary the z end position.
- Lightning forced visibility probability: The chance that the lightning bolt will attempt to be visible in the camera.
- Ground lightning chance: The chance that lightning will strike the ground.
- Cloud lightning chance: The chance that lightning will only be visible in the clouds.
- Sun: Used to lessen the lightning brightness during the day.
- Normal Distance: Force lightning away from the camera by this range of distance for normal lightning.
- Intense Distance: Force lightning away from the camera by this range of distance for intense lightning.
- *Note lightning will always be at least normal or intense distance minimum distance from the camera, regardless of Start / End variance settings.

Here are the properties you are most likely to want to change on LightningBoltPrefab (WeatherMakerLightningBoltPrefabScript):
- Duration Range: Range of how long, in seconds, that each lightning bolt will be displayed.
- Trunk width range: Range of possible width, in world units, of the trunk.
- Glow tint color: Change the outer color of the lightning.
- Generations: Change how many line segments the lightning has.
- Glow Intensity: How bright the glow is.
- Glow Width Multiplier: How wide the glow is.
- Lights: To turn off lights, set LightPercent to 0.

To get notified of lightning bolt start and end events, use WeatherMakerThunderAndLightningScript.Instance.LightningStartedCallback and WeatherMakerThunderAndLightningScript.Instance.LightningEndedCallback.
To get notified of thunder audio events, see WeatherMakerThunderAndLightningScript.Instance.ThunderSoundPlayed.
To force a lightning strike in your script, call WeatherMakerThunderAndLightningScript.Instance.CallNormalLightning(start, end) or WeatherMakerThunderAndLightningScript.Instance.CallIntenseLightning(start, end)

Atmospheric Scattering
--------------------------------------------------
The sky sphere profile has an atmospheric scattering profile. This controls atmospheric scattering settings and light shafts. The current performance profile controls whether atmospheric scattering is enabled, whether atmospheric light shafts are enabled and what the sample count and max ray length is.

The sky sphere also blends with the scene as of Weather Maker 5.8.0. You can control the sky fade and intensity on the sky profile.

Ensure that you have configured your camera far plane first before changing sky or atmosphere profile settings, as the far plane can influence how the sky and atmosphere look.

Sky Sphere
--------------------------------------------------
- Located in WeatherMakerPrefab/SkySphere.
- Not used in 2D mode, for 2D see the Sky Plane section.
- If you see any edges in the sky, increase the resolution property.
- Uses WeatherMakerSkyProfileScript.cs for the profile.

Sun / Moon:
- Celestial objects each have their own WeatherMakerCelestialObject script. In the default prefab, there is a day night cycle, underneath that are sun and moon objects.
- Sun and moon are rendererd with a mesh renderer.
- Lens flare can be blocked. By default the WeatherMakerPrefab has a sun lens flare blocker that is setup to block using compute shaders and cloud probes, with fallback to global cloud coverage if no compute shaders.

SkyMode:
- Procedural: Sky is fully procedural and day and dawn dusk sky sphere textures are ignored. Night texture is used as the sun goes below the horizon.
- Procedural Textured: Default, blends a procedural sky with the textures you specify. Your day and dawn/dusk textures should contain transparent areas with translucent clouds, make sure the import settings for the textures allows transparency. Night texture should still be opaque. The night texture will be hidden by any areas of the dawn/dusk texture that are opaque.
- Textured: Sky is fully textured and not procedural. If you need to convert a cubemap to a sphere/panorama map, try this Google search: https://www.google.com/search?q=unity+cubemap+to+panorama&oq=unity+cubemap+to+pano&aqs=chrome.1.69i57j0.11805j0j4&sourceid=chrome&ie=UTF-8#q=unity+cubemap+to+equirectangular.
- Note: Setting the player color space to linear may give a more Earth-like sky color for procedural modes.

Texture Modes:
- If you are using SkyMode "Textured" or "Procedural Textured", there are texture options:
- Set texture type to "Advanced". Then disable all settings or set to defaults. Then set wrap mode to clamp, filter mode to bilinear, aniso level 1, set max size to appropriate value and format to automatic truecolor for best appearance.
- Sphere: Texture should be about 2:1 aspect ratio. Top half is upper part (sky), bottom half is the ground.
- Panorama: Entire texture is the upper half of the sky sphere only. Great for higher resolution sky when the player won't ever look below the horizon.
- Panorama Mirror Down: Same as Panorama, except that the texture mirrors down to the bottom half of the sky sphere.
- Dome: Texture maps such that the center of the texture is the top of the sky sphere, and the edges of the circle are the horizon. This produces the best looking sky but requires pre-processing of the texture before importing into Unity.
- Dome Mirror Down: Same as dome except that the texture mirros down to the bottom half of the sky sphere.
- Dome Double: Same as dome except the texture contains two domes. The left half is the bottom dome, the right half is the top dome.
- Fish Eye Mirrored: Maps the fish eye to the front of the sky sphere and mirrors it to the back. Not suitable for 360 degree viewing.
- Fish Eye 360: Maps the fish eye to the entire sky sphere. There is a slight distortion at one end of the sky sphere so this is not suitable for full 360 degree viewing.
- *NOTE* If you will be using the procedural sky option, you can null out the day and dawn/dusk texture properties to save some disk space.
All sky sphere modes come with example textures (WeatherMaker/Prefab/Textures/SkySphere).

Night Sky:
- Night sky intensity can be changed via script parameter on the sky sphere script.
- NightVisibilityThreshold controls which pixels are visible. For a city or high light pollution scene, you could raise this value so only bright stars are visible.
- The night sky stars twinkle by default. There are several parameters that control how the sky twinkles. See the night sky twinkle section in the inspector of the sky sphere script. You can turn off the twinkle by setting the randomness and twinkle variance to 0.

Rotation:
- Sky sphere can be rotated with day/night cycle by setting rotation axis to non-zero. Requires procedural (non-textured) sky mode. There is no performance overhead, the night sky textures are NOT cubemaps.

Sky Plane
--------------------------------------------------
In 2D, a Sky Plane is used for procedural sky. Most parameters are the same as the sky sphere.

The Sky Plane renders in the default sorting queue at int.MinValue.

See DemoScene2D for an example.

Day Night Cycle Manager
--------------------------------------------------
WeatherMakerDayNightCycleManagerScript.cs contains full sun, moons and ambient color capabilities. This script uses a profile (WeatherMakerDayNightCycleProfileScript.cs) to manage settings.

Speed - The speed of the cycle during the day. Negative values are allowed. A value of 1 matches real time.
NightSpeed - The speed of the cycle at night. Negative values are allowed, but should match the sign of the Speed parameter. A value of 1 matches real time.
UpdateInterval - How often (in seconds) the cycle updates. 0 is the default which is every frame. Increase this if you are seeing flickering shadows, etc.
TimeOfDay - The time of day in local time, in seconds.
Year - the current year, this is currently only used for sun positioning.
Month - the current month, this is currently only used for sun positioning.
Day - the current day of month, this is currently only used for sun positioning.
TimeZoneOffsetSeconds - Used to convert TimeOfDay to UTC time. You should look this up once and set it based on your longitude, year, month and day. Set to -1111 for auto-detect. During editor mode, -1111 will cause a web service query to get you the right timezone for you location and date.
Latitude - Latitude on the planet in degrees (0 - 90)
Longitude - Longitude on the planet in decimal degrees (-180 to 180)
AxisTilt - The tilt of the planet in degrees (earth is about 23.439 degrees)
RotateYDegrees - Rotate around the up vector, useful for something besides an East/West cycle
DayDawnDuskNightGradient - Determines when it is day, dawn or dusk, or night. Green = day, red = dawn/dusk, blue = night. Center of gradient is sun at horizon.
SunIntensityGradient - Allow setting sun intensity multiplier, where center of gradient is sun at horizon.
SunShadowStrengthGradient - Allow setting sun shadow strength multiplier, where center of gradient is sun at horizon.

Ambient Colors:
- Day, dusk and night ambient colors and intensities are confiurable in the day/night script. There are multiple ambient possibilities.
- Select the appropriate ambient mode for your specific setup.
- If using "All" or "AllWithUnityMode" ambient mode, the ambient color will be added to the sky, ground and equator ambient color.
- If you want to use existing ambient settings and ignore the day night cycle colors, set the ambient mode to UnityAmbientSettings.

Ambient Sounds / Weather Sounds
--------------------------------------------------
You can create any number of sounds to play during dawn, day, dusk and night or even particular hours in the day and/or in zones. Weather Maker uses scriptable objects to allow you to setup a sound profile.

Right click in the project window and select create -> Weather Maker -> Sound Group, and name your profile. Then do the same and create Sound objects for each type of sound you want. In the sound you can set the audio clip, whether it loops, interval, duration and fade time. See WeatherMakerSoundScript.cs for more details.

When you are ready to add your sound groups to Weather Maker, simply drag in WeatherMakerSoundZone prefab into your scene. If the sounds will be active in the whole scene, put the prefab inside your player object (so the trigger will always be entered), otherwise find a nice place in your scene for the particular sounds in the group you created.

The sounds are not meant to be 3D and will have the chance to play as long as the trigger zone has been entered.

To debug sounds, uncomment the first line in WeatherMakerSoundScript.cs which will log start / stop sounds to the console.

As always, please watch the sound tutorial in the tutorial list at the top of this file.

You can add a sound zone script to your main camera, add a kinematic rigid body and tiny collider to have the sound zone follow the player. Ensure that the camera is a child of another game object with a collider that contains it. See Player prefab for setup.

Sound zones can be nested, but should never have more than two sound zones overlap. Sound zones should use 2D sound. If you want ambient sounds in 3D space, use AudioSource with 3D sound properties and sprinkler game objects around your scene.

Fog
--------------------------------------------------
Fog is only supported in 3D non-orthographic currently. Turn fog on and off by calling WeatherMakerFullScreenFogScript.Instance.TransitionFogDensity(fromIntensity, toIntensity, transitionSeconds). Ensure you have a fog profile set.

Fog Details:
- Highly performant and very configurable.
- Play around with the noise settings, density and fog types to get the look you want.
- You can set the fog height for ground fog.
- The noise scale, noise adder and noise multiplier will determine the variance and appearance of the fog. Please set these to appropriate values for the scale and feel of your game.
- *NOTE*: Fog noise height and warping has been removed in favor of true 3D fog noise. Set the fog adder property into the negatives to get a more varied fog appearance with holes.
- *NOTE*: For fog noise to work properly, it is important that the WeatherMakerLightManagerScript 'NoiseTexture3D' property is set. If you don't need fog noise, you can null out this property and save a few MB of disk space.
- FogNoiseSampleCount defines how many samples are taken for fog noise. You may need to adjust this for your platform, especially if you are targetting mobile devices.
- Fog contains shadow map capability. If fog shadow sample count is > 0, fog shadows will be used for sun only. Be careful about performance, and tune the settings. Watch the tutorial (see link at top) for best results.
- For full screen fog, a sun shaft sample count > 0 will add sun shafts. Tweak sample count and other parameters to your liking. 32 is a good value I have found.
- Tweak the dithering level if you see banding in low density fog.
- If you must use billboard trees (please don't though) but if you must, you can enable the full screen fog aware tree billboard shader by modifying WeatherMakerTreeBillboardShader.shader (see top of that file).
- Please watch the tutorial videos (at the top of this readme file). It covers most parameters.
- Email support@digitalruby.com with questions.

If you are seeing rendering artifacts, try changing the FogRenderQueue of the WeatherMakerFullScreenFogScript.cs in the inspector. It's at the bottom in the full screen settings.

The fog sphere and cube require a WeatherMakerPrefab be in your scene and setup properly.

Volumetric Lights:
Point, spot and area lights can be enabled in the fog by setting EnableFogLights to true on WeatherMakerScript. Lights must be added to the light manager (please see the light manager section). The easiest way is to add your lights to the light manager script 'AutoAddLights' list.

Please test that performance is adequate before shipping your game with this turned on. The demo scene is setup automatically to add the flash light, point light and a spot light. Volumetric lights work best in Linear color space.

Fog now supports temporal reprojection as of Weather Maker 5.0.0. Please watch the temporal reprojection tutorial video.

Water
--------------------------------------------------
A water prefab is included. Water now uses water profiles (see water profile property of WeatherMakerWaterScript).

Water has two rendering modes - one pass and forward base plus add. In one pass, all lights are rendered in a single draw call (huge performance boost for mobile and VR). In forward base plus add, the water uses one draw call per light.

Water fog can be configured to give water density and reduce light. If you are going underwater, you will want to make sure you set this up to look right for your water.

Water material inv fade parameter x value can be set to less than 100 to fade the shore, but this will make refraction have artifacts. Set to more than 100 if you prefer to have refraction look 100% correct and can deal with a harder shore line.

Water material inv fade parameter y value controls how normals fade out as camera is higher above water

Water material inv fade parameter z value controls reflection strength.

Water material inv fade parameter w value controls how normals fade out as water vertex is further away from the camera.

Water has a sparkle mode. This is setup on the clear water material. You can turn off sparkles by setting the _SparkleScale.w to 0 on the water material.

Water caustics are also configurable, see water material caustics section. Screen space shadows are required to render shadows over caustics.

Up to eight waves can be configured for the water using the WaterWave1 - WaterWave8 parameters: x and y are wave direction, z is wave amplitude and w is wave length. Set all to 0 for no wave.

Each wave has extra parameters, x = speed, y = height reducer (how much smaller height reduces the wave).

Wave heights and water fade are controlled by a water height depth map. Shorter depths to the floor of the water volume result in more fade and smaller waves (this is configurable). The water prefab uses WeatherMakerDepthCameraScript to snapshot this texture once. You can configure which layers are included in the depth map. You should only include mesh terrain, Unity terrain or other static volumes that represent the water volume, do not include boats and other dynamic object layers. Set the height parameter if you have mountains or other higher volumes above your water that do not have a floor inside (if the camera is inside a mountain for example, it will get max depth). Set Direty to true if you have changed the static water volume and it will recompute the depth map once, as this requires a re-render of the depth map (expensive).

The water profile has an option to allow wind to affect the waves. If this is enabled, there will be no waves unless there is wind.

Water foam can be configured via the water foam properties. You can set scale, intensity, depth fade (more foam in smaller depth) and wave factor (more foam on waves), along with foam velocity and specular reduction.

Underwater mode is supported. You can add a post processing layer to your camera object and a post processing volume to the water object to further customize the apperance. Color grading and depth of field really add extra pop.

Water has tesselated and non-tesselated modes. For tesselated water, you can tweak TesselationParams x to control number of vertices, y should be set to max displacement (waves) for proper culling.

Displacement options are also available for tesselated water to add finer detailed waves.

I would suggest to keep your water at least 2 world units below the flat ground around the water as the water null zone can fade up through the ground. Or you can change the water null zone profile fade to be higher values.

Volumetric light and shadows for water will be returning in a future version once performance is acceptable.

Shader Integrations (Water, Fog, Shadows, etc.)
--------------------------------------------------
WeatherMakerFogExternalShaderInclude.cginc contains all the shader integration for external shaders using Weather Maker, such as water, etc.

You can use Weather Maker fog in your own transparent materials, see WeatherMakerFogExternalShaderInclude.cginc, ComputeWeatherMakerFog function.

You can sample Weather Maker shadows using ComputeWeatherMakerShadows, also in WeatherMakerFogExternalShaderInclude.cginc.

Search for 'External shader integration functions' in your code editor (Visual Studio, etc.) to find all the possible function definitions.

Orthographic Size
--------------------------------------------------
For 2D Weather Maker, clouds and lightning might need a little tweaking, depending on your orthographic size. Please be sure to set the cloud anchor offset along with the lightning trunk width and light range to appropriate values for your orthographic size.

Light Manager
--------------------------------------------------
Weather Maker contains a Light Manager script. This script can keep track of the lights you want to use in the various Weather Maker shaders such as fog, clouds, etc. in WeatherMakerLightManagerScript.cs, SetLightsToShader sets up all the shader parameters for the lights in world space. These are set as global shader variables, so you can use them in your own shaders if you like.

The light manager automatically adds all sun, moon and lightning lights, you do not need to do this yourself.

The light manager can auto-find all lights in your scene, but this may be a performance problem, so if it is, you must manually add and remove lights that you want to be used for cloud and fog lighting, etc. or add them to the AutoAddLights list.

Real-time area lights are supported. You can turn this off by setting the area light quadratic attenuation to 0.

Area lights use the lossy scale of the light for the area size, since area size is an editor only property.

The light manager contains two dictionaries to modify directional light intensity and shadow strength. If you need to temporarily change the intensity of directional lights, this is a good place to do it.

Downsampling and Depth Buffer
--------------------------------------------------
Weather Maker has built in 1/2, 1/4, 1/8 and 1/16 depth buffer sizes.

The following samplers are available in WeatherMakerCoreShaderInclude.cginc:

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTextureHalf);
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTextureQuarter);
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTextureEighth);
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTextureSixteenth);

You can reference these in your own shaders if you are doing depth buffer downsampling, you can remove your own depth buffer downsampling and simply rely on the weather maker textures, which will be correct for each camera rendered.

Shaders
--------------------------------------------------
Weather Maker contains a number of shaders. Some of the global shader variables may be useful to you if you have your own customer shaders. See WeatherMakerCoreShaderInclude.cginc for a list of the globals and information about them.

Other Integrations / Extensions
--------------------------------------------------
If you are integrating Weather Maker into your own asset, use a #if WEATHER_MAKER_PRESENT ... #endif around your code.

Weather Maker has integration with other assets:

- Crest Ocean
	- Use my fork of crest here: https://github.com/DigitalRuby/crest.
	- Make sure Weather Maker is in your project first. Put crest (crest/Assets/Crest) as a sibling folder to the Weather Maker root folder.
	- Main demo scene has this setup already but you need to:
		- Add an OceanPlanarReflection script to your main camera, this is needed for sky and cloud reflections.
		- Enable planar reflections and shadows on the ocean and ocean underwater material.
		- Add WeatherMakerPrefab to your scene.
		- Set the OceanRenderer script primary dir light to the WeatherMakerPrefab -> DayNightCycle -> Sun object. Set to the Moon object for night scenes.
		- Add reflection probe to player object.
		- Set weather maker fog object render queue to after forward alpha.
		- Add a large plane a few units below crest ocean to provide a full depth buffer if you see any artifacts or depth issues.
		- Turn off reflection probes on all Weather Maker performance profiles if you see flickering.
	- My fork of crest only supports standard pipeline.
- Playmaker
	- Actions for get/set date time and weather profile by name. Let me know if you would like additional actions.
- Vegetation Studio Pro
	- You can customize snow and rain buildup / melt speeds.
	- Add Weatker Maker Vegetation Studio Pro to sync rain/snow/wind from Weather Maker to Vegetation Studio biomes.
	- Attach a weather zone to each biome - great for rain forests, snowy mountains, dry desert, etc.
- uMMORPG
	- Should work out of the box as long as you follow all the mirror network instructions exactly.

Add the following extensions via Component -> Weather Maker -> Extensions -> ...

- Wet Stuff
	- Define WET_STUFF_PRESENT in scripting defines.
	- Set deferred rendering on camera.
	- Drag the WeatherMakerExtensionWetStuffScript to a game object.
	- Add "AutoPuddlePrefab" to scene from Wet Stuff asset. Set position and scale to cover desired area.
	- Set the base wetness source on the "AutoPuddlePrefab" game object to the WeatherMakerExtensionWetStuffScript script.
	- You can add wet stuff particle scripts to the Weather Maker particle systems if desired. Disable the "RainRipples" and "SleetRipples" objects in the WeatherMakerPrefab if this is done.
	- Assign the WeatherMakerExtensionWetStuffScript to the base wetness source on the "AutoPuddlePrefab".
	- In editor mode you can test wetness. In play mode, Weather Maker precipitation determines wetness.
- MegaSplat and MicroSplat
	- Customize rain and snow parameters.
- WAPI (World Manager API)
	- Drag the WeatherMakerExtensionWorldManagerScript to a game object.
	- Don't user weather zones.
	- Setup your cloud profile on the full screen cloud script.
	- (Optional) drag a fog profile into the full screen fog script.
	- WAPI will manage rain, snow, clouds, fog, etc.
- Uber - standard shader ultra
	- You must define UBER_STANDARD_SHADER_ULTRA_PRESENT under player settings -> other settings -> scripting define symbols.
	- Add an UBER_GlobalParams script somewhere in your scene and turn off the "User Particle System" option.
	- Configure global params to get the effect you want.
	- Set the minimum water level and minimum wetness on the WeatherMakerExtensionUberScript on the Weather Maker prefab -> extensions object.
	- Weather Maker will set RainIntensity, WaterLevel, WetnessAmount and SnowLevel automatically.
- RTP - Relief Terrain Pack
	- You must define RELIEF_TERRAIN_PACK_PRESENT under player settings -> other settings -> scripting define symbols.
	- Weather maker will set wetness and snow.
- CTS (Complete Terrain Shader) old version:
	- Weather Maker will set the rain and snow on the terrain when there is precipitation and set the season based on latitude/longitude/date/time.
	- You can control which of these features are on.
- Gaia
	- In the Gaia manager under the GX tab, Digital Ruby, LLC -> Weather Maker are a number of useful options including setting location, moon and/or time of day. This is only available in the Unity editor.

Performance Considerations
--------------------------------------------------
Here is a list of things to try if performance is not adequate:

- First and always, try lowering your Unity quality setting or setting a custom performance profile of lower quality. By default, Weather Maker adapts to the Unity quality level.
- Turn off volumetric clouds or reduce volumetric cloud sample counts.
- Turn on temporal reprojection for clouds.
- Turn off water reflection layers on the water reflection script, set the reflection layers to none.
- Change water reflection shader to VertexLit instead of Forward (this can cause artifacts, watch out for that). Works great for daytime scene with minimal shadows.
- Increase downsample scale for clouds and fog.
- Lower the emission value of the particle systems, especially snow, which is the most CPU intensive.
- Turn off mist by raising the mist threshold to 1 on the rain/snow/hail/sleet script.
- Turn off per pixel lighting on the performance profile.
- Set EnableFogLights to false on the performance profile.
- Turn off precipitation collision on the performance profile.
- Turn off cloud and fog sun shafts on the performance profile.
- Reduce lightning generations and turn off lights (WeatherMakerPrafab/Precipitation/ThunderAndLightning). Turn off lightning glow (set glow intensity to 0).
- Turn off soft particles in quality settings. This will cause hard edges, so it's a trade-off.
- Double check that your terrain and collidable objects are setup as efficient as possible. For example, very large terrain can cause performance issues. Use a collision plane if possible.
- Reduce the MaximumLightCount constant in WeatherMakerLightManagerScript.cs if GPU is pegged and you have many lights in your scene.
- Turn off CheckForEclipse on the sky sphere if you are seeing CPU spikes. These can be caused by a Unity bug.
- Reduce null zone count.

Troubleshooting / FAQ
--------------------------------------------------
- If you are getting strange errors and compile problems, you should remove Weather Maker entirely and redownload from the asset store and reimport and readd any WeatherMakerPrefab objects.
- Clouds are ghosting / blurring too much: Turn OFF WeatherMakerPrefab -> FullScreenEffects -> Clouds -> Auto Set Temporal Reprojection Blend Mode.
- Clouds or shadow are flickering, wiggling or jiggling, jitter, etc. : try post processing: depth of field, bloom, color grading, temporal AA and/or motion blur.
- Too much fog - double check your atmosphere fog density and other atmosphere settings on the sky sphere -> sky profile -> atmosphere profile.
- Everything is gray / grey - Double check that LWRP or HDRP is not turned on and that any pre-processor definitions have removed any LWRP or HDRP defines.
- Scene view UI is white or flickering - turn off enable scene camera on root of prefab on WeatherMakerScript.
- Weather zone is not triggering: Make sure your player has kinematic rigid body, trigger sphere collider and that project physics settings allows the player and weather zone layer to collide.
- Rain, mist, etc. looks bad - make sure HDR is on for your camera.
- Night sky is not showing: Try turning up the night intensity on your sky sphere profile.
- When your mouse leaves Unity and comes back or you unfocus the editor, the sky might go black. To fix, deactivate and reactivate the weather maker prefab. Unity command buffer bug is responsible. Editor only problem.
- If rain, snow, etc. is flying up instead of down, double check all the velocity over time values on the particle systems. Everything except the explosions should have negative y values.
- Fog is not showing or is all white - Double check that the WeatherMakerPrefab is setup in your scene properly and the Sun object is enabled.
- If you see shadow flickering, try adding some wind to vary the trees up a bit or increase the day CelestialObjectRotationUpdateInterval value on the day night script. For the fog, consider disabling shadows if it is enabled, or lowering the shadow map resolution in quality settings.
- Shadows look funny: try changing your shadow distance in quality settings and/or increasing cascades to 2 or 4. Change WeatherMakerPrefab -> DayNightCycle -> Sun -> Celestial Object Script -> Base Shadow Strength to 1.
- The Unity post processing stack is highly recommended to cleanup any additional banding or aliasing, etc. Temporal anti-alising really helps, along with the right level of bloom.
- I see weird depth buffer issues at night with particle systems: Try setting the dest blend mode of the mist particle systems to 1 (add) if you have a very dark / night scene.
- Trees get covered by fog or are in front of fog: Tree billboards need to have WeatherMakerTreeBillboardShader enabled (see top of that file for instructions).
- Sun shafts and other effects aren't working in player: Make sure scene tab isn't showing, hide the tab. Not sure why this causes problems but until I figure it out this is the fix.
- For ultimate survival or other custom camera assets, try tweaking the clear flags or culling mask if you see display glitches.
- Cloud shadows - cloud shadows fade out at distance due to Unity shadow fade defaults, see volumetric cloud section on how to work around this.
- Reflection probes - make sure to not use box projection, this is not supported. Set mode to realtime as well if you want to show moving clouds, lightning, etc.
- Flickering lights - try turning off reflection probes in performance profile.

Known Issues
--------------------------------------------------
- Sky sphere may have some slight distortion at the poles when using sphere or panorama mode. This is easily fixed by using dome or double dome mode, or by correcting the texture.
- Fish Eye 360 sky sphere texture mode has some distortion at the side pole of the sphere. I have as of yet been unable to fix this. I welcome suggestions and feedback on how to fix this.
- Sun can shine through the bottom as it dips below the horizon. Fix this by adding a large shadow casting cube as the base of your world. Find the LargeGroundAndSunHorizonBlocker object in the demo scene for an example.
- It is recommended to disable Odin serialization for Weather Maker, as this has some weird bugs.

Wrap Up
--------------------------------------------------
I'm Jeff Johnson, I created Weather Maker just for you. Please email support@digitalruby.com with any feedback, bug reports or suggestions.

- Jeff

Credits
--------------------------------------------------
Third Party Code / Assets
- https://github.com/SlightlyMad/AtmosphericScattering (Atmospheric scattering, heavily modified, BSD license)
- https://www.turbosquid.com/FullPreview/Index.cfm/ID/711530 (Airplane, free license)

Code / Rendering:
- https://github.com/playdeadgames/temporal
- https://catlikecoding.com/unity/tutorials/flow/looking-through-water/
- https://patapom.com/topics/Revision2013/Revision%202013%20-%20Real-time%20Volumetric%20Rendering%20Course%20Notes.pdf
- http://advances.realtimerendering.com/s2017/Nubis%20-%20Authoring%20Realtime%20Volumetric%20Cloudscapes%20with%20the%20Decima%20Engine%20-%20Final%20.pdf
- https://gist.github.com/stephenmerendino/8b8aea77ac8d69a4427de588475cc0d2
- https://www.gamedev.net/forums/topic/680832-horizonzero-dawn-cloud-system/
- http://killzone.dl.playstation.net/killzone/horizonzerodawn/presentations/Siggraph15_Schneider_Real-Time_Volumetric_Cloudscapes_of_Horizon_Zero_Dawn.pdf
- https://bib.irb.hr/datoteka/949019.Final_0036470256_56.pdf
- http://bitsquid.blogspot.com/2016/07/volumetric-clouds.html
- http://petewerner.blogspot.com/2015/02/intro-to-curl-noise.html

Free Sound and Graphics:
- http://soundbible.com/1718-Hailstorm.html
- http://blenderartists.org/forum/archive/index.php/t-24038.html
- https://www.binpress.com/tutorial/creating-an-octahedron-sphere/162
- http://freesound.org/
- http://www.orangefreesounds.com/meditation-music-for-relaxation-and-dreaming/
- https://opengameart.org/content/seamless-animated-raindrop-ripples-texture
- https://freesound.org/people/akemov/sounds/255597/
- https://freesound.org/people/InspectorJ/sounds/398700/
- https://www.bensound.com "Royalty Free Music from Bensound"
