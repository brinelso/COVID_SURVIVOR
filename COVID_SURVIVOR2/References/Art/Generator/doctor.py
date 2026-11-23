"""Desenha o médico em pixel art a partir de formas geométricas.

Canvas: 24 x 32 pixels (exportado em 3x -> 72 x 96).
Cores extraídas da referência References/Art/doctor_reference.png.
"""
import numpy as np
from common import grid, shift, outline, dither_fade, hex_to_rgba

W, H = 24, 32
X, Y = grid(W, H)

C = {
    'outline': '#14141F',
    'hair_hi': '#F2F2F9', 'hair': '#BFC3D6', 'hair_sh': '#858AA0',
    'skin': '#F5B98A', 'skin_sh': '#D98F66', 'blush': '#F4968A',
    'eye': '#1A2338', 'eye_shine': '#FFFFFF', 'brow': '#2A2F45',
    'mouth': '#B5623F',
    'coat': '#F2F5FB', 'coat_sh': '#C3CCDF',
    'shirt': '#2F6DB5', 'shirt_sh': '#1F4A80',
    'pants': '#2B3A5C', 'pants_sh': '#1C2640',
    'shoe': '#2A2A33', 'shoe_hi': '#55566A',
}


def ellipse(cx, cy, rx, ry):
    return ((X - cx) / rx) ** 2 + ((Y - cy) / ry) ** 2 <= 1.0


def rect(x0, y0, x1, y1):
    """Retângulo com coordenadas inclusivas de pixel."""
    return (X >= x0) & (X <= x1) & (Y >= y0) & (Y <= y1)


def draw_doctor(upper_dy=0, leg_dx_left=0, leg_dx_right=0,
                foot_lift_left=0, foot_lift_right=0,
                arm_dy_left=0, arm_dy_right=0):
    """Monta um frame. Os parâmetros deslocam partes para criar animações.

    upper_dy: deslocamento vertical da parte de cima (respiração/bob).
    leg_dx_*: deslocamento horizontal de cada perna (passos).
    foot_lift_*: quantos pixels o pé sobe (passo no ar).
    arm_dy_*: deslocamento vertical de cada braço (balanço).
    """
    # --- Máscaras das partes ---
    face = ellipse(12, 12.0, 6.4, 5.6)
    # Cabelo cacheado: um corpo central mais três bolhas no topo.
    hair_body = ellipse(12, 9.0, 8.4, 6.2)
    hair_curls = (ellipse(6.4, 6.0, 2.6, 2.6) | ellipse(9.4, 4.6, 2.8, 2.6)
                  | ellipse(12.4, 3.9, 2.8, 2.6) | ellipse(15.4, 4.6, 2.8, 2.6)
                  | ellipse(17.6, 6.0, 2.6, 2.6))
    hair_outer = hair_body | hair_curls
    hair = hair_outer & ((Y < 10.6) | (np.abs(X - 12) > 6.6))

    body = rect(5, 16, 18, 25)
    # Gola em V: a camisa azul aparece entre as lapelas e se estreita para baixo.
    shirt = (Y >= 16) & (Y <= 21) & (np.abs(X - 12) <= (Y - 15.0) * 0.55)
    lapel_l = rect(8, 16, 9, 22)
    lapel_r = rect(14, 16, 15, 22)

    arm_l = shift(rect(3, 16, 5, 23), 0, arm_dy_left)
    arm_r = shift(rect(18, 16, 20, 23), 0, arm_dy_right)
    hand_l = shift(rect(3, 23, 5, 24), 0, arm_dy_left)
    hand_r = shift(rect(18, 23, 20, 24), 0, arm_dy_right)

    pants_l = shift(rect(8, 25, 11, 29), leg_dx_left, 0)
    pants_r = shift(rect(12, 25, 15, 29), leg_dx_right, 0)
    shoe_l = shift(rect(7, 29, 11, 31), leg_dx_left, -foot_lift_left)
    shoe_r = shift(rect(12, 29, 16, 31), leg_dx_right, -foot_lift_right)

    # A parte de cima (cabeça, corpo e braços) recebe o mesmo deslocamento.
    upper = [face, hair, body, shirt, lapel_l, lapel_r, arm_l, arm_r, hand_l, hand_r]
    upper = [shift(m, 0, upper_dy) for m in upper]
    face, hair, body, shirt, lapel_l, lapel_r, arm_l, arm_r, hand_l, hand_r = upper

    eye_l = shift(rect(8, 12, 9, 14), 0, upper_dy)
    eye_r = shift(rect(14, 12, 15, 14), 0, upper_dy)
    brow_l = shift(rect(8, 11, 9, 11), 0, upper_dy)
    brow_r = shift(rect(14, 11, 15, 11), 0, upper_dy)
    mouth = shift(rect(11, 16, 12, 16), 0, upper_dy)
    blush_l = shift(rect(6, 15, 6, 15), 0, upper_dy)
    blush_r = shift(rect(17, 15, 17, 15), 0, upper_dy)

    # --- Silhueta e contorno ---
    layers = [pants_l, pants_r, shoe_l, shoe_r, body, arm_l, arm_r,
              hand_l, hand_r, face, hair]
    silhouette = np.zeros((H, W), dtype=bool)
    for m in layers:
        silhouette |= m

    img = np.zeros((H, W, 4), dtype=np.uint8)

    def paint(mask, color_key):
        img[mask] = hex_to_rgba(C[color_key])

    # Cores de baixo para cima: o que aparece na frente vem por último.
    paint(outline(silhouette), 'outline')
    paint(pants_l | pants_r, 'pants')
    paint((pants_l | pants_r) & (Y >= 28), 'pants_sh')
    paint(shoe_l | shoe_r, 'shoe')
    paint((shoe_l | shoe_r) & (Y <= 29.5), 'shoe_hi')
    paint(body, 'coat')
    paint(body & (Y >= 23), 'coat_sh')
    paint(shirt, 'shirt')
    paint(shirt & (X >= 12.5), 'shirt_sh')
    paint(shirt & (Y >= 21), 'shirt_sh')
    paint(lapel_l | lapel_r, 'coat_sh')
    paint(arm_l | arm_r, 'coat')
    paint((arm_l & (X >= 4.5)) | (arm_r & (X <= 19.5)), 'coat_sh')
    paint(hand_l | hand_r, 'skin')
    paint(face, 'skin')
    paint(face & (Y >= 16), 'skin_sh')

    # Cabelo: luz no alto, sombra nas laterais e na base.
    hair_light = ellipse(9.0, 4.4, 3.4, 2.2)
    hair_shadow = (X > 17.6) | (Y > 12.8) | ((X < 6.6) & (Y > 8.6))
    paint(hair, 'hair')
    paint(hair & hair_light, 'hair_hi')
    paint(hair & hair_shadow, 'hair_sh')

    paint(eye_l | eye_r, 'eye')
    paint(brow_l | brow_r, 'brow')
    paint(mouth, 'mouth')
    paint(blush_l | blush_r, 'blush')

    # Brilho nos olhos (um pixel branco no canto superior de cada olho).
    shine_l = shift(rect(8, 12, 8, 12), 0, upper_dy) & eye_l
    shine_r = shift(rect(14, 12, 14, 12), 0, upper_dy) & eye_r
    paint(shine_l | shine_r, 'eye_shine')

    # Pixels com alpha 0 não devem carregar cor.
    img[img[:, :, 3] == 0] = 0
    return img


def squash(img, scale, anchor_bottom=True):
    """Achata verticalmente o sprite (morte), mantendo a base fixa."""
    h, w = img.shape[:2]
    out = np.zeros_like(img)
    for y in range(h):
        # Linha de destino y com base no fundo: mapeamento inverso.
        src_from_bottom = (h - 1 - y) / scale
        src = int(round((h - 1) - src_from_bottom))
        if 0 <= src < h:
            out[y] = img[src]
    return out


def build_frames():
    frames = {}

    # Idle: respiração suave, 4 frames (sobe e desce 1 px).
    idle_dy = [0, -1, -1, 0]
    frames['Idle'] = [
        draw_doctor(upper_dy=dy, arm_dy_left=dy * 0, arm_dy_right=dy * 0)
        for dy in idle_dy
    ]

    # Andar: 6 frames de um ciclo completo de passos.
    # Pernas alternam horizontalmente; o pé levanta na fase de passo.
    walk = []
    steps = 6
    for i in range(steps):
        t = 2 * np.pi * i / steps
        leg = int(round(1.2 * np.sin(t)))
        lift_left = 1 if np.sin(t) > 0.5 else 0
        lift_right = 1 if np.sin(t) < -0.5 else 0
        bob = -1 if abs(np.sin(t)) > 0.6 else 0
        arm = int(round(np.sin(t) * 0.8))
        walk.append(draw_doctor(
            upper_dy=bob,
            leg_dx_left=leg, leg_dx_right=-leg,
            foot_lift_left=lift_left, foot_lift_right=lift_right,
            arm_dy_left=-arm, arm_dy_right=arm,
        ))
    frames['Walk'] = walk

    # Morte: 6 frames. O corpo achata (cai) e some aos poucos com dither.
    base = draw_doctor()
    scales = [1.0, 0.9, 0.78, 0.62, 0.45, 0.32]
    keeps = [1.0, 1.0, 1.0, 0.8, 0.45, 0.15]
    death = []
    for s, k in zip(scales, keeps):
        f = squash(base, s)
        f = dither_fade(f, k)
        death.append(f)
    frames['Death'] = death
    return frames


if __name__ == '__main__':
    import sys
    from common import save_scaled
    out_dir = sys.argv[1] if len(sys.argv) > 1 else '/home/claude/art/out/doctor'
    import os
    frames = build_frames()
    for name, seq in frames.items():
        folder = os.path.join(out_dir, name)
        os.makedirs(folder, exist_ok=True)
        for i, f in enumerate(seq):
            save_scaled(f, os.path.join(folder, f'frame_{i:02d}.png'), 3)
        print(name, len(seq))
