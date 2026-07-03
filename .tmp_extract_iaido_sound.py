import os
import struct

PCK = r"F:/SteamLibrary/steamapps/workshop/content/2868840/3747549749/Danjin.pck"
BASE = r"H:/STS2MOD/Feiyap/Feiyap"
IMPORTED_DIR = os.path.join(BASE, ".godot", "imported")
SOUNDS_DIR = os.path.join(BASE, "Sounds")
HASH = "6c58c31ddfba53e8524f4c4759658575"
IMPORTED_NAME = f"iaido_slash.ogg-{HASH}.oggvorbisstr"

os.makedirs(IMPORTED_DIR, exist_ok=True)
os.makedirs(SOUNDS_DIR, exist_ok=True)

with open(PCK, "rb") as f:
    data = f.read()

file_base = struct.unpack("<Q", data[24:32])[0]
dir_offset = struct.unpack("<Q", data[32:40])[0]
pos = dir_offset + 4
count = struct.unpack("<I", data[dir_offset : dir_offset + 4])[0]

for _ in range(count):
    plen = struct.unpack("<I", data[pos : pos + 4])[0]
    pos += 4
    path = data[pos : pos + plen].decode("utf-8").rstrip("\x00")
    pos += plen
    pad = (4 - (plen % 4)) % 4
    pos += pad
    foff = struct.unpack("<Q", data[pos : pos + 8])[0]
    pos += 8
    fsize = struct.unpack("<Q", data[pos : pos + 8])[0]
    pos += 8
    pos += 16
    pos += 4
    if f"jian_qi.ogg-{HASH}.oggvorbisstr" in path:
        blob = data[file_base + foff : file_base + foff + fsize]
        out = os.path.join(IMPORTED_DIR, IMPORTED_NAME)
        with open(out, "wb") as wf:
            wf.write(blob)
        print("wrote", out, len(blob))

import_text = f"""[remap]

importer="oggvorbisstr"
type="AudioStreamOggVorbis"
uid="uid://cfeiyapiaidoslash"
path="res://.godot/imported/{IMPORTED_NAME}"
"""
with open(os.path.join(SOUNDS_DIR, "iaido_slash.ogg.import"), "w", encoding="utf-8") as wf:
    wf.write(import_text)

# 占位源文件；运行时通过 .import 重映射到 oggvorbisstr。
with open(os.path.join(SOUNDS_DIR, "iaido_slash.ogg"), "wb") as wf:
    wf.write(b"")

print("done")
