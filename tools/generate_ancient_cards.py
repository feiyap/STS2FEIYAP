#!/usr/bin/env python3
"""为绯夜氏 7 张先古卡各生成 5 个批次的 SD 卡图。"""

from __future__ import annotations

import base64
import json
import os
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

# 绕过系统代理，避免本地 SD WebUI 请求返回 502。
os.environ["NO_PROXY"] = "127.0.0.1,localhost"
os.environ["no_proxy"] = "127.0.0.1,localhost"
urllib.request.install_opener(urllib.request.build_opener(urllib.request.ProxyHandler({})))

API_URL = "http://127.0.0.1:7860/sdapi/v1/txt2img"
OUTPUT_ROOT = Path(__file__).resolve().parents[1] / "generated_cards"
LOG_FILE = OUTPUT_ROOT / "generation_log.json"

NEGATIVE_PROMPT = (
    "lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,"
    "cropped,worst quality,low quality,jpeg artifacts,signature,watermark,username,blurry,"
    "artist name,multiple views,extra limbs,bad face,deformed,ugly"
)

STYLE_PREFIX = (
    "masterpiece,best quality,very aesthetic,highres,"
    "anime style illustration,game card art,portrait composition,"
    "dynamic angle,cinematic lighting,vivid colors,detailed background,"
    "ancient card rarity,epic mythic atmosphere,"
)

CHARACTER_BLOCK = (
    "<lora:hakurei_reimu XL:0.7>,<lora:sukotteiforill:0.5>,<lora:aosiai123_style:0.5>,"
    "1girl,solo,female samurai,"
    "very long hair,(black hair:1.5),long straight hair,white hair band,(golden eyes:1.5),white pupils,"
    "white coat,white shirt,black corset,black skirt,"
    "katana,japanese sword,"
)

BATCH_SUFFIXES = [
    "from below,dramatic perspective,",
    "from above,bird eye view,",
    "close-up,upper body focus,",
    "wide shot,full body,",
    "dutch angle,tilted frame,dynamic composition,",
]

CARDS: dict[str, str] = {
    # 渴血症：握牌受伤叠段，暗红杀意汇入刀身
    "KeXueZheng": (
        CHARACTER_BLOCK
        + "blood thirst theme,wounded side kneeling pose,hand clutching bleeding wound on arm,"
        "dark crimson mist flowing into katana blade not outward,"
        "multiple overlapping slash marks around sword each brighter than last,"
        "blood droplet suspended on blade tip,gripping katana while injured,"
        "dark battlefield ruins,moonlight,crimson and deep purple palette,"
        "dangerous restrained mood,about to unleash stored fury,"
        "glowing blue katana edge mixed with blood red aura,"
    ),
    # 绯樱狱华落：居合终极绽放，樱焰落花
    "FeiYingYuHuaLuo": (
        CHARACTER_BLOCK
        + "iaido,draw sword slash moment,epic low angle composition,"
        "giant crimson sakura tree,cherry blossom cloud,petals falling as flames,"
        "massive fan-shaped slash wave,blue-white iaido core wrapped in crimson fire,"
        "cracked ground,faint torii gate silhouette in burning sakura clouds,"
        "devastating cherry blossom fallout,prison hell cherry theme,"
        "glowing katana slash,inner cold outer burning contrast,"
        "melancholic violent beauty,finishing blow pose,"
    ),
    # 无声星陨：热寂末世，巨星无声碎裂
    "WuShengXingYun": (
        CHARACTER_BLOCK
        + "silent starfall,giant star silently shattering in night sky,"
        "star debris turning to ash dust without explosion,heat death theme,"
        "stars dying one by one across cosmos,descending grey entropy mist,"
        "deep space black,dying star gold,ash grey palette,"
        "katana tip pointing at falling star trail,looking up at sky,"
        "floating playing card silhouette with embedded meteor shard texture,"
        "cosmic silence,apocalyptic grandeur,no sound devastation,"
        "minimal cold glow on blade and dying star core only,"
    ),
    # XXI-世界：塔罗世界牌，曼陀罗与飞卡（不含绯夜氏角色）
    "WorldXxi": (
        "tarot card frame,roman numeral XXI,the world tarot,"
        "four elemental guardians mandala circle,human eagle bull lion symbols,"
        "complete golden white cosmic ring,wreath of victory,"
        "multiple tarot cards flying from center like star orbits,"
        "ornate tarot border ornament,fate and destiny theme,"
        "mystical glowing mandala,sacred majestic bright atmosphere,"
        "female figure silhouette at center as world embodiment,arms spread,"
        "playing cards orbiting the ring,cosmic completeness,"
        "no anime character face focus,tarot illustration style,"
    ),
    # 于万千碎裂的世界破片：碎裂多元世界，力量涌入
    "WorldShards": (
        CHARACTER_BLOCK
        + "shattered mirror multiverse,space cracked like broken glass,"
        "multiple world fragments reflecting forest city stars ruins realms,"
        "standing on largest floating shard,shard edges cutting arms,"
        "crimson strength aura flowing from wounds into body,"
        "tense muscles,red power rings,inner strength awakening,"
        "multicolored shard reflections,crimson strength glow vs cold blue rift light,"
        "broken worlds theme,ancient cosmic debris,"
        "easter egg card gear constellation motifs on shards,"
        "determined weathered expression,power card art,"
    ),
    # 雨曾为紫：紫雨护罩，活力常驻
    "YuCengWeiZi": (
        CHARACTER_BLOCK
        + "violet purple rain pouring heavily,translucent rain shield around body,"
        "closed eyes calm standing pose,raindrops turning to golden-green vigor light,"
        "vigor aura pulsing at chest and katana sheath,light grows steadier in rain,"
        "blurred ancient rooftop background,dusty grimoire pages motif,"
        "purple indigo gold green palette,nostalgic protective poetic atmosphere,"
        "rain was purple theme,mystical rainfall,retain innate power,"
        "raindrops never sliding off body,transforming into energy particles,"
    ),
    # 斋时雨：细雨冥想，居合雨刃翻倍
    "ZhaiShiYu": (
        CHARACTER_BLOCK
        + "fine drizzle rain,zen meditation,kneeling iaido preparation pose,"
        "katana across knees,rain attracted to blade forming second rain-blade,"
        "every raindrop reflecting double sword shadow,symmetrical composition,"
        "doubled iaido blue light refracted through rain curtain,"
        "japanese garden,stone lantern,moss garden,teahouse atmosphere,"
        "grey-blue rain palette,iaido blue glow,subtle crimson accent,"
        "quiet before storm,accumulating power in stillness,"
        "tooth-shaped light pattern on scabbard,ancient fang motif,"
        "serene zen rain,iaido rain power doubling theme,"
    ),
}


def build_payload(prompt_body: str, batch_index: int, seed: int) -> dict:
    prompt = STYLE_PREFIX + prompt_body + BATCH_SUFFIXES[batch_index]
    return {
        "prompt": prompt,
        "negative_prompt": NEGATIVE_PROMPT,
        "width": 1000,
        "height": 760,
        "sampler_name": "Euler a",
        "scheduler": "Karras",
        "cfg_scale": 7,
        "steps": 30,
        "seed": seed,
        "batch_size": 1,
        "n_iter": 1,
        "enable_hr": True,
        "hr_upscaler": "4x-AnimeSharp",
        "hr_second_pass_steps": 10,
        "denoising_strength": 0.3,
        "hr_scale": 2,
    }


def call_api(payload: dict, retries: int = 3) -> bytes:
    data = json.dumps(payload).encode("utf-8")
    last_error: Exception | None = None

    for attempt in range(1, retries + 1):
        request = urllib.request.Request(
            API_URL,
            data=data,
            headers={"Content-Type": "application/json"},
            method="POST",
        )
        try:
            with urllib.request.urlopen(request, timeout=900) as response:
                result = json.loads(response.read().decode("utf-8"))
            images = result.get("images") or []
            if not images:
                raise RuntimeError("API 未返回图片")
            return base64.b64decode(images[0])
        except (urllib.error.URLError, TimeoutError, RuntimeError) as exc:
            last_error = exc
            if attempt < retries:
                wait_seconds = attempt * 15
                print(f"  重试 {attempt}/{retries - 1}，{wait_seconds}s 后重试: {exc}")
                time.sleep(wait_seconds)
            continue

    assert last_error is not None
    raise last_error


def load_log() -> dict:
    if LOG_FILE.exists():
        return json.loads(LOG_FILE.read_text(encoding="utf-8"))
    return {"completed": [], "errors": []}


def save_log(log: dict) -> None:
    LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
    LOG_FILE.write_text(json.dumps(log, ensure_ascii=False, indent=2), encoding="utf-8")


def main() -> int:
    only_cards = [arg for arg in sys.argv[1:] if not arg.startswith("--")]
    force = "--force" in sys.argv

    cards = CARDS
    if only_cards:
        cards = {name: CARDS[name] for name in only_cards if name in CARDS}

    log = load_log()
    completed = set(log.get("completed", []))

    total = len(cards) * len(BATCH_SUFFIXES)
    done_count = 0

    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)

    for card_name, body in cards.items():
        card_dir = OUTPUT_ROOT / card_name
        card_dir.mkdir(parents=True, exist_ok=True)

        for batch_index, batch_suffix in enumerate(BATCH_SUFFIXES):
            output_path = card_dir / f"batch_{batch_index + 1:02d}.png"
            task_key = f"{card_name}/batch_{batch_index + 1:02d}"

            if output_path.exists() and not force and task_key in completed:
                done_count += 1
                print(f"[跳过] {task_key} 已存在")
                continue

            seed = abs(hash(task_key)) % (2**32 - 1)
            payload = build_payload(body, batch_index, seed)

            print(f"[生成] ({done_count + 1}/{total}) {task_key} seed={seed}")
            started = time.time()

            try:
                image_bytes = call_api(payload)
                output_path.write_bytes(image_bytes)
                elapsed = time.time() - started
                print(f"[完成] {task_key} -> {output_path} ({elapsed:.1f}s)")

                if task_key not in completed:
                    log.setdefault("completed", []).append(task_key)
                save_log(log)
                done_count += 1
            except (urllib.error.URLError, TimeoutError, RuntimeError) as exc:
                message = f"{task_key}: {exc}"
                print(f"[错误] {message}", file=sys.stderr)
                log.setdefault("errors", []).append(message)
                save_log(log)
                return 1

    print(f"全部完成：{done_count}/{total}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
