"""Funções auxiliares para desenhar pixel art por regras geométricas.

Cada pixel é testado pelo centro (x + 0.5, y + 0.5), o que mantém formas
limpas e sem anti-aliasing, como na pixel art tradicional.
"""
import numpy as np
from PIL import Image

BAYER_4 = np.array([
    [0, 8, 2, 10],
    [12, 4, 14, 6],
    [3, 11, 1, 9],
    [15, 7, 13, 5],
]) / 16.0


def hex_to_rgba(hex_color, alpha=255):
    h = hex_color.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), alpha)


def grid(width, height):
    """Coordenadas do centro de cada pixel."""
    ys, xs = np.mgrid[0:height, 0:width]
    return xs + 0.5, ys + 0.5


def shift(mask, dx=0, dy=0):
    """Desloca uma máscara booleana sem enrolar as bordas."""
    if dx == 0 and dy == 0:
        return mask
    out = np.zeros_like(mask)
    h, w = mask.shape
    ys = slice(max(0, dy), min(h, h + dy))
    yd = slice(max(0, -dy), min(h, h - dy))
    xs = slice(max(0, dx), min(w, w + dx))
    xd = slice(max(0, -dx), min(w, w - dx))
    out[ys, xs] = mask[yd, xd]
    return out


def outline(silhouette):
    """Marca como contorno os pixels vazios encostados (4 direções) na silhueta."""
    padded = np.pad(silhouette, 1)
    neighbors = (padded[:-2, 1:-1] | padded[2:, 1:-1] |
                 padded[1:-1, :-2] | padded[1:-1, 2:])
    return neighbors & ~silhouette


def dither_fade(image_array, keep):
    """Remove pixels com padrão Bayer para simular transparência em pixel art.

    keep = 1.0 mantém tudo; keep = 0.0 remove tudo.
    """
    h, w = image_array.shape[:2]
    threshold = np.tile(BAYER_4, (h // 4 + 1, w // 4 + 1))[:h, :w]
    result = image_array.copy()
    if keep >= 1.0:
        return result
    remove = threshold >= keep
    result[remove, 3] = 0
    return result


def save_scaled(image_array, path, scale):
    """Salva a imagem RGBA ampliada com vizinho mais próximo (pixel perfeito)."""
    img = Image.fromarray(image_array, 'RGBA')
    img = img.resize((img.width * scale, img.height * scale), Image.NEAREST)
    img.save(path)
    return img
