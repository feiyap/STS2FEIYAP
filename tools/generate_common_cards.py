#!/usr/bin/env python3
"""为绯夜氏 20 张普通卡各生成 5 个批次的 SD 卡图。"""

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
    "FeiyapCommon1": (
        CHARACTER_BLOCK
        + "evening cherry blossoms,sakura petals falling,sunset sky,pink and purple atmosphere,"
        "half-drawn katana,cherry petals frozen on blade,serene but deadly mood,"
        "glowing blue katana edge,calm standing pose,"
    ),
    "FeiyapCommon2": (
        CHARACTER_BLOCK
        + "swallow bird motif,low angle shot,aerial turning slash,"
        "U-shaped blue energy arc,swift lightweight strike,"
        "swallow silhouette,speed lines,bright sky background,"
        "glowing blue katana slash,motion blur,"
    ),
    "FeiyapCommon3": (
        CHARACTER_BLOCK
        + "pine forest,wind through pine trees,flying pine needles,"
        "double slash,two parallel blue energy slashes,multi-hit attack,"
        "forest background,green and blue color scheme,"
        "glowing blue katana slash,action pose,"
    ),
    "FeiyapCommon4": (
        CHARACTER_BLOCK
        + "crescent moon,night sky,moonlight on blade,"
        "lingering sword intent,afterimage of slash,moonbeam glow on katana tip,"
        "cold blue and silver palette,melancholic atmosphere,"
        "glowing blue katana slash,finishing blow pose,"
    ),
    "FeiyapCommon5": (
        CHARACTER_BLOCK
        + "cold north wind,ice crystal wind,blizzard atmosphere,"
        "four parallel slash marks,rapid consecutive slashes,"
        "side view combo attack,winter cold palette,"
        "glowing blue katana slash,motion blur,speed lines,"
    ),
    "FeiyapCommon6": (
        CHARACTER_BLOCK
        + "constellation,cygnus constellation,star map,connecting stars,starry night sky,"
        "stellar slash,katana tip pointing at bright star,"
        "floating card silhouettes among stars,constellation theme,"
        "glowing blue katana slash,action pose,"
    ),
    "FeiyapCommon7": (
        CHARACTER_BLOCK
        + "vega star,lyra constellation,milky way,stardust,"
        "double strike,two overlapping slashes,second slash brighter,"
        "weaver star motif,galaxy background,"
        "glowing blue katana slash,twin slash effect,"
    ),
    "FeiyapCommon8": (
        CHARACTER_BLOCK
        + "altair star,aquila constellation,red star glow,"
        "burning slash,inner flame effect,blue to red energy transition,"
        "internal fire aura,hot and cold contrast,"
        "glowing katana slash,fire and blue energy mix,"
    ),
    "FeiyapCommon9": (
        CHARACTER_BLOCK
        + "tarot card frame,roman numeral XIX,the sun tarot,"
        "giant golden sun,sunflower,solar corona,radiant sunlight,"
        "slash from sun disc,golden and blue energy,"
        "tarot border ornament,majestic bright atmosphere,"
        "glowing katana slash,powerful strike pose,"
    ),
    "FeiyapCommon10": (
        CHARACTER_BLOCK
        + "iaido,breathing technique,meditation pose,kneeling or low stance,"
        "absorbing blue energy into body,energy streams flowing inward,"
        "quick absorption,scabbard,closed eyes or half-closed eyes,"
        "blue aura spiraling toward katana sheath,inner power gathering,"
    ),
    "FeiyapCommon11": (
        CHARACTER_BLOCK
        + "still water,water surface mirror reflection,ripples,"
        "scabbard touching water,hexagonal blue shield forming from ripples,"
        "calm defensive stance,symmetrical reflection,"
        "blue magical shield,serene atmosphere,"
    ),
    "FeiyapCommon12": (
        CHARACTER_BLOCK
        + "multiple ghostly afterimages,parallel world silhouettes,"
        "heart transmission,spiritual legacy,martial arts inheritance,"
        "defending with sword back,blue shield from chest,"
        "overlapping transparent duplicates,golden eye glow,"
        "blue magical shield,defensive stance,"
    ),
    "FeiyapCommon13": (
        CHARACTER_BLOCK
        + "hazy moonlight,fog,mist,blurred duplicate silhouette,"
        "iaido,draw sword from sheath,phantom afterimage,"
        "agile nimble pose,shadow stepping,"
        "blue iaido flash in fog,ethereal atmosphere,"
    ),
    "FeiyapCommon14": (
        CHARACTER_BLOCK
        + "stepping on own shadow,dark shadow on ground,"
        "shadow shattering into blue energy,energy rising up leg to scabbard,"
        "ghostly footwork,dynamic stepping pose,"
        "playing card dissolving into shadow,iaido preparation,"
        "blue aura around katana,"
    ),
    "FeiyapCommon15": (
        CHARACTER_BLOCK
        + "noto,sheathing sword,sword fully in scabbard,"
        "both hands on scabbard,intense focused gaze,"
        "blue light leaking from scabbard gap,accumulating power,"
        "minimal background,ready stance,before battle,"
        "iaido,sharp concentration,"
    ),
    "FeiyapCommon16": (
        CHARACTER_BLOCK
        + "snowfall,snowy night,spinning body turn,"
        "snowflakes forming shield,snow turning into blue sword energy,"
        "returning snow motif,white and blue palette,"
        "blue magical shield,iaido blue glow,defensive spin,"
    ),
    "FeiyapCommon17": (
        CHARACTER_BLOCK
        + "stellar bridge,bridge made of starlight in night sky,"
        "trading cards across star bridge,one card sinking one card rising,"
        "cosmic pathway,constellation background,"
        "hands reaching across light bridge,magical card exchange,"
        "upper body focus,fantasy sci-fi atmosphere,"
    ),
    "FeiyapCommon18": (
        CHARACTER_BLOCK
        + "star curtain,celestial veil,falling starry curtain,"
        "semi-transparent constellation barrier,shield of stars,"
        "playing cards falling like shooting stars behind curtain,"
        "cosmic dome,deep space background,"
        "blue magical shield,standing before star curtain,"
    ),
    "FeiyapCommon19": (
        "tarot card frame,roman numeral X,wheel of fortune tarot,"
        "spinning wheel of fate,sphinx,anubis symbolic figures,"
        "golden white side and dark purple side,split duality,"
        "female hand touching wheel edge,ornate tarot border,"
        "fortune turning,fate and destiny theme,"
        "mystical glowing wheel,no character face focus,"
    ),
    "FeiyapCommon20": (
        "tarot card frame,roman numeral XVII,the star tarot,"
        "eight pointed stars,naked figure silhouette pouring water into pool,"
        "starlight reflection on water,night sky full of stars,"
        "playing cards floating from water surface,"
        "attack cards with red glow,skill cards with blue glow,"
        "hope and inspiration theme,majestic tarot illustration,"
        "ornate tarot border,no anime character,"
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
