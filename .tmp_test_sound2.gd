extends SceneTree

func _init() -> void:
    var paths = [
        "res://Feiyap/Sounds/iaido_slash.ogg",
        "res://Feiyap/.godot/imported/iaido_slash.ogg-6c58c31ddfba53e8524f4c4759658575.oggvorbisstr",
    ]
    for path in paths:
        print(path, " exists=", ResourceLoader.exists(path))
        var stream = ResourceLoader.load(path)
        print("  stream=", stream)
        if stream:
            print("  length=", stream.get_length())
    quit()
