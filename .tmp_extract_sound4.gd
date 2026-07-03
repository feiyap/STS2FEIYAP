extends SceneTree

func _init() -> void:
    ProjectSettings.load_resource_pack("F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck")
    var path := "res://Danjin/Sounds/Cards/jian_qi.ogg"
    var file = FileAccess.open(path, FileAccess.READ)
    print("open=", file)
    if file:
        var data = file.get_buffer(file.get_length())
        file.close()
        var out = FileAccess.open("H:/STS2MOD/Feiyap/Feiyap/Sounds/iaido_slash.ogg", FileAccess.WRITE)
        out.store_buffer(data)
        out.close()
        print("wrote ", data.size())
    quit()
