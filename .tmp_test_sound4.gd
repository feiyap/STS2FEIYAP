extends SceneTree
func _init() -> void:
    var path := "res://Feiyap/Sounds/iaido_slash.oggvorbisstr"
    print(ResourceLoader.exists(path), ResourceLoader.load(path))
    quit()
