"""Desenha o vírus em pixel art (canvas 24 x 24, exportado em 3x -> 72 x 72).

Cores seguem o sprite provisório do projeto: corpo verde e espículas roxas.
"""
import math
import os
import numpy as np
from common import grid, outline, dither_fade, hex_to_rgba, save_scaled

W, H = 24, 24
X, Y = grid(W, H)

C = {
    'outline': '#1D2633',
    'body': '#8ED35B', 'body_hi': '#C9F597', 'body_sh': '#4F9A3C',
    'spike': '#A95BD6', 'spike_hi': '#D9A6F2', 'spike_sh': '#7A3AA6',
    'face': '#18202E',
}


def capsule(p0, p1, radius):
    """Pixels a uma distância menor ou igual a radius do segmento p0-p1."""
    ax, ay = p0
    bx, by = p1
    dx, dy = bx - ax, by - ay
    length_sq = dx * dx + dy * dy
    t = np.clip(((X - ax) * dx + (Y - ay) * dy) / length_sq, 0, 1)
    px = ax + t * dx
    py = ay + t * dy
    return (X - px) ** 2 + (Y - py) ** 2 <= radius * radius


def draw_virus(center_y=12.0, spin_deg=0.0, scale=1.0):
    cx, cy = 12.0, center_y
    body_r = 7.0 * scale

    body = (X - cx) ** 2 + (Y - cy) ** 2 <= body_r ** 2

    spikes = np.zeros((H, W), dtype=bool)
    tips = np.zeros((H, W), dtype=bool)
    for k in range(8):
        angle = math.radians(k * 45.0 + spin_deg)
        ux, uy = math.cos(angle), math.sin(angle)
        p0 = (cx + ux * (body_r - 1.0), cy + uy * (body_r - 1.0))
        p1 = (cx + ux * (body_r + 3.6 * scale), cy + uy * (body_r + 3.6 * scale))
        spikes |= capsule(p0, p1, 0.95 * scale)
        tip = (X - p1[0]) ** 2 + (Y - p1[1]) ** 2 <= (1.7 * scale) ** 2
        tips |= tip

    silhouette = body | spikes | tips
    img = np.zeros((H, W, 4), dtype=np.uint8)

    def paint(mask, key):
        img[mask] = hex_to_rgba(C[key])

    paint(outline(silhouette), 'outline')
    paint(spikes | tips, 'spike')
    paint(tips & ((X - cx) + (Y - cy) < -1.5), 'spike_hi')
    paint(spikes & ((Y - cy) > 2.5), 'spike_sh')

    paint(body, 'body')
    paint(body & ((X - (cx - 2.2)) ** 2 + (Y - (cy - 2.4)) ** 2 <= (3.2 * scale) ** 2), 'body_hi')
    paint(body & ((Y - cy) > 3.6 * scale), 'body_sh')

    # Rosto: dois olhos 1x2 e uma boca curta.
    eye_l = (X >= cx - 3.5) & (X <= cx - 2.5) & (Y >= cy - 2.2) & (Y <= cy - 0.2)
    eye_r = (X >= cx + 2.5) & (X <= cx + 3.5) & (Y >= cy - 2.2) & (Y <= cy - 0.2)
    mouth = (X >= cx - 1.5) & (X <= cx + 1.5) & (Y >= cy + 2.5) & (Y <= cy + 3.5)
    if scale >= 0.6:
        paint(body & (eye_l | eye_r | mouth), 'face')

    img[img[:, :, 3] == 0] = 0
    return img


def build_frames():
    frames = {}

    # Andar: 6 frames. As espículas giram e o corpo sobe e desce.
    walk = []
    steps = 6
    for i in range(steps):
        t = 2 * math.pi * i / steps
        bob = -0.6 * abs(math.sin(t))
        spin = 7.5 * math.sin(t)
        walk.append(draw_virus(center_y=12.0 + bob, spin_deg=spin))
    frames['Walk'] = walk

    # Morte: 6 frames. Estoura (cresce), encolhe e some com dither.
    death_scales = [1.0, 1.12, 0.95, 0.7, 0.45, 0.25]
    death_keeps = [1.0, 1.0, 1.0, 0.8, 0.45, 0.15]
    death = []
    for s, k in zip(death_scales, death_keeps):
        f = draw_virus(center_y=12.0, spin_deg=0.0, scale=s)
        death.append(dither_fade(f, k))
    frames['Death'] = death
    return frames


if __name__ == '__main__':
    import sys
    out_dir = sys.argv[1] if len(sys.argv) > 1 else '/home/claude/art/out/virus'
    frames = build_frames()
    for name, seq in frames.items():
        folder = os.path.join(out_dir, name)
        os.makedirs(folder, exist_ok=True)
        for i, f in enumerate(seq):
            save_scaled(f, os.path.join(folder, f'frame_{i:02d}.png'), 3)
        print(name, len(seq))
