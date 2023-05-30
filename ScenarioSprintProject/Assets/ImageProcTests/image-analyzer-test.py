import os
import cv2
import imutils
import socket
import numpy as np
from skimage.metrics import structural_similarity as compare_ssim


def flaw_analysis(base, flawed):
    # Source: https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python/
    gray_a = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    gray_b = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    (score, diff) = compare_ssim(gray_a, gray_b, full=True)
    diff = (diff * 255).astype("uint8")

    thresh = cv2.threshold(diff, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
    contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    contours = imutils.grab_contours(contours)

    for c in contours:
        (x, y, w, h) = cv2.boundingRect(c)
        cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)

        # If the contour covers more than 20% of the image, there is a chance there is a color inconsistency
        if w * h > flawed.shape[0] * flawed.shape[1] * 0.2:
            print("Very large contour detected. Color consistency may be off")

    cv2.imwrite(f'Images/contours{i}.png', flawed)
    cv2.imwrite(f'Images/diff{i}.png', diff)
    cv2.imwrite(f'Images/thresh{i}.png', thresh)

    return score


def color_consistency_analysis(base, flawed):
    gray_a = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    gray_b = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    (score, diff) = compare_ssim(gray_a, gray_b, full=True)
    diff = (diff * 255).astype("uint8")

    # Checking for an overall color change
    rev = 255-diff
    gray_sum = np.sum(rev)
    score = gray_sum/(rev.shape[0]*rev.shape[1])

    return score


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

prefixed = [filename for filename in os.listdir('Images') if filename.startswith("base") & filename.endswith(".png")]
total_images = len(prefixed)
total_ssim_score = 0
total_color_score = 0

for i in range(0, total_images):
    base_image = cv2.imread(f'Images/base{i}.png')
    flawed_image = cv2.imread(f'Images/flawed{i}.png')

    # Resize the flawed image to match the base
    if flawed_image.shape != base_image.shape:
        flawed_image = cv2.resize(flawed_image, (base_image.shape[1], base_image.shape[0]))

    ssim_score = flaw_analysis(base_image, flawed_image)
    total_ssim_score += ssim_score

    color_change_metric = color_consistency_analysis(base_image, flawed_image)
    total_color_score += color_change_metric

print("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images, total_color_score / total_images))
sock.sendto(("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images, total_color_score / total_images)).encode(), (UDP_IP, UDP_PORT))
