extends SceneTree

const PACK := "F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck"
const OUT := "H:/STS2MOD/Feiyap/Feiyap/Sounds/iaido_slash.ogg"

func _init() -> void:
    ProjectSettings.load_resource_pack(PACK)
    var path := "res://Danjin/Sounds/Cards/jian_qi.ogg"
    if not FileAccess.file_exists(path):
        push_error("Missing " + path)
        quit(1)
        return
    DirAccess.make_dir_recursive_absolute("H:/STS2MOD/Feiyap/Feiyap/Sounds")
    var data = FileAccess.get_file_as_bytes(path)
    var out = FileAccess.open(OUT, FileAccess.WRITE)
    out.store_buffer(data)
    out.close()
    print("Extracted ", data.size(), " bytes to ", OUT)
    quit()
