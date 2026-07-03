extends SceneTree

func _init() -> void:
    ProjectSettings.load_resource_pack("F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck")
    var stream = ResourceLoader.load("res://Danjin/Sounds/Cards/jian_qi.ogg")
    print("stream=", stream)
    if stream:
        for prop in stream.get_property_list():
            if prop.name in ["data", "file", "loop", "loop_offset", "bpm", "beat_count"]:
                print(prop.name, stream.get(prop.name))
    quit()
