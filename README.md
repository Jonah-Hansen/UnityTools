# UnityTools
useful scripts for unity that follow a scriptable object service-based architecture.

currently included 
### `PlayerInputService`
  an alternative to the PlayerInput monobehaviour component that works well with the Unity Input System. connects with an `InputActionsAsset` generated c# class.

### `MenuControllerService`
  extendable scriptable object service for managing UI windows with a Stack structure. Connects to a `PlayerInputService` asset for closing windows with a 'cancel' button and uses `UIElement` Pages.
  
### `UIElement`
  extendable monobehaviour for UI game objects to work with a `MenuControllerService`.

### `SaveLoadService`
  a scriptable object service for storing and retrieving `SerializablePlayerData` from the unity PlayerPrefs. requires implementation of a `SerializablePlayerData` class.
