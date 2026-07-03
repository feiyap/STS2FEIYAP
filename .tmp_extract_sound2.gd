extends SceneTree

func _init() -> void:
    var pack := "F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck"
    var err = ProjectSettings.load_resource_pack(pack)
    print("load pack err=", err)
    var path := "res://Danjin/Sounds/Cards/jian_qi.ogg"
    print("exists=", FileAccess.file_exists(path))
    if ResourceLoader.exists(path):
        print("ResourceLoader exists")
    var stream = ResourceLoader.load(path)
    print("stream=", stream)
    quit()
