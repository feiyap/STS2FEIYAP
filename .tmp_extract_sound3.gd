extends SceneTree

func _init() -> void:
    ProjectSettings.load_resource_pack("F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck")
    var path := "res://Danjin/Sounds/Cards/jian_qi.ogg"
    var stream = ResourceLoader.load(path)
    var out := "H:/STS2MOD/Feiyap/Feiyap/Sounds/iaido_slash.ogg"
    DirAccess.make_dir_recursive_absolute("H:/STS2MOD/Feiyap/Feiyap/Sounds")
    var err = ResourceSaver.save(stream, out)
    print("save err=", err, " type=", stream.get_class())
    quit()
