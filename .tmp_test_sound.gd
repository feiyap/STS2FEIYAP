extends SceneTree

func _init() -> void:
    var path := "res://Feiyap/Sounds/iaido_slash.ogg"
    print("exists=", ResourceLoader.exists(path))
    var stream = ResourceLoader.load(path)
    print("stream=", stream, " class=", stream.get_class() if stream else null)
    if stream:
        print("length=", stream.get_length())
    quit()
