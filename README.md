# UnityTools
useful scripts for unity that follow a scriptable object service-based architecture.

currently included 
### `PlayerInputService`
  an alternative to the PlayerInput monobehaviour component that works well with the Unity Input System. connects with an `InputActionsAsset` generated c# class.

### `MenuController`
  extendable monobehaviour for managing UI windows with a Stack structure. Connects to a `PlayerInputService` asset for closing windows with a 'cancel' button and uses `UIElement` Pages. simply extend and attach to an empty game object in your main canvas. pages can be added as children.
  
### `UIElement`
  extendable monobehaviour for UI game objects to work with a `MenuControllerService`.

### `SaveLoadService`
  a scriptable object service for storing and retrieving `SerializablePlayerData` from the unity PlayerPrefs. requires implementation of a `SerializablePlayerData` class.
