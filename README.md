# UnityTools
useful scripts for unity that follow a scriptable object service-based architecture.

currently included 
### `InputService`
  an alternative to the built in PlayerInput monobehaviour component that works well with the Unity Input System. connects with an `InputActionsAsset` generated c# class.

### `MenuController`
  extendable monobehaviour for managing UI windows with a Stack structure. Connects to a `InputService` asset for closing windows and performing other actions with input events and uses `UIElement` Pages. simply extend and attach to an empty game object in your main canvas. pages can be added as children.
  
### `UIElement`
  extendable monobehaviour for UI game objects to work with a `MenuControllerService` provides access to its RectTransform and CanvasGroup, and invokes events for when closing is requested, and performed.

### `SaveLoadService`
  a scriptable object service for storing and retrieving `SerializablePlayerData` from the unity PlayerPrefs. requires implementation of a `SerializablePlayerData` class.

### `Utilities`
  global utilities. includes WaitOneFrame() which allows easily running a callback on the next frame.