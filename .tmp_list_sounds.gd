extends SceneTree

const PACK := "F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck"

func _init() -> void:
    ProjectSettings.load_resource_pack(PACK)
    var dir = DirAccess.open("res://Danjin/Sounds")
    if dir:
        _walk("res://Danjin/Sounds", dir)
    else:
        print("No Sounds dir")
    quit()

func _walk(path: String, dir: DirAccess) -> void:
    dir.list_dir_begin()
    var f = dir.get_next()
    while f != "":
        var full = path + "/" + f if path != "" else f
        if dir.current_is_dir():
            var sub = DirAccess.open(path + "/" + f)
            if sub:
                _walk(path + "/" + f, sub)
        elif f.ends_with(".ogg") or f.ends_with(".import"):
            print(full)
        f = dir.get_next()
